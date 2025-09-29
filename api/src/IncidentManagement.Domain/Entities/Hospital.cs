namespace IncidentManagement.Domain.Entities;

public class Hospital
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string AgencyCode { get; set; } = ""; // EKAB for hospitals
    public DateTime CreatedAt { get; set; }
}