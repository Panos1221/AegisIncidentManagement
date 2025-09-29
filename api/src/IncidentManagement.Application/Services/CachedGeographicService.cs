using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IncidentManagement.Application.Services;

public class CachedGeographicService : IGeographicService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<CachedGeographicService> _logger;
    private readonly IMemoryCache _cache;
    
    // Cache keys
    private const string ALL_STATIONS_CACHE_KEY = "all_fire_stations_with_boundaries";
    private const string STATION_LOOKUP_CACHE_PREFIX = "station_lookup_";
    
    // Cache durations
    private static readonly TimeSpan StationsCacheDuration = TimeSpan.FromMinutes(30); // Stations don't change often
    private static readonly TimeSpan LookupCacheDuration = TimeSpan.FromMinutes(10);   // Geographic lookups can be cached shorter

    public CachedGeographicService(
        IncidentManagementDbContext context,
        ILogger<CachedGeographicService> logger,
        IMemoryCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    public async Task<FireStation?> FindStationByCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
            // Create cache key for this specific lookup
            var lookupKey = $"{STATION_LOOKUP_CACHE_PREFIX}{latitude:F6}_{longitude:F6}";
            
            // Try to get from cache first
            if (_cache.TryGetValue(lookupKey, out FireStation? cachedResult))
            {
                _logger.LogDebug("Cache hit for station lookup at {Latitude}, {Longitude}", latitude, longitude);
                return cachedResult;
            }

            _logger.LogDebug("Cache miss for station lookup at {Latitude}, {Longitude}", latitude, longitude);

            var stationsWithBoundaries = await GetAllStationsWithBoundariesAsync();
            
            FireStation? bestMatch = null;
            double smallestArea = double.MaxValue;

            foreach (var station in stationsWithBoundaries)
            {
                foreach (var boundary in station.Boundaries)
                {
                    try
                    {
                        var coordinates = JsonSerializer.Deserialize<List<List<double>>>(boundary.CoordinatesJson);
                        if (coordinates != null && IsPointInPolygonInternal(latitude, longitude, coordinates))
                        {
                            // If multiple stations contain the point, choose the one with smallest area
                            if (station.Area < smallestArea)
                            {
                                bestMatch = station;
                                smallestArea = station.Area;
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse coordinates for boundary {BoundaryId}", boundary.Id);
                    }
                }
            }

            // Cache the result (including null results to avoid repeated expensive lookups)
            _cache.Set(lookupKey, bestMatch, LookupCacheDuration);
            
            if (bestMatch != null)
            {
                _logger.LogDebug("Found station {StationName} for coordinates {Latitude}, {Longitude}", 
                    bestMatch.Name, latitude, longitude);
            }
            else
            {
                _logger.LogDebug("No station found for coordinates {Latitude}, {Longitude}", latitude, longitude);
            }

            return bestMatch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding station by coordinates {Latitude}, {Longitude}", latitude, longitude);
            return null;
        }
    }

    public async Task<List<FireStation>> GetAllStationsWithBoundariesAsync()
    {
        try
        {
            // Try to get from cache first
            if (_cache.TryGetValue(ALL_STATIONS_CACHE_KEY, out List<FireStation>? cachedStations))
            {
                _logger.LogDebug("Cache hit for all stations with boundaries");
                return cachedStations!;
            }

            _logger.LogDebug("Cache miss for all stations with boundaries, fetching from database");

            // Fetch from database with optimized query
            var stations = await _context.FireStations
                .Include(fs => fs.Boundaries)
                .AsNoTracking() // Improve performance for read-only operations
                .ToListAsync();

            // Cache the result
            _cache.Set(ALL_STATIONS_CACHE_KEY, stations, StationsCacheDuration);
            
            _logger.LogInformation("Loaded {StationCount} fire stations with boundaries from database", stations.Count);

            return stations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire stations with boundaries");
            return new List<FireStation>();
        }
    }

    public bool IsPointInPolygon(double latitude, double longitude, List<List<double>> polygon)
    {
        return IsPointInPolygonInternal(latitude, longitude, polygon);
    }

    /// <summary>
    /// Optimized point-in-polygon test using ray casting algorithm
    /// </summary>
    private bool IsPointInPolygonInternal(double latitude, double longitude, List<List<double>> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return false;

        bool inside = false;
        int j = polygon.Count - 1;

        for (int i = 0; i < polygon.Count; i++)
        {
            var xi = polygon[i][0]; // longitude
            var yi = polygon[i][1]; // latitude
            var xj = polygon[j][0]; // longitude
            var yj = polygon[j][1]; // latitude

            if (((yi > latitude) != (yj > latitude)) &&
                (longitude < (xj - xi) * (latitude - yi) / (yj - yi) + xi))
            {
                inside = !inside;
            }
            j = i;
        }

        return inside;
    }

    /// <summary>
    /// Clear all geographic caches
    /// </summary>
    public void ClearCache()
    {
        _cache.Remove(ALL_STATIONS_CACHE_KEY);
        
        // Note: Individual lookup caches will expire naturally        
        _logger.LogInformation("Geographic service cache cleared");
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    public object GetCacheStatistics()
    {
        var hasStationsCache = _cache.TryGetValue(ALL_STATIONS_CACHE_KEY, out _);
        
        return new
        {
            HasStationsCache = hasStationsCache,
            StationsCacheDurationMinutes = StationsCacheDuration.TotalMinutes,
            LookupCacheDurationMinutes = LookupCacheDuration.TotalMinutes
        };
    }
}

// Coordinate class for compatibility with existing interface
public class Coordinate
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}