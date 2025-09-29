namespace AegisDispatcher.Models
{
    public class StationAssignmentRequest
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string AgencyType { get; set; } = string.Empty;
    }
}
