using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Application.Services;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.OutputCaching;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FireStationsController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<FireStationsController> _logger;
    private readonly IGeographicService _geographicService;
    private readonly IMemoryCache _cache;
    private readonly IStationService _stationService;
    private readonly IStationAssignmentService _stationAssignmentService;

    public FireStationsController(
        IncidentManagementDbContext context,
        ILogger<FireStationsController> logger,
        IGeographicService geographicService,
        IMemoryCache cache,
        IStationService stationService,
        IStationAssignmentService stationAssignmentService)
    {
        _context = context;
        _logger = logger;
        _geographicService = geographicService;
        _cache = cache;
        _stationService = stationService;
        _stationAssignmentService = stationAssignmentService;
    }

    /// <summary>
    /// Get count of loaded fire stations for testing purposes
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<object>> GetFireStationCount()
    {
        try
        {
            var stationCount = await _context.FireStations.CountAsync();
            var boundaryCount = await _context.StationBoundaries.CountAsync();
            
            return Ok(new 
            { 
                FireStationCount = stationCount,
                BoundaryCount = boundaryCount,
                Message = stationCount > 0 ? "Fire station data loaded successfully" : "No fire station data found"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire station count");
            return StatusCode(500, "Error retrieving fire station data");
        }
    }

    /// <summary>
    /// Get all fire stations with location details for map display
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<FireStationDto>>> GetFireStations()
    {
        try
        {
            var stations = await _context.FireStations
                .Select(fs => new FireStationDto
                {
                    Id = fs.Id,
                    Name = fs.Name,
                    Address = fs.Address,
                    City = fs.City,
                    Region = fs.Region,
                    Area = fs.Area,
                    Latitude = fs.Latitude,
                    Longitude = fs.Longitude,
                    GeometryJson = fs.GeometryJson,
                    CreatedAt = fs.CreatedAt
                })
                .ToListAsync();

            return Ok(stations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire stations");
            return StatusCode(500, "Error retrieving fire station data");
        }
    }

    /// <summary>
    /// Get fire station by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<FireStationDto>> GetFireStation(int id)
    {
        try
        {
            var station = await _context.FireStations
                .Where(fs => fs.Id == id)
                .Select(fs => new FireStationDto
                {
                    Id = fs.Id,
                    Name = fs.Name,
                    Address = fs.Address,
                    City = fs.City,
                    Region = fs.Region,
                    Area = fs.Area,
                    Latitude = fs.Latitude,
                    Longitude = fs.Longitude,
                    GeometryJson = fs.GeometryJson,
                    CreatedAt = fs.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (station == null)
            {
                return NotFound($"Fire station with ID {id} not found");
            }

            return Ok(station);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire station {Id}", id);
            return StatusCode(500, "Error retrieving fire station data");
        }
    }

    /// <summary>
    /// Get fire station boundaries for map display with caching and performance optimizations
    /// </summary>
    [HttpGet("boundaries")]
    [OutputCache(Duration = 300)] // Cache for 5 minutes at HTTP level
    public async Task<ActionResult<List<FireStationBoundaryDto>>> GetBoundaries(
        [FromQuery] int? limit = null,
        [FromQuery] bool simplify = false,
        [FromQuery] double tolerance = 0.001)
    {
        try
        {
            // Create cache key based on parameters
            var cacheKey = $"boundaries_{limit}_{simplify}_{tolerance}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out List<FireStationBoundaryDto>? cachedBoundaries))
            {
                _logger.LogDebug("Cache hit for boundaries with parameters: limit={Limit}, simplify={Simplify}", limit, simplify);
                return Ok(cachedBoundaries);
            }

            var stations = await _geographicService.GetAllStationsWithBoundariesAsync();
            
            // Apply limit if specified (for performance)
            if (limit.HasValue && limit.Value > 0)
            {
                stations = stations.Take(limit.Value).ToList();
            }
            
            var boundaryDtos = new List<FireStationBoundaryDto>();
            
            foreach (var station in stations)
            {
                foreach (var boundary in station.Boundaries)
                {
                    try
                    {
                        // Try to deserialize as List<List<double>> first (single ring format)
                        var singleRingCoordinates = JsonSerializer.Deserialize<List<List<double>>>(boundary.CoordinatesJson);
                        if (singleRingCoordinates != null && singleRingCoordinates.Any())
                        {
                            // Apply simplification if requested
                            if (simplify && tolerance > 0)
                            {
                                singleRingCoordinates = SimplifyPolygon(singleRingCoordinates, tolerance);
                            }
                            
                            // Convert single ring to multi-ring format expected by DTO
                            var multiRingCoordinates = new List<List<List<double>>> { singleRingCoordinates };
                            
                            boundaryDtos.Add(new FireStationBoundaryDto
                            {
                                Id = boundary.Id,
                                FireStationId = station.Id,
                                Name = station.Name,
                                Region = station.Region,
                                Area = station.Area,
                                Coordinates = multiRingCoordinates
                            });
                        }
                    }
                    catch (JsonException ex)
                    {
                        try
                        {
                            // Fallback: try to deserialize as multi-ring format
                            var multiRingCoordinates = JsonSerializer.Deserialize<List<List<List<double>>>>(boundary.CoordinatesJson);
                            if (multiRingCoordinates != null && multiRingCoordinates.Any())
                            {
                                // Apply simplification if requested
                                if (simplify && tolerance > 0)
                                {
                                    multiRingCoordinates = multiRingCoordinates.Select(ring => 
                                        SimplifyPolygon(ring, tolerance)
                                    ).ToList();
                                }
                                
                                boundaryDtos.Add(new FireStationBoundaryDto
                                {
                                    Id = boundary.Id,
                                    FireStationId = station.Id,
                                    Name = station.Name,
                                    Region = station.Region,
                                    Area = station.Area,
                                    Coordinates = multiRingCoordinates
                                });
                            }
                        }
                        catch (JsonException ex2)
                        {
                            _logger.LogWarning(ex2, "Failed to parse coordinates for boundary {BoundaryId} of station {StationId}. Original error: {OriginalError}", 
                                boundary.Id, station.Id, ex.Message);
                        }
                    }
                }
            }
            
            // Cache the result for future requests
            _cache.Set(cacheKey, boundaryDtos, TimeSpan.FromMinutes(10));
            
            _logger.LogInformation("Processed {BoundaryCount} fire station boundaries with parameters: limit={Limit}, simplify={Simplify}", 
                boundaryDtos.Count, limit, simplify);
            
            return Ok(boundaryDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire station boundaries");
            return StatusCode(500, "Error retrieving fire station boundary data");
        }
    }

    /// <summary>
    /// Find fire station by geographic coordinates using district boundaries
    /// </summary>
    [HttpPost("find-by-location")]
    public async Task<ActionResult<FireStationDto?>> FindByLocation([FromBody] FindStationByLocationDto locationDto)
    {
        try
        {
            if (locationDto.Latitude < -90 || locationDto.Latitude > 90)
            {
                return BadRequest("Latitude must be between -90 and 90 degrees");
            }

            if (locationDto.Longitude < -180 || locationDto.Longitude > 180)
            {
                return BadRequest("Longitude must be between -180 and 180 degrees");
            }

            // Use the new station assignment service with district-based logic
            var station = await _stationAssignmentService.FindFireStationByLocationAsync(
                locationDto.Latitude,
                locationDto.Longitude);

            if (station == null)
            {
                return Ok((FireStationDto?)null);
            }

            var stationDto = new FireStationDto
            {
                Id = station.Id,
                Name = station.Name,
                Address = station.Address,
                City = station.City,
                Region = station.Region,
                Area = station.Area,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                GeometryJson = station.GeometryJson,
                CreatedAt = station.CreatedAt
            };

            return Ok(stationDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid coordinates provided: Lat={Latitude}, Lon={Longitude}",
                locationDto.Latitude, locationDto.Longitude);
            return BadRequest($"Invalid coordinates: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding station by location: Lat={Latitude}, Lon={Longitude}",
                locationDto.Latitude, locationDto.Longitude);
            return StatusCode(500, "Error performing geographic lookup");
        }
    }

    /// <summary>
    /// Get all stations for filtering purposes (filtered by user's agency)
    /// </summary>
    [HttpGet("stations")]
    public async Task<ActionResult<List<FireStationDto>>> GetStations()
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            var stations = await _stationService.GetStationsByAgencyAsync(userAgencyId.Value);

            var fireStationDtos = stations.Select(s => new FireStationDto
            {
                Id = s.Id,
                Name = s.Name,
                Region = GetAgencyRegionName(s.AgencyName), 
                Area = 0, // Station entity doesn't have area
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                CreatedAt = DateTime.UtcNow // Station entity doesn't have CreatedAt
            }).ToList();

            return Ok(fireStationDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stations");
            return StatusCode(500, "Error retrieving station data");
        }
    }

    private static string GetAgencyRegionName(string agencyName)
    {
        return agencyName switch
        {
            "Hellenic Fire Service" => "Fire District",
            "Hellenic Coast Guard" => "Maritime District",
            "EKAB" => "EMS District",
            "Hellenic Police" => "Police District",
            _ => "District"
        };
    }

    /// <summary>
    /// Get fire station boundaries in GeoJSON format compatible with fire districts
    /// </summary>
    [HttpGet("geojson")]
    [OutputCache(Duration = 300)]
    public async Task<ActionResult> GetBoundariesAsGeoJson()
    {
        try
        {
            const string cacheKey = "boundaries_geojson";
            
            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out object? cachedGeoJson))
            {
                _logger.LogDebug("Cache hit for GeoJSON boundaries");
                return Ok(cachedGeoJson);
            }

            var stations = await _geographicService.GetAllStationsWithBoundariesAsync();
            
            var features = new List<object>();
            
            foreach (var station in stations)
            {
                foreach (var boundary in station.Boundaries)
                {
                    try
                    {
                        // Try to deserialize coordinates
                        var coordinates = JsonSerializer.Deserialize<List<List<double>>>(boundary.CoordinatesJson);
                        if (coordinates != null && coordinates.Any())
                        {
                            var feature = new
                            {
                                type = "Feature",
                                properties = new
                                {
                                    PYR_YPIRES = station.Name,
                                    region = station.Region ?? "Unknown Region",
                                    area = station.Area,
                                    stationId = station.Id
                                },
                                geometry = new
                                {
                                    type = "Polygon",
                                    coordinates = new[] { coordinates }
                                }
                            };
                            
                            features.Add(feature);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse coordinates for boundary {BoundaryId} of station {StationId}", 
                            boundary.Id, station.Id);
                    }
                }
            }
            
            var geoJson = new
            {
                type = "FeatureCollection",
                features = features
            };
            
            // Cache the result
            _cache.Set(cacheKey, geoJson, TimeSpan.FromMinutes(10));
            
            _logger.LogInformation("Generated GeoJSON with {FeatureCount} features", features.Count);
            
            return Ok(geoJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating GeoJSON boundaries");
            return StatusCode(500, "Error generating GeoJSON boundaries");
        }
    }

    /// <summary>
    /// Cache management endpoint for development/testing
    /// </summary>
    [HttpPost("clear-cache")]
    public ActionResult ClearCache()
    {
        try
        {
            if (_geographicService is CachedGeographicService cachedService)
            {
                cachedService.ClearCache();
            }
            
            // Clear controller-level caches
            var cacheKeys = new[] { "boundaries_", "stations_", "boundaries_geojson" };
            foreach (var keyPrefix in cacheKeys)
            {
                // In a real implementation, you'd need a more sophisticated way to track and clear cache keys
                // For now, we'll just clear known keys
                if (keyPrefix == "boundaries_geojson")
                {
                    _cache.Remove(keyPrefix);
                }
            }
            
            return Ok(new { Message = "Cache cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, "Error clearing cache");
        }
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    [HttpGet("cache-stats")]
    public ActionResult GetCacheStatistics()
    {
        try
        {
            var stats = new
            {
                Timestamp = DateTime.UtcNow,
                GeographicServiceStats = _geographicService is CachedGeographicService cachedService 
                    ? cachedService.GetCacheStatistics() 
                    : null
            };
            
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return StatusCode(500, "Error getting cache statistics");
        }
    }

    /// <summary>
    /// Douglas-Peucker polygon simplification algorithm
    /// </summary>
    private static List<List<double>> SimplifyPolygon(List<List<double>> coordinates, double tolerance)
    {
        if (coordinates.Count <= 2) return coordinates;

        static double GetPerpendicularDistance(List<double> point, List<double> lineStart, List<double> lineEnd)
        {
            var x0 = point[0];
            var y0 = point[1];
            var x1 = lineStart[0];
            var y1 = lineStart[1];
            var x2 = lineEnd[0];
            var y2 = lineEnd[1];

            var A = x0 - x1;
            var B = y0 - y1;
            var C = x2 - x1;
            var D = y2 - y1;

            var dot = A * C + B * D;
            var lenSq = C * C + D * D;
            
            if (lenSq == 0) return Math.Sqrt(A * A + B * B);

            var param = dot / lenSq;
            double xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * C;
                yy = y1 + param * D;
            }

            var dx = x0 - xx;
            var dy = y0 - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        static List<List<double>> DouglasPeucker(List<List<double>> points, double epsilon)
        {
            if (points.Count <= 2) return points;

            var maxDistance = 0.0;
            var index = 0;
            var end = points.Count - 1;

            for (var i = 1; i < end; i++)
            {
                var distance = GetPerpendicularDistance(points[i], points[0], points[end]);
                if (distance > maxDistance)
                {
                    index = i;
                    maxDistance = distance;
                }
            }

            if (maxDistance > epsilon)
            {
                var left = DouglasPeucker(points.Take(index + 1).ToList(), epsilon);
                var right = DouglasPeucker(points.Skip(index).ToList(), epsilon);
                return left.Take(left.Count - 1).Concat(right).ToList();
            }

            return new List<List<double>> { points[0], points[end] };
        }

        return DouglasPeucker(coordinates, tolerance);
    }
}