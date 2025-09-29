namespace IncidentManagement.Domain.Entities;

public class IncidentInvolvement
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }

    // Personnel section
    public int? FireTrucksNumber { get; set; }
    public int? FirePersonnel { get; set; }

    // Other agencies
    public string? OtherAgencies { get; set; }

    // Service actions
    public string? ServiceActions { get; set; }

    // Rescues
    public int? RescuedPeople { get; set; }
    public string? RescueInformation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}