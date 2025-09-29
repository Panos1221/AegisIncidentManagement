using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class ShiftAssignment
{
    public int Id { get; set; }
    public int ShiftInstanceId { get; set; }
    public ShiftInstance? ShiftInstance { get; set; }
    public ShiftRole Role { get; set; }
    public int? PersonnelId { get; set; }
    public int? VehicleId { get; set; }
}