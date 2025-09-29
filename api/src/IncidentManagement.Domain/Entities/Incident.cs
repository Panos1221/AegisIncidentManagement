using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Incident
{
    public int Id { get; set; }
    public int AgencyId { get; set; }
    public Agency? Agency { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
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
    public IncidentStatus Status { get; set; } = IncidentStatus.Created;
    public IncidentPriority Priority { get; set; } = IncidentPriority.Normal;
    public string? Notes { get; set; }
    public int CreatedByUserId { get; set; }
    public User? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Closure properties
    public bool IsClosed { get; set; } = false;
    public IncidentClosureReason? ClosureReason { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserId { get; set; }
    public User? ClosedBy { get; set; }
    
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<IncidentLog> Logs { get; set; } = new List<IncidentLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Caller> Callers { get; set; } = new List<Caller>();

    // New detailed incident information
    public IncidentInvolvement? Involvement { get; set; }
    public ICollection<IncidentCommander> Commanders { get; set; } = new List<IncidentCommander>();
    public ICollection<Injury> Injuries { get; set; } = new List<Injury>();
    public ICollection<Death> Deaths { get; set; } = new List<Death>();
    public IncidentFire? Fire { get; set; }
    public IncidentDamage? Damage { get; set; }
}