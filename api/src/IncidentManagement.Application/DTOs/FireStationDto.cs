namespace IncidentManagement.Application.DTOs;

public class FireStationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public double Area { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string GeometryJson { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public class FireStationBoundaryDto
{
    public int Id { get; set; }
    public int FireStationId { get; set; }
    public string Name { get; set; } = "";
    public string Region { get; set; } = "";
    public double Area { get; set; }
    public List<List<List<double>>> Coordinates { get; set; } = new();
}

public class LocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class FindStationByLocationDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}