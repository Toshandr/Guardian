namespace Guardian.Api.DTOs;

// Информация о посетителе вместе с его историей.
public record VisitorResponse(int Id, int Trust, List<VisitorActionItem> Actions);

public record VisitorActionItem(string Action, DateTime CreatedAt);
