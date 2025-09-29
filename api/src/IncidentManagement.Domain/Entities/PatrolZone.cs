using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Domain.Entities;

public class PatrolZone
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = "";
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int AgencyId { get; set; }
    public Agency Agency { get; set; } = null!;
    
    public int StationId { get; set; }
    public Station Station { get; set; } = null!;
    
    /// <summary>
    /// GeoJSON polygon coordinates representing the patrol zone boundaries
    /// Format: [[[lng, lat], [lng, lat], ...]]
    /// </summary>
    [Required]
    public string BoundaryCoordinates { get; set; } = "";
    
    /// <summary>
    /// Center point of the patrol zone for display purposes
    /// </summary>
    public double CenterLatitude { get; set; }
    public double CenterLongitude { get; set; }
    
    /// <summary>
    /// Priority level for patrol zone (1 = High, 2 = Medium, 3 = Low)
    /// </summary>
    public int Priority { get; set; } = 2;
    
    /// <summary>
    /// Whether this patrol zone is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Color code for displaying the zone on the map (hex format)
    /// </summary>
    [MaxLength(7)]
    public string? Color { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;
    
    // Navigation properties
    public ICollection<PatrolZoneAssignment> VehicleAssignments { get; set; } = new List<PatrolZoneAssignment>();
}