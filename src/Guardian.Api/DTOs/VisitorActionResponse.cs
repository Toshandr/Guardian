namespace Guardian.Api.DTOs;

// Результат выполненного действия.
public record VisitorActionResponse(int VisitorId, string Action, int Trust, string Message);
