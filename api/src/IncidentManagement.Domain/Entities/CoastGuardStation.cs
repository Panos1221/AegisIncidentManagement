namespace IncidentManagement.Domain.Entities;

public class CoastGuardStation
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string NameGr { get; set; } = "";
    public string Address { get; set; } = "";
    public string Area { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}