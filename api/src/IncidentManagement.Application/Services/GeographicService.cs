using System.Text.Json;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IncidentManagement.Application.Services;

public class GeographicService : IGeographicService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<GeographicService> _logger;

    public GeographicService(
        IncidentManagementDbContext context,
        ILogger<GeographicService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FireStation?> FindStationByCoordinatesAsync(double latitude, double longitude)
    {
        try
        {
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
                        if (coordinates != null && IsPointInPolygon(latitude, longitude, coordinates))
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

            return bestMatch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding station by coordinates ({Latitude}, {Longitude})", latitude, longitude);
            return null;
        }
    }

    public async Task<List<FireStation>> GetAllStationsWithBoundariesAsync()
    {
        try
        {
            return await _context.FireStations
                .Include(fs => fs.Boundaries)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving fire stations with boundaries");
            return new List<FireStation>();
        }
    }

    public bool IsPointInPolygon(double latitude, double longitude, List<List<double>> polygon)
    {
        if (polygon == null || polygon.Count < 3)
        {
            return false;
        }

        try
        {
            // Ray casting algorithm implementation
            bool isInside = false;
            int polygonLength = polygon.Count;

            for (int i = 0, j = polygonLength - 1; i < polygonLength; j = i++)
            {
                var xi = polygon[i][0]; // longitude
                var yi = polygon[i][1]; // latitude
                var xj = polygon[j][0]; // longitude  
                var yj = polygon[j][1]; // latitude

                // Check if point is on the same horizontal line as the edge
                if (((yi > latitude) != (yj > latitude)) &&
                    (longitude < (xj - xi) * (latitude - yi) / (yj - yi) + xi))
                {
                    isInside = !isInside;
                }
            }

            return isInside;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error in point-in-polygon calculation for point ({Latitude}, {Longitude})", latitude, longitude);
            return false;
        }
    }

    public (double longitude, double latitude) TransformGreekGridToWgs84(double easting, double northing)
    {
        try
        {
            // Greek Grid (EPSG:2100) to WGS84 (EPSG:4326) transformation
            // This is based on Greek Grid parameters
            
            // Greek Grid constants
            const double falseEasting = 500000.0;
            const double falseNorthing = 0.0;
            const double centralMeridian = 24.0; // 24°E in decimal degrees
            const double scaleFactor = 0.9996;
            
            // Semi-major axis and flattening for GRS80 ellipsoid (used by Greek Grid)
            const double semiMajorAxis = 6378137.0; // meters
            const double flattening = 1.0 / 298.257222101;
            double eccentricity = Math.Sqrt(2 * flattening - flattening * flattening);
            
            // Remove false easting and northing
            double adjustedEasting = easting - falseEasting;
            double adjustedNorthing = northing - falseNorthing;
            
            // Convert to radians for calculations
            double centralMeridianRad = centralMeridian * Math.PI / 180.0;
            
            // Simplified inverse transformation
            
            // Calculate longitude (simplified)
            double longitudeRad = centralMeridianRad + (adjustedEasting / (scaleFactor * semiMajorAxis));
            double longitude = longitudeRad * 180.0 / Math.PI;
            
            // Calculate latitude (simplified)
            double M = adjustedNorthing / scaleFactor;
            double mu = M / (semiMajorAxis * (1 - eccentricity * eccentricity / 4 - 3 * Math.Pow(eccentricity, 4) / 64));
            
            // Footprint latitude calculation (simplified)
            double e1 = (1 - Math.Sqrt(1 - eccentricity * eccentricity)) / (1 + Math.Sqrt(1 - eccentricity * eccentricity));
            double J1 = (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32);
            double J2 = (21 * e1 * e1 / 16 - 55 * Math.Pow(e1, 4) / 32);
            double J3 = (151 * Math.Pow(e1, 3) / 96);
            double J4 = (1097 * Math.Pow(e1, 4) / 512);
            
            double fp = mu + J1 * Math.Sin(2 * mu) + J2 * Math.Sin(4 * mu) + J3 * Math.Sin(6 * mu) + J4 * Math.Sin(8 * mu);
            double latitude = fp * 180.0 / Math.PI;
            
            // Apply bounds checking for Greece
            longitude = Math.Max(19.0, Math.Min(29.0, longitude));
            latitude = Math.Max(34.0, Math.Min(42.0, latitude));
            
            return (longitude, latitude);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error transforming coordinates from Greek Grid ({Easting}, {Northing}) to WGS84", easting, northing);
            
            // Fallback to simple approximation
            double longitude = 24.0 + (easting - 500000.0) / 111320.0;
            double latitude = northing / 111320.0;
            
            longitude = Math.Max(19.0, Math.Min(29.0, longitude));
            latitude = Math.Max(34.0, Math.Min(42.0, latitude));
            
            return (longitude, latitude);
        }
    }
}