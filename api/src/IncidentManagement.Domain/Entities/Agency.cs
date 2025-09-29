using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Agency
{
    public int Id { get; set; }
    public AgencyType Type { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Station> Stations { get; set; } = new List<Station>();
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}