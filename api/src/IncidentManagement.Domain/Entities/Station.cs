namespace IncidentManagement.Domain.Entities;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int AgencyId { get; set; }
    public Agency Agency { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    
    public ICollection<Personnel> Personnel { get; set; } = new List<Personnel>();
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}