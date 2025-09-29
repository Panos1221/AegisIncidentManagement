namespace IncidentManagement.Domain.Entities;

public class Personnel
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
    public int AgencyId { get; set; }
    public Agency Agency { get; set; } = null!;
    public int? UserId { get; set; }
    public User? User { get; set; }
    public string Name { get; set; } = "";
    public string Rank { get; set; } = "";
    public string? BadgeNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<VehicleAssignment> VehicleAssignments { get; set; } = new List<VehicleAssignment>();
}