namespace IncidentManagement.Domain.Entities;

public class StationBoundary
{
    public int Id { get; set; }
    public int FireStationId { get; set; }
    public string CoordinatesJson { get; set; } = ""; // Polygon coordinates as JSON
    
    // Navigation properties
    public FireStation FireStation { get; set; } = null!;
}