namespace IncidentManagement.Domain.Entities;

public class Injury
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "firefighter" or "civilian"
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}