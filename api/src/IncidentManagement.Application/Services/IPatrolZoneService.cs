using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Services;

public interface IPatrolZoneService
{
    Task<IEnumerable<PatrolZoneDto>> GetAllAsync(int agencyId);
    Task<PatrolZoneDto?> GetByIdAsync(int id, int agencyId);
    Task<PatrolZoneDto> CreateAsync(CreatePatrolZoneDto createDto, int agencyId, int userId);
    Task<PatrolZoneDto?> UpdateAsync(int id, UpdatePatrolZoneDto updateDto, int agencyId);
    Task<bool> DeleteAsync(int id, int agencyId);
    Task<IEnumerable<PatrolZoneDto>> GetByStationIdAsync(int stationId, int agencyId);
    
    // Vehicle assignment methods
    Task<PatrolZoneAssignmentDto> AssignVehicleAsync(CreatePatrolZoneAssignmentDto assignmentDto, int agencyId, int userId);
    Task<bool> UnassignVehicleAsync(int assignmentId, int agencyId, int userId);
    Task<IEnumerable<PatrolZoneAssignmentDto>> GetActiveAssignmentsAsync(int patrolZoneId, int agencyId);
    Task<IEnumerable<PatrolZoneAssignmentDto>> GetVehicleAssignmentHistoryAsync(int vehicleId, int agencyId);
}