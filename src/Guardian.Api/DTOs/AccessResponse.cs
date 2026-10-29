namespace Guardian.Api.DTOs;

// Результат проверки доступа в замок.
public record AccessResponse(int VisitorId, int Trust, bool Allowed, string Message);
