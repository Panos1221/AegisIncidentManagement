namespace AegisDispatcher.Models
{
    public class Incident
    {
        public int Id { get; set; }
        public int StationId { get; set; }
        public int AgencyId { get; set; }
        public string MainCategory { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;
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
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsClosed { get; set; }
        public IncidentClosureReason? ClosureReason { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int? ClosedByUserId { get; set; }
        public string? ClosedByName { get; set; }
        public List<Assignment> Assignments { get; set; } = new();
        public List<IncidentLog> Logs { get; set; } = new();
        public List<Caller> Callers { get; set; } = new();
        public string? ParticipationType { get; set; }
    }
}
