namespace CRM.API.Models;

public class ServiceRequest
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string Status { get; set; } = "Ongoing";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Links to Customer record auto-created from homepage form
    public int? CustomerId { get; set; }
}