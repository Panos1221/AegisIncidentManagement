using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public class IncidentDto
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public int StationId { get; set; }
    public string MainCategory { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string? Address { get; set; }
    public string? Street { get; set; }
    public string? StreetNumber { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IncidentStatus Status { get; set; }
    public IncidentPriority Priority { get; set; }
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    
    // Closure properties
    public bool IsClosed { get; set; }
    public IncidentClosureReason? ClosureReason { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserId { get; set; }
    public string? ClosedByName { get; set; }
    
    public List<AssignmentDto> Assignments { get; set; } = new();
    public List<IncidentLogDto> Logs { get; set; } = new();
    public List<CallerDto> Callers { get; set; } = new();

    // New detailed incident information
    public IncidentInvolvementDto? Involvement { get; set; }
    public List<IncidentCommanderDto> Commanders { get; set; } = new();
    public List<InjuryDto> Injuries { get; set; } = new();
    public List<DeathDto> Deaths { get; set; } = new();
    public IncidentFireDto? Fire { get; set; }
    public IncidentDamageDto? Damage { get; set; }

    public string? ParticipationType { get; set; }
}

public class CreateIncidentDto
{
    public int StationId { get; set; }
    public string MainCategory { get; set; } = "";
    public string SubCategory { get; set; } = "";
    public string? Address { get; set; }
    public string? Street { get; set; }
    public string? StreetNumber { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public IncidentPriority Priority { get; set; } = IncidentPriority.Normal;
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public List<CreateCallerDto> Callers { get; set; } = new();
}

public class UpdateIncidentStatusDto
{
    public IncidentStatus Status { get; set; }
}

public class CloseIncidentDto
{
    public IncidentClosureReason ClosureReason { get; set; }
    public int ClosedByUserId { get; set; }
}