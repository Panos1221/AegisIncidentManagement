namespace IncidentManagement.Domain.Entities;

public class FireHydrant
{
    public int Id { get; set; }
    public string ExternalId { get; set; } = ""; // The @id from GeoJSON (e.g., "node/391792487")
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Position { get; set; } // fire_hydrant:position (e.g., "sidewalk")
    public string? Type { get; set; } // fire_hydrant:type (e.g., "pillar")
    public string? AdditionalProperties { get; set; } // JSON string for any other properties
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}