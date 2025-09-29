namespace IncidentManagement.Application.DTOs;

public class StationAssignmentRequestDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string AgencyType { get; set; } = string.Empty; // "Fire", "CoastGuard", "Police", "Hospital"
}

public class StationAssignmentResponseDto
{
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public string AssignmentMethod { get; set; } = string.Empty; // "District", "Nearest"
    public double Distance { get; set; } // Distance in meters (for nearest assignments)
    public string DistrictName { get; set; } = string.Empty; // For district-based assignments
}

public class FireDistrictFeature
{
    public string Type { get; set; } = "Feature";
    public FireDistrictGeometry Geometry { get; set; } = new();
    public FireDistrictProperties Properties { get; set; } = new();
}

public class FireDistrictGeometry
{
    public string Type { get; set; } = "Polygon";
    public object Coordinates { get; set; } = Array.Empty<double[][]>();
}

public class FireDistrictProperties
{
    public string PYR_YPIRES { get; set; } = string.Empty; // Station name
    public string FIRST_PER_ { get; set; } = string.Empty;
    public string FIRST_NOM_ { get; set; } = string.Empty;
    public double Area { get; set; }
}

public class FireDistrictsGeoJson
{
    public string Type { get; set; } = "FeatureCollection";
    public List<FireDistrictFeature> Features { get; set; } = new();
}