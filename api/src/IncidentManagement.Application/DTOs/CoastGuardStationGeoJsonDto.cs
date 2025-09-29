using System.Text.Json.Serialization;

namespace IncidentManagement.Application.DTOs;

public class CoastGuardStationGeoJsonDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("features")]
    public List<CoastGuardStationFeatureDto> Features { get; set; } = new();
}

public class CoastGuardStationFeatureDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("properties")]
    public CoastGuardStationPropertiesDto Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public CoastGuardStationGeometryDto Geometry { get; set; } = new();
}

public class CoastGuardStationPropertiesDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("name_gr")]
    public string NameGr { get; set; } = "";

    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("area")]
    public string Area { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("telephone")]
    public string? Telephone { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}

public class CoastGuardStationGeometryDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; set; } = new();
}

public class CoastGuardStationDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string NameGr { get; set; } = "";
    public string Address { get; set; } = "";
    public string Area { get; set; } = "";
    public string Type { get; set; } = "";
    public string? Telephone { get; set; }
    public string? Email { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}