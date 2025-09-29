namespace IncidentManagement.Application.DTOs;

public class VehicleAssignmentDto
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleCallsign { get; set; } = "";
    public int PersonnelId { get; set; }
    public string PersonnelName { get; set; } = "";
    public string PersonnelRank { get; set; } = "";
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; }
}

public class CreateVehicleAssignmentDto
{
    public int VehicleId { get; set; }
    public int PersonnelId { get; set; }
}