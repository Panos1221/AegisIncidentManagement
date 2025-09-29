namespace AegisDispatcher.Models
{
    public class PatrolZone
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AgencyId { get; set; }
        public int? StationId { get; set; }
        public string Coordinates { get; set; } = string.Empty; // GeoJSON coordinates
        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public string? AgencyName { get; set; }
        public string? StationName { get; set; }
        public string? CreatedByUserName { get; set; }
        public List<PatrolZoneVehicleAssignment> VehicleAssignments { get; set; } = new();
    }

    public class PatrolZoneVehicleAssignment
    {
        public int Id { get; set; }
        public int PatrolZoneId { get; set; }
        public int VehicleId { get; set; }
        public DateTime AssignedAt { get; set; }
        public int AssignedByUserId { get; set; }
        public bool IsActive { get; set; }
        
        // Navigation properties
        public string? VehicleCallsign { get; set; }
        public string? VehicleType { get; set; }
        public string? AssignedByUserName { get; set; }
    }

    public class CreatePatrolZone
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int AgencyId { get; set; }
        public int? StationId { get; set; }
        public string Coordinates { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
    }
}