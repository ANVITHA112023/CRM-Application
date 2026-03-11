namespace CRM.API.Models;

public class SalesRep
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public string? ProfilePicture { get; set; }   // Optional
    public bool IsAvailable { get; set; } = true;

    // Navigation property – one SR has many Tasks
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}