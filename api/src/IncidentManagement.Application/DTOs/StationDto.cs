namespace IncidentManagement.Application.DTOs;

public class StationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}