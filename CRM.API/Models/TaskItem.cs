namespace CRM.API.Models;

public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int SalesRepId { get; set; }
    public int? CustomerId { get; set; }      // ← only new line
    public SalesRep? SalesRep { get; set; }
}