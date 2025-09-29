namespace IncidentManagement.Domain.Entities;

public class FireStation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public double Area { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string GeometryJson { get; set; } = ""; // GeoJSON polygon for boundaries
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<StationBoundary> Boundaries { get; set; } = new List<StationBoundary>();
}