namespace Guardian.Api.Models;

// История одного действия посетителя.
public class VisitorAction
{
    public int Id { get; set; }
    public int VisitorId { get; set; }
    public string Action { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Visitor Visitor { get; set; } = null!;
}
