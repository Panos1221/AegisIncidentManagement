using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.Services;

public interface IVehicleService
{
    /// <summary>
    /// Gets vehicles filtered by station and agency
    /// </summary>
    /// <param name="stationId">Optional station ID to filter by</param>
    /// <param name="userAgencyId">Optional agency ID to filter by</param>
    /// <returns>List of vehicles</returns>
    Task<List<VehicleDto>> GetVehiclesByStationAsync(int? stationId = null, int? userAgencyId = null);

    /// <summary>
    /// Updates a vehicle with new information
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <param name="dto">Update data</param>
    /// <returns>Updated vehicle</returns>
    Task<VehicleDto> UpdateVehicleAsync(int id, UpdateVehicleDto dto);

    /// <summary>
    /// Gets vehicles grouped by their assigned station
    /// </summary>
    /// <returns>Dictionary with station ID as key and list of vehicles as value</returns>
    Task<Dictionary<int, List<VehicleDto>>> GetVehiclesGroupedByStationAsync();

    /// <summary>
    /// Gets a single vehicle by ID
    /// </summary>
    /// <param name="id">Vehicle ID</param>
    /// <returns>Vehicle or null if not found</returns>
    Task<VehicleDto?> GetVehicleByIdAsync(int id);

    /// <summary>
    /// Gets all vehicles with optional status filter and agency filter
    /// </summary>
    /// <param name="status">Optional status filter</param>
    /// <param name="userAgencyId">Optional agency ID to filter by</param>
    /// <returns>List of vehicles</returns>
    Task<List<VehicleDto>> GetVehiclesAsync(VehicleStatus? status = null, int? userAgencyId = null);

    /// <summary>
    /// Creates a new vehicle
    /// </summary>
    /// <param name="dto">Vehicle creation data</param>
    /// <returns>Created vehicle</returns>
    Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto);
}