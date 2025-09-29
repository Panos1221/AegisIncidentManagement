using System.Text.Json.Serialization;

namespace IncidentManagement.Application.DTOs;

public class PoliceStationGeoJsonDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("features")]
    public List<PoliceStationFeatureDto> Features { get; set; } = new();
}

public class PoliceStationFeatureDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("properties")]
    public PoliceStationPropertiesDto Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public PoliceStationGeometryDto Geometry { get; set; } = new();
}

public class PoliceStationPropertiesDto
{
    [JsonPropertyName("gid")]
    public int Gid { get; set; }

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("sinoikia")]
    public string Sinoikia { get; set; } = "";

    [JsonPropertyName("diam")]
    public string? Diam { get; set; }
}

public class PoliceStationGeometryDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("coordinates")]
    public List<List<double>> Coordinates { get; set; } = new();
}

public class PoliceStationDto
{
    public int Id { get; set; }
    public int Gid { get; set; }
    public int OriginalId { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Sinoikia { get; set; } = "";
    public string? Diam { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}