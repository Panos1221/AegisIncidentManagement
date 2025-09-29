using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Services;

public interface IStationService
{
    /// <summary>
    /// Gets stations filtered by agency
    /// </summary>
    /// <param name="agencyId">Agency ID to filter by</param>
    /// <returns>List of stations</returns>
    Task<List<StationDto>> GetStationsByAgencyAsync(int agencyId);

    /// <summary>
    /// Gets a single station by ID, with agency validation
    /// </summary>
    /// <param name="id">Station ID</param>
    /// <param name="userAgencyId">User's agency ID for validation</param>
    /// <returns>Station or null if not found or access denied</returns>
    Task<StationDto?> GetStationByIdAsync(int id, int? userAgencyId = null);

    /// <summary>
    /// Creates a new station
    /// </summary>
    /// <param name="dto">Station creation data</param>
    /// <returns>Created station</returns>
    Task<StationDto> CreateStationAsync(CreateStationDto dto);

    /// <summary>
    /// Updates a station
    /// </summary>
    /// <param name="id">Station ID</param>
    /// <param name="dto">Update data</param>
    /// <param name="userAgencyId">User's agency ID for validation</param>
    /// <returns>Updated station</returns>
    Task<StationDto> UpdateStationAsync(int id, UpdateStationDto dto, int? userAgencyId = null);
}
