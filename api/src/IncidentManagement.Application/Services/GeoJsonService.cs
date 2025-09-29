using System.Text.Json;
using System.Text.Json.Serialization;

namespace IncidentManagement.Application.Services;

public static class GeoJsonService
{
    public static T? DeserializeFeatureCollection<T>(string jsonContent)
    {
        return JsonSerializer.Deserialize<T>(jsonContent);
    }
}

// GeoJSON data models for deserialization
public class GeoJsonFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("features")]
    public List<GeoJsonFeature> Features { get; set; } = new();
}

public class GeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("properties")]
    public Dictionary<string, object>? Properties { get; set; }

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry? Geometry { get; set; }
}

public class GeoJsonGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("coordinates")]
    public double[] Coordinates { get; set; } = Array.Empty<double>();
}

// Fire Station specific GeoJSON models
public class FireStationGeoJsonFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("properties")]
    public FireStationGeoJsonProperties Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public GeoJsonGeometry? Geometry { get; set; }
}

public class FireStationGeoJsonProperties
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("address")]
    public string Address { get; set; } = "";

    [JsonPropertyName("city")]
    public string City { get; set; } = "";

    [JsonPropertyName("region")]
    public string Region { get; set; } = "";
}

public class FireStationGeoJsonFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("features")]
    public List<FireStationGeoJsonFeature> Features { get; set; } = new();
}