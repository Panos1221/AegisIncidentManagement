using IncidentManagement.Application.Services;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Api.Services;

public interface IRealTimeNotificationService : INotificationService
{
    Task SendIncidentNotification(int incidentId, string title, string message, int? targetUserId = null, int? targetStationId = null, int? targetAgencyId = null);
    Task SendIncidentUpdate(int incidentId, object updateData, int? targetStationId = null, int? targetAgencyId = null);
    Task BroadcastIncidentCreated(int incidentId, object incidentData, int targetStationId);
    Task BroadcastIncidentStatusChanged(int incidentId, IncidentStatus newStatus, int targetStationId);
    Task BroadcastResourceAssigned(int incidentId, string resourceType, int resourceId, int targetStationId, int targetAgencyId);
    Task BroadcastAssignmentStatusChanged(int incidentId, int assignmentId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId);
    Task BroadcastIncidentLogAdded(int incidentId, string message, DateTime at, string? by, int targetStationId, int targetAgencyId);
    Task BroadcastVehicleAssignmentChanged(int vehicleId, int? personnelId, string action, int targetStationId);

    // Roster and personnel updates
    Task BroadcastPersonnelStatusChanged(int personnelId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId);
    Task BroadcastPersonnelCreated(object personnelData, int targetStationId, int targetAgencyId);
    Task BroadcastPersonnelUpdated(int personnelId, object updateData, int targetStationId, int targetAgencyId);
    Task BroadcastPersonnelDeleted(int personnelId, int targetStationId, int targetAgencyId);

    // Vehicle updates
    Task BroadcastVehicleStatusChanged(int vehicleId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId);
    Task BroadcastVehicleCreated(object vehicleData, int targetStationId, int targetAgencyId);
    Task BroadcastVehicleUpdated(int vehicleId, object updateData, int targetStationId, int targetAgencyId);
    Task BroadcastVehicleDeleted(int vehicleId, int targetStationId, int targetAgencyId);

    // Global commander/dashboard updates
    Task BroadcastToGlobalDispatchers(string eventType, object data);
    Task BroadcastToRole(string role, string eventType, object data);
}