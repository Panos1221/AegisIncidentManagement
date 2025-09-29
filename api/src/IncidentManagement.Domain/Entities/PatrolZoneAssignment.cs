using System.ComponentModel.DataAnnotations;

namespace IncidentManagement.Domain.Entities;

public class PatrolZoneAssignment
{
    public int Id { get; set; }
    
    public int PatrolZoneId { get; set; }
    public PatrolZone PatrolZone { get; set; } = null!;
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    /// <summary>
    /// When the vehicle was assigned to this patrol zone
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the vehicle assignment ended (null if still active)
    /// </summary>
    public DateTime? UnassignedAt { get; set; }
    
    /// <summary>
    /// Whether this assignment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// User who made the assignment
    /// </summary>
    public int AssignedByUserId { get; set; }
    public User AssignedByUser { get; set; } = null!;
    
    /// <summary>
    /// User who ended the assignment (if applicable)
    /// </summary>
    public int? UnassignedByUserId { get; set; }
    public User? UnassignedByUser { get; set; }
    
    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}