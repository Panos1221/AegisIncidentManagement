using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Services;

public interface IGeographicService
{
    /// <summary>
    /// Finds the fire station that contains the given coordinates
    /// </summary>
    /// <param name="latitude">Latitude in WGS84 (decimal degrees)</param>
    /// <param name="longitude">Longitude in WGS84 (decimal degrees)</param>
    /// <returns>The fire station containing the point, or null if no station contains it</returns>
    Task<FireStation?> FindStationByCoordinatesAsync(double latitude, double longitude);

    /// <summary>
    /// Gets all fire stations with their boundary data
    /// </summary>
    /// <returns>List of fire stations with boundaries</returns>
    Task<List<FireStation>> GetAllStationsWithBoundariesAsync();

    /// <summary>
    /// Determines if a point is inside a polygon using ray casting algorithm
    /// </summary>
    /// <param name="latitude">Point latitude in WGS84</param>
    /// <param name="longitude">Point longitude in WGS84</param>
    /// <param name="polygon">Polygon coordinates as [longitude, latitude] pairs</param>
    /// <returns>True if point is inside polygon, false otherwise</returns>
    bool IsPointInPolygon(double latitude, double longitude, List<List<double>> polygon);

}