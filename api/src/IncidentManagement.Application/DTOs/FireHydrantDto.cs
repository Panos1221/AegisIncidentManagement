namespace IncidentManagement.Application.DTOs;

public class FireHydrantDto
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Position { get; set; }
    public string? Type { get; set; }
    public Dictionary<string, object>? AdditionalProperties { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}