using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Domain.Entities;

public class Assignment
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }
    public ResourceType ResourceType { get; set; }
    public int ResourceId { get; set; }
    public string Status { get; set; } = "Assigned";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Time tracking fields
    public DateTime? DispatchedAt { get; set; }
    public DateTime? EnRouteAt { get; set; }
    public DateTime? OnSceneAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}