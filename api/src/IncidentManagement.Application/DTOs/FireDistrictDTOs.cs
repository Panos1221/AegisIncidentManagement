namespace IncidentManagement.Application.DTOs
{
    public class FindDistrictByLocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class ResponsibleDistrictDto
    {
        public bool Found { get; set; }
        public int? StationId { get; set; }
        public string? StationName { get; set; }
        public string? Region { get; set; }
        public double? Area { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class AssignIncidentDto
    {
        public int IncidentId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }

    public class IncidentAssignmentDto
    {
        public bool Success { get; set; }
        public int IncidentId { get; set; }
        public int? AssignedStationId { get; set; }
        public string? AssignedStationName { get; set; }
        public string? Region { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}