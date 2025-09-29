using IncidentManagement.Application.Configuration;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using GeoAPI.CoordinateSystems;
using GeoAPI.CoordinateSystems.Transformations;

namespace IncidentManagement.Application.Services;

public class StationAssignmentService : IStationAssignmentService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<StationAssignmentService> _logger;
    private readonly FireDistrictDataOptions _fireDistrictOptions;
    private static FireDistrictsGeoJson? _fireDistricts;
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    
    // Coordinate transformation objects
    private static readonly ICoordinateSystem WGS84 = GeographicCoordinateSystem.WGS84;
    private static readonly ICoordinateSystem GreekGrid = CreateGreekGridCoordinateSystem();
    private static readonly ICoordinateTransformation WGS84ToGreekGrid = CreateTransformation();

    public StationAssignmentService(
        IncidentManagementDbContext context,
        ILogger<StationAssignmentService> logger,
        IOptions<FireDistrictDataOptions> fireDistrictOptions)
    {
        _context = context;
        _logger = logger;
        _fireDistrictOptions = fireDistrictOptions.Value;
    }

    public async Task<StationAssignmentResponseDto?> FindStationByLocationAsync(StationAssignmentRequestDto request)
    {
        try
        {
            return request.AgencyType.ToLowerInvariant() switch
            {
                "fire" => await FindFireStationByLocationInternalAsync(request.Latitude, request.Longitude),
                "coastguard" => await FindNearestCoastGuardStationInternalAsync(request.Latitude, request.Longitude),
                "police" => await FindNearestPoliceStationInternalAsync(request.Latitude, request.Longitude),
                "hospital" => await FindNearestHospitalInternalAsync(request.Latitude, request.Longitude),
                _ => throw new ArgumentException($"Unsupported agency type: {request.AgencyType}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding station for location {Latitude}, {Longitude}, agency {Agency}",
                request.Latitude, request.Longitude, request.AgencyType);
            return null;
        }
    }

    public async Task<FireStation?> FindFireStationByLocationAsync(double latitude, double longitude)
    {
        var result = await FindFireStationByLocationInternalAsync(latitude, longitude);
        if (result == null) return null;

        return await _context.FireStations
            .FirstOrDefaultAsync(fs => fs.Id == result.StationId);
    }

    public async Task<CoastGuardStation?> FindNearestCoastGuardStationAsync(double latitude, double longitude)
    {
        var result = await FindNearestCoastGuardStationInternalAsync(latitude, longitude);
        if (result == null) return null;

        return await _context.CoastGuardStations
            .FirstOrDefaultAsync(cgs => cgs.Id == result.StationId);
    }

    public async Task<PoliceStation?> FindNearestPoliceStationAsync(double latitude, double longitude)
    {
        var result = await FindNearestPoliceStationInternalAsync(latitude, longitude);
        if (result == null) return null;

        return await _context.PoliceStations
            .FirstOrDefaultAsync(ps => ps.Name == result.StationName);
    }

    public async Task<Hospital?> FindNearestHospitalAsync(double latitude, double longitude)
    {
        var result = await FindNearestHospitalInternalAsync(latitude, longitude);
        if (result == null) return null;

        return await _context.Hospitals
            .FirstOrDefaultAsync(h => h.Id == result.StationId);
    }

    private async Task<StationAssignmentResponseDto?> FindFireStationByLocationInternalAsync(double latitude, double longitude)
    {
        try
        {
            // Load fire districts data if not already loaded
            await EnsureFireDistrictsLoadedAsync();

            if (_fireDistricts?.Features == null || !_fireDistricts.Features.Any())
            {
                _logger.LogWarning("No fire districts data available");
                return null;
            }

            // Transform WGS84 coordinates to Greek Grid EPSG:2100 for point-in-polygon testing
            var greekGridCoords = TransformWgs84ToGreekGrid(latitude, longitude);
            if (greekGridCoords == null)
            {
                _logger.LogWarning("Failed to transform coordinates {Lat}, {Lng} to Greek Grid", latitude, longitude);
                return null;
            }

            // Find the district containing this point
            var containingDistrict = FindContainingDistrict(greekGridCoords.Value.x, greekGridCoords.Value.y);
            if (containingDistrict == null)
            {
                _logger.LogInformation("No fire district found containing point {Lat}, {Lng}", latitude, longitude);
                return null;
            }

            // Find the corresponding fire station by name
            var stationName = containingDistrict.Properties.PYR_YPIRES;
            if (string.IsNullOrEmpty(stationName))
            {
                _logger.LogWarning("District found but no station name in PYR_YPIRES property");
                return null;
            }

            var fireStation = await _context.FireStations
                .FirstOrDefaultAsync(fs => fs.Name == stationName);

            if (fireStation == null)
            {
                _logger.LogWarning("No fire station found with name '{StationName}'", stationName);
                return null;
            }

            return new StationAssignmentResponseDto
            {
                StationId = fireStation.Id,
                StationName = fireStation.Name,
                AssignmentMethod = "District",
                DistrictName = stationName,
                Distance = 0 // District-based assignment doesn't use distance
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding fire station for location {Lat}, {Lng}", latitude, longitude);
            return null;
        }
    }

    private async Task<StationAssignmentResponseDto?> FindNearestCoastGuardStationInternalAsync(double latitude, double longitude)
    {
        var stations = await _context.CoastGuardStations.ToListAsync();
        return FindNearestStation(latitude, longitude, stations.Select(s => new { s.Id, s.Name, s.Latitude, s.Longitude }));
    }

    private async Task<StationAssignmentResponseDto?> FindNearestPoliceStationInternalAsync(double latitude, double longitude)
    {
        var policeAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Name == "Hellenic Police");
        if (policeAgency == null)
        {
            _logger.LogError("Hellenic Police agency not found in database");
            return null;
        }

        var stations = await _context.Stations
            .Where(s => s.AgencyId == policeAgency.Id)
            .ToListAsync();
        
        return FindNearestStation(latitude, longitude, stations.Select(s => new { s.Id, s.Name, s.Latitude, s.Longitude }));
    }

    private async Task<StationAssignmentResponseDto?> FindNearestHospitalInternalAsync(double latitude, double longitude)
    {
        var stations = await _context.Hospitals.ToListAsync();
        return FindNearestStation(latitude, longitude, stations.Select(s => new { s.Id, s.Name, s.Latitude, s.Longitude }));
    }

    private StationAssignmentResponseDto? FindNearestStation<T>(double latitude, double longitude, IEnumerable<T> stations) where T : class
    {
        var stationList = stations.ToList();
        if (!stationList.Any()) return null;

        var nearestStation = stationList
            .Select(s => new
            {
                Station = s,
                Distance = CalculateDistance(latitude, longitude,
                    (double)s.GetType().GetProperty("Latitude")!.GetValue(s)!,
                    (double)s.GetType().GetProperty("Longitude")!.GetValue(s)!)
            })
            .OrderBy(x => x.Distance)
            .First();

        return new StationAssignmentResponseDto
        {
            StationId = (int)nearestStation.Station.GetType().GetProperty("Id")!.GetValue(nearestStation.Station)!,
            StationName = (string)nearestStation.Station.GetType().GetProperty("Name")!.GetValue(nearestStation.Station)!,
            AssignmentMethod = "Nearest",
            Distance = nearestStation.Distance,
            DistrictName = string.Empty
        };
    }

    private async Task EnsureFireDistrictsLoadedAsync()
    {
        if (_fireDistricts != null) return;

        await _semaphore.WaitAsync();
        try
        {
            if (_fireDistricts != null) return;

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _fireDistrictOptions.JsonFilePath);
            if (!File.Exists(filePath))
            {
                _logger.LogError("Fire districts file not found at {FilePath}", filePath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);

            // Parse as raw JSON first to handle the complex structure
            using var doc = JsonDocument.Parse(jsonContent);
            var root = doc.RootElement;

            if (!root.TryGetProperty("features", out var featuresElement))
            {
                _logger.LogError("No 'features' property found in fire districts JSON");
                return;
            }

            var features = new List<FireDistrictFeature>();

            foreach (var featureElement in featuresElement.EnumerateArray())
            {
                try
                {
                    if (!featureElement.TryGetProperty("properties", out var propsElement) ||
                        !featureElement.TryGetProperty("geometry", out var geomElement))
                        continue;

                    // Extract properties
                    var stationName = propsElement.TryGetProperty("PYR_YPIRES", out var nameElement)
                        ? nameElement.GetString() ?? string.Empty
                        : string.Empty;

                    if (string.IsNullOrEmpty(stationName))
                        continue; // Skip features without station names

                    // Extract geometry type and coordinates
                    if (!geomElement.TryGetProperty("coordinates", out var coordsElement) ||
                        !geomElement.TryGetProperty("type", out var typeElement))
                        continue;

                    var geometryType = typeElement.GetString() ?? "Polygon";
                    object coordinates;

                    if (geometryType == "MultiPolygon")
                    {
                        coordinates = ParseMultiPolygonCoordinates(coordsElement);
                    }
                    else
                    {
                        coordinates = ParsePolygonCoordinates(coordsElement);
                    }

                    if (coordinates == null)
                        continue;

                    var feature = new FireDistrictFeature
                    {
                        Properties = new FireDistrictProperties { PYR_YPIRES = stationName },
                        Geometry = new FireDistrictGeometry 
                        { 
                            Type = geometryType,
                            Coordinates = coordinates 
                        }
                    };

                    features.Add(feature);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing fire district feature, skipping");
                    continue;
                }
            }

            _fireDistricts = new FireDistrictsGeoJson { Features = features };

            _logger.LogInformation("Loaded {Count} fire districts from {FilePath}",
                _fireDistricts?.Features?.Count ?? 0, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fire districts data from {FilePath}", _fireDistrictOptions.JsonFilePath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private double[][][] ParsePolygonCoordinates(JsonElement coordsElement)
    {
        try
        {
            var rings = new List<double[][]>();

            // Coordinates should be [[[x,y],[x,y],...]] for a Polygon
            foreach (var ringElement in coordsElement.EnumerateArray())
            {
                var ring = new List<double[]>();

                foreach (var pointElement in ringElement.EnumerateArray())
                {
                    if (pointElement.ValueKind == JsonValueKind.Array)
                    {
                        var coords = pointElement.EnumerateArray().ToArray();
                        if (coords.Length >= 2)
                        {
                            // Ensure both coordinates are numbers before trying to parse
                            if (coords[0].ValueKind == JsonValueKind.Number && 
                                coords[1].ValueKind == JsonValueKind.Number)
                            {
                                var x = coords[0].GetDouble();
                                var y = coords[1].GetDouble();
                                ring.Add(new[] { x, y });
                            }
                            else
                            {
                                _logger.LogDebug("Skipping coordinate point with non-numeric values: [{CoordType0}, {CoordType1}]", 
                                    coords[0].ValueKind, coords[1].ValueKind);
                            }
                        }
                    }
                }

                if (ring.Count > 0)
                    rings.Add(ring.ToArray());
            }

            return rings.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing polygon coordinates");
            return Array.Empty<double[][]>();
        }
    }

    private double[][][][] ParseMultiPolygonCoordinates(JsonElement coordsElement)
    {
        try
        {
            var polygons = new List<double[][][]>();

            // Coordinates should be [[[[x,y],[x,y],...]], [[[x,y],[x,y],...]]] for a MultiPolygon
            foreach (var polygonElement in coordsElement.EnumerateArray())
            {
                var rings = new List<double[][]>();

                foreach (var ringElement in polygonElement.EnumerateArray())
                {
                    var ring = new List<double[]>();

                    foreach (var pointElement in ringElement.EnumerateArray())
                    {
                        if (pointElement.ValueKind == JsonValueKind.Array)
                        {
                            var coords = pointElement.EnumerateArray().ToArray();
                            if (coords.Length >= 2)
                            {
                                // Ensure both coordinates are numbers before trying to parse
                                if (coords[0].ValueKind == JsonValueKind.Number && 
                                    coords[1].ValueKind == JsonValueKind.Number)
                                {
                                    var x = coords[0].GetDouble();
                                    var y = coords[1].GetDouble();
                                    ring.Add(new[] { x, y });
                                }
                                else
                                {
                                    _logger.LogDebug("Skipping coordinate point with non-numeric values: [{CoordType0}, {CoordType1}]", 
                                        coords[0].ValueKind, coords[1].ValueKind);
                                }
                            }
                        }
                    }

                    if (ring.Count > 0)
                        rings.Add(ring.ToArray());
                }

                if (rings.Count > 0)
                    polygons.Add(rings.ToArray());
            }

            return polygons.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing multi-polygon coordinates");
            return Array.Empty<double[][][]>();
        }
    }

    private FireDistrictFeature? FindContainingDistrict(double x, double y)
    {
        if (_fireDistricts?.Features == null) return null;

        foreach (var district in _fireDistricts.Features)
        {
            if (district.Geometry?.Coordinates == null) continue;

            // Handle both Polygon and MultiPolygon geometries
            if (district.Geometry.Type == "Polygon")
            {
                // For Polygon: coordinates is array of rings [outer, hole1, hole2, ...]
                if (district.Geometry.Coordinates is double[][][] polygonCoords)
                {
                    if (IsPointInPolygonGeometry(x, y, polygonCoords))
                    {
                        return district;
                    }
                }
            }
            else if (district.Geometry.Type == "MultiPolygon")
            {
                // For MultiPolygon: coordinates is array of polygons, each with their own rings
                if (district.Geometry.Coordinates is double[][][][] multiPolygonCoords)
                {
                    foreach (var polygon in multiPolygonCoords)
                    {
                        if (IsPointInPolygonGeometry(x, y, polygon))
                        {
                            return district;
                        }
                    }
                }
            }
        }

        return null;
    }

    private bool IsPointInPolygonGeometry(double x, double y, double[][][] polygonRings)
    {
        if (polygonRings == null || polygonRings.Length == 0) return false;

        // Check the outer ring (first ring)
        var outerRing = polygonRings[0];
        if (outerRing == null || outerRing.Length < 3) return false;

        var outerPoints = outerRing.Select(coord => new { X = coord[0], Y = coord[1] }).ToArray();
        
        // Point must be inside the outer ring
        if (!IsPointInPolygon(x, y, outerPoints))
        {
            return false;
        }

        // Point must not be inside any holes (remaining rings)
        for (int i = 1; i < polygonRings.Length; i++)
        {
            var hole = polygonRings[i];
            if (hole == null || hole.Length < 3) continue;

            var holePoints = hole.Select(coord => new { X = coord[0], Y = coord[1] }).ToArray();
            
            // If point is inside a hole, it's not in the polygon
            if (IsPointInPolygon(x, y, holePoints))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsPointInPolygon(double x, double y, dynamic[] polygon)
    {
        int n = polygon.Length;
        bool inside = false;

        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            double xi = polygon[i].X, yi = polygon[i].Y;
            double xj = polygon[j].X, yj = polygon[j].Y;

            if (((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi))
            {
                inside = !inside;
            }
        }

        return inside;
    }

    // Create Greek Grid EPSG:2100 coordinate system
    private static ICoordinateSystem CreateGreekGridCoordinateSystem()
    {
        var factory = new CoordinateSystemFactory();
        
        // Create Greek Grid (EPSG:2100) coordinate system using WKT
        var greekGrid = factory.CreateFromWkt(@"
            PROJCS[""Greek Grid"",
                GEOGCS[""GCS_GGRS87"",
                    DATUM[""D_GGRS87"",
                        SPHEROID[""GRS_1980"",6378137,298.257222101]],
                    PRIMEM[""Greenwich"",0],
                    UNIT[""Degree"",0.017453292519943295]],
                PROJECTION[""Transverse_Mercator""],
                PARAMETER[""latitude_of_origin"",0],
                PARAMETER[""central_meridian"",24],
                PARAMETER[""scale_factor"",0.9996],
                PARAMETER[""false_easting"",500000],
                PARAMETER[""false_northing"",0],
                UNIT[""Meter"",1]]");
            
        return greekGrid;
    }
    
    // Create coordinate transformation
    private static ICoordinateTransformation CreateTransformation()
    {
        var factory = new CoordinateSystemFactory();
        var wgs84 = factory.CreateFromWkt(@"
            GEOGCS[""WGS 84"",
                DATUM[""WGS_1984"",
                    SPHEROID[""WGS 84"",6378137,298.257223563]],
                PRIMEM[""Greenwich"",0],
                UNIT[""degree"",0.01745329251994328]]");
        var greekGrid = CreateGreekGridCoordinateSystem();
        
        var transformFactory = new CoordinateTransformationFactory();
        return transformFactory.CreateFromCoordinateSystems(wgs84, greekGrid);
    }

    // Transform coordinates from WGS84 to Greek Grid EPSG:2100
    private (double x, double y)? TransformWgs84ToGreekGrid(double lat, double lng)
    {
        try
        {
            // Basic validation for Greek coordinates
            if (lat < 34 || lat > 42 || lng < 19 || lng > 30)
            {
                _logger.LogWarning("Coordinates outside Greece bounds: lat={Lat}, lng={Lng}", lat, lng);
                return null;
            }

            // Transform using ProjNet4GeoAPI
            var result = WGS84ToGreekGrid.MathTransform.Transform(new[] { lng, lat });
            return (result[0], result[1]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming coordinates {Lat}, {Lng}", lat, lng);
            return null;
        }
    }

    private static double CalculateDistance(double lat1, double lng1, double lat2, double lng2)
    {
        const double R = 6371000; // Earth's radius in meters

        double lat1Rad = lat1 * Math.PI / 180;
        double lat2Rad = lat2 * Math.PI / 180;
        double deltaLatRad = (lat2 - lat1) * Math.PI / 180;
        double deltaLngRad = (lng2 - lng1) * Math.PI / 180;

        double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                   Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                   Math.Sin(deltaLngRad / 2) * Math.Sin(deltaLngRad / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return R * c;
    }
}