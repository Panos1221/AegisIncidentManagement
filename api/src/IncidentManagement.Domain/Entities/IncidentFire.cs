namespace IncidentManagement.Domain.Entities;

public class IncidentFire
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }

    // Burned areas and items
    public string? BurnedArea { get; set; }
    public string? BurnedItems { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}