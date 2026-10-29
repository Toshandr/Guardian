namespace Guardian.Api.Models;

// Посетитель и его текущий уровень доверия.
public class Visitor
{
    public int Id { get; set; }
    public int Trust { get; set; }
    public List<VisitorAction> Actions { get; set; } = [];
}
