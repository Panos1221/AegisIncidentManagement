namespace IncidentManagement.Domain.Entities;

public class PoliceStation
{
    public int Id { get; set; }
    public int Gid { get; set; }
    public int OriginalId { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Sinoikia { get; set; } = "";
    public string? Diam { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}