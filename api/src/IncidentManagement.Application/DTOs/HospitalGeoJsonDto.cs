using System.Text.Json.Serialization;

namespace IncidentManagement.Application.DTOs;

public class HospitalGeoJsonDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("features")]
    public List<HospitalFeatureDto> Features { get; set; } = new();
}

public class HospitalFeatureDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("properties")]
    public HospitalPropertiesDto Properties { get; set; } = new();

    [JsonPropertyName("geometry")]
    public HospitalGeometryDto Geometry { get; set; } = new();
}

public class HospitalPropertiesDto
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

public class HospitalGeometryDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("coordinates")]
    public List<double> Coordinates { get; set; } = new();
}

public class HospitalDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string City { get; set; } = "";
    public string Region { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime CreatedAt { get; set; }
}