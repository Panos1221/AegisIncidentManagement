namespace IncidentManagement.Domain.Entities;

public class IncidentCommander
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }

    public int PersonnelId { get; set; }
    public Personnel? Personnel { get; set; }

    public string? Observations { get; set; }

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public int AssignedByUserId { get; set; }
    public User? AssignedBy { get; set; }
}