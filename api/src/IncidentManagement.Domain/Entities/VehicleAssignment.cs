namespace IncidentManagement.Domain.Entities;

public class VehicleAssignment
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }
    public int PersonnelId { get; set; }
    public Personnel? Personnel { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; } = true;
}