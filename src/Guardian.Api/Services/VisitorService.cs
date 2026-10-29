using Guardian.Api.Data;
using Guardian.Api.DTOs;
using Guardian.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Guardian.Api.Services;

// Содержит бизнес-логику посетителей
public class VisitorService(GuardianDbContext db, ICacheService cache, IConfiguration configuration,ILogger<VisitorService> logger) : IVisitorService{
    private const string ListKey = "visitors:all";
    private const int AccessTrust = 50;

    public async Task<CreateVisitorResponse> CreateAsync()
    {
        var visitor = new Visitor { Trust = 0 };
        db.Visitors.Add(visitor);
        await db.SaveChangesAsync();

        var response = new CreateVisitorResponse(visitor.Id, visitor.Trust);
        await cache.SetAsync(VisitorKey(visitor.Id), ToResponse(visitor), VisitorSeconds());
        await cache.RemoveAsync(ListKey);
        logger.LogInformation("Создан посетитель {VisitorId}", visitor.Id);
        return response;
    }

    public async Task<List<VisitorResponse>> GetAllAsync()
    {
        var cached = await cache.GetAsync<List<VisitorResponse>>(ListKey);
        if (cached is not null)
            return cached;

        var visitors = await db.Visitors.Include(visitor => visitor.Actions)
            .AsNoTracking().OrderBy(visitor => visitor.Id).ToListAsync();
        var response = visitors.Select(ToResponse).ToList();
        await cache.SetAsync(ListKey, response, ListSeconds());
        return response;
    }

    public async Task<VisitorResponse?> GetByIdAsync(int id)
    {
        var key = VisitorKey(id);
        var cached = await cache.GetAsync<VisitorResponse>(key);
        if (cached is not null)
            return cached;

        var visitor = await db.Visitors.Include(item => item.Actions)
            .AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (visitor is null)
            return null;

        var response = ToResponse(visitor);
        await cache.SetAsync(key, response, VisitorSeconds());
        return response;
    }

    public async Task<VisitorActionResponse?> PerformActionAsync(int id, string action)
    {
        var visitor = await db.Visitors.Include(item => item.Actions)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (visitor is null)
            return null;

        var change = action switch
        {
            "Introduce" => 20,
            "OfferHelp" => 30,
            "Threaten" => -20,
            _ => (int?)null //дефолт если что то не то ввели
        };

        if (change is null)
            throw new ArgumentException("Недопустимое действие. Используйте Introduce, OfferHelp или Threaten.");

        if (visitor.Actions.Any(item => item.Action == action))
            throw new InvalidOperationException("Это действие уже было выполнено.");

        visitor.Trust = Math.Clamp(visitor.Trust + change.Value, 0, 100); //последние два параметра задают отрезок минимума и максимума
        visitor.Actions.Add(new VisitorAction
        {
            Action = action,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var response = new VisitorActionResponse(visitor.Id, action, visitor.Trust, "Действие выполнено.");
        await cache.SetAsync(VisitorKey(id), ToResponse(visitor), VisitorSeconds());
        await cache.RemoveAsync(ListKey);
        await cache.RemoveAsync(AccessKey(id));
        logger.LogInformation("Посетитель {VisitorId} выполнил {Action}; доверие: {Trust}", id, action, visitor.Trust);
        return response;
    }

    public async Task<AccessResponse?> CheckAccessAsync(int id)
    {
        var key = AccessKey(id);
        var cached = await cache.GetAsync<AccessResponse>(key);
        if (cached is not null)
            return cached;

        var visitor = await db.Visitors.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        if (visitor is null)
            return null;

        var allowed = visitor.Trust >= AccessTrust;
        var response = new AccessResponse(
            visitor.Id,
            visitor.Trust,
            allowed,
            allowed ? "Хранитель разрешает проход." : "Хранитель не разрешает проход.");

        await cache.SetAsync(key, response, VisitorSeconds());
        return response;
    }

    private static string VisitorKey(int id) => $"visitor:{id}";
    private static string AccessKey(int id) => $"visitor:{id}:access";
    private int VisitorSeconds() => configuration.GetValue("Cache:VisitorSeconds", 300);
    private int ListSeconds() => configuration.GetValue("Cache:ListSeconds", 60);

    private static VisitorResponse ToResponse(Visitor visitor) =>
        new(visitor.Id, visitor.Trust, visitor.Actions.OrderBy(action => action.CreatedAt).Select(action => new VisitorActionItem(action.Action, action.CreatedAt)).ToList());
}
