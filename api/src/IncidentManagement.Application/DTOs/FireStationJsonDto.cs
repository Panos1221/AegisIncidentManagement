using System.Text.Json;
using System.Text.Json.Serialization;

namespace IncidentManagement.Application.DTOs;

public class FireStationFeatureCollection
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("features")]
    public List<FireStationFeature> Features { get; set; } = new();
}

public class FireStationFeature
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("geometry")]
    public FireStationGeometry Geometry { get; set; } = new();
    
    [JsonPropertyName("properties")]
    public FireStationProperties Properties { get; set; } = new();
}

public class FireStationGeometry
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("coordinates")]
    public JsonElement Coordinates { get; set; }
}

public class FireStationProperties
{
    [JsonPropertyName("PYR_YPIRES")]
    public string StationName { get; set; } = "";
    
    [JsonPropertyName("FIRST_PER_")]
    public string Region { get; set; } = "";
    
    [JsonPropertyName("FIRST_NOM_")]
    public string Code { get; set; } = "";
    
    [JsonPropertyName("Area")]
    public JsonElement Area { get; set; }
}