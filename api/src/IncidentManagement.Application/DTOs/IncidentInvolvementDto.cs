namespace IncidentManagement.Application.DTOs;

public class IncidentInvolvementDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }

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

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateIncidentInvolvementDto
{
    public int? FireTrucksNumber { get; set; }
    public int? FirePersonnel { get; set; }
    public string? OtherAgencies { get; set; }
    public string? ServiceActions { get; set; }
    public int? RescuedPeople { get; set; }
    public string? RescueInformation { get; set; }
}

public class UpdateIncidentInvolvementDto : CreateIncidentInvolvementDto
{
}