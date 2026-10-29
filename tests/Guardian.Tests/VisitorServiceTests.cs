using Guardian.Api.Data;
using Guardian.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Guardian.Tests;

// Проверяет правила доверия на тестовой базе в памяти.
public class VisitorServiceTests
{
    [Fact]
    public async Task CreateVisitor_StartsWithZeroTrust()
    {
        await using var db = CreateDb();
        var service = CreateService(db);

        var visitor = await service.CreateAsync();

        Assert.Equal(0, visitor.Trust);
        Assert.Equal(1, visitor.Id);
    }

    [Fact]
    public async Task Actions_UpdateTrustAndCannotBeRepeated()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var visitor = await service.CreateAsync();

        var first = await service.PerformActionAsync(visitor.Id, "Introduce");
        var second = await service.PerformActionAsync(visitor.Id, "OfferHelp");

        Assert.Equal(20, first!.Trust);
        Assert.Equal(50, second!.Trust);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PerformActionAsync(visitor.Id, "Introduce"));
    }

    [Fact]
    public async Task AccessIsAllowedAtFiftyTrust()
    {
        await using var db = CreateDb();
        var service = CreateService(db);
        var visitor = await service.CreateAsync();
        await service.PerformActionAsync(visitor.Id, "Introduce");
        await service.PerformActionAsync(visitor.Id, "OfferHelp");

        var access = await service.CheckAccessAsync(visitor.Id);

        Assert.NotNull(access);
        Assert.True(access.Allowed);
        Assert.Equal(50, access.Trust);
    }

    private static GuardianDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<GuardianDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new GuardianDbContext(options);
    }

    private static VisitorService CreateService(GuardianDbContext db)
    {
        var cache = new FakeCacheService();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();
        var logger = Mock.Of<ILogger<VisitorService>>();
        return new VisitorService(db, cache, configuration, logger);
    }

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object> _items = [];

        public Task<T?> GetAsync<T>(string key)
        {
            if (_items.TryGetValue(key, out var value) && value is T result)
                return Task.FromResult<T?>(result);

            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, int seconds)
        {
            _items[key] = value!;
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _items.Remove(key);
            return Task.CompletedTask;
        }
    }
}
