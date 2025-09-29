using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;

namespace IncidentManagement.Application.Services;

public interface IStationAssignmentService
{
    Task<StationAssignmentResponseDto?> FindStationByLocationAsync(StationAssignmentRequestDto request);
    Task<FireStation?> FindFireStationByLocationAsync(double latitude, double longitude);
    Task<CoastGuardStation?> FindNearestCoastGuardStationAsync(double latitude, double longitude);
    Task<PoliceStation?> FindNearestPoliceStationAsync(double latitude, double longitude);
    Task<Hospital?> FindNearestHospitalAsync(double latitude, double longitude);
}