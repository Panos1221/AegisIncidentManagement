namespace AegisDispatcher.Models
{
    public class CreateIncident
    {
        public int StationId { get; set; }
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
        public IncidentPriority Priority { get; set; }
        public string? Notes { get; set; }
        public int CreatedByUserId { get; set; }
        public List<CallerDto>? Callers { get; set; }
    }
}
