using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public class AssignmentDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public ResourceType ResourceType { get; set; }
    public int ResourceId { get; set; }
    public string Status { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    
    // Time tracking fields
    public DateTime? DispatchedAt { get; set; }
    public DateTime? EnRouteAt { get; set; }
    public DateTime? OnSceneAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class CreateAssignmentDto
{
    public ResourceType ResourceType { get; set; }
    public int ResourceId { get; set; }
}

public class UpdateAssignmentStatusDto
{
    public string Status { get; set; } = "";
}