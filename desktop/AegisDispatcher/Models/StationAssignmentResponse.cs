namespace AegisDispatcher.Models
{
    public class StationAssignmentResponse
    {
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string AssignmentMethod { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string? DistrictName { get; set; }
    }
}
