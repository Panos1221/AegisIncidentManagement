using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Application.DTOs;

public class PatrolZoneDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public int AgencyId { get; set; }
    public string AgencyName { get; set; } = "";
    public int StationId { get; set; }
    public string StationName { get; set; } = "";
    public string BoundaryCoordinates { get; set; } = "";
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public string? Color { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedByUserId { get; set; }
    public string CreatedByUserName { get; set; } = "";
    public List<PatrolZoneAssignmentDto> VehicleAssignments { get; set; } = new();
}

public class CreatePatrolZoneDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    [Required]
    public int StationId { get; set; }
    
    /// <summary>
    /// GeoJSON polygon coordinates representing the patrol zone boundaries
    /// Format: [[[lng, lat], [lng, lat], ...]]
    /// </summary>
    [Required]
    public string BoundaryCoordinates { get; set; } = "";
    
    /// <summary>
    /// Center point of the patrol zone for display purposes
    /// </summary>
    [Required]
    public double CenterLatitude { get; set; }
    
    [Required]
    public double CenterLongitude { get; set; }
    
    /// <summary>
    /// Priority level for patrol zone (1 = High, 2 = Medium, 3 = Low)
    /// </summary>
    [Range(1, 3)]
    public int Priority { get; set; } = 2;
    
    /// <summary>
    /// Color code for displaying the zone on the map (hex format)
    /// </summary>
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000)")]
    public string? Color { get; set; }
}

public class UpdatePatrolZoneDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public string? BoundaryCoordinates { get; set; }
    
    public double? CenterLatitude { get; set; }
    
    public double? CenterLongitude { get; set; }
    
    [Range(1, 3)]
    public int? Priority { get; set; }
    
    public bool? IsActive { get; set; }
    
    [MaxLength(7)]
    [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex color code (e.g., #FF0000)")]
    public string? Color { get; set; }
}

public class PatrolZoneAssignmentDto
{
    public int Id { get; set; }
    public int PatrolZoneId { get; set; }
    public string PatrolZoneName { get; set; } = "";
    public int VehicleId { get; set; }
    public string VehicleCallsign { get; set; } = "";
    public string VehicleType { get; set; } = "";
    public DateTime AssignedAt { get; set; }
    public DateTime? UnassignedAt { get; set; }
    public bool IsActive { get; set; }
    public int AssignedByUserId { get; set; }
    public string AssignedByUserName { get; set; } = "";
    public int? UnassignedByUserId { get; set; }
    public string? UnassignedByUserName { get; set; }
    public string? Notes { get; set; }
}

public class CreatePatrolZoneAssignmentDto
{
    [Required]
    public int PatrolZoneId { get; set; }
    
    [Required]
    public int VehicleId { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class UpdatePatrolZoneAssignmentDto
{
    [MaxLength(500)]
    public string? Notes { get; set; }
    
    public bool? IsActive { get; set; }
}