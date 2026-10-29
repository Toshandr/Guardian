using Guardian.Api.DTOs;

namespace Guardian.Api.Services;

// Описывает операции с посетителями для контроллера.
public interface IVisitorService
{
    Task<CreateVisitorResponse> CreateAsync();
    Task<List<VisitorResponse>> GetAllAsync();
    Task<VisitorResponse?> GetByIdAsync(int id);
    Task<VisitorActionResponse?> PerformActionAsync(int id, string action);
    Task<AccessResponse?> CheckAccessAsync(int id);
}
