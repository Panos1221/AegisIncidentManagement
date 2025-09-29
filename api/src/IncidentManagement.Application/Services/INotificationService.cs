using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.Services;

public interface INotificationService
{
    Task NotifyIncidentAssigned(int incidentId, int stationId);
    Task NotifyIncidentStatusUpdate(int incidentId, IncidentStatus newStatus);
    Task NotifyVehicleAssigned(int incidentId, int vehicleId);
    Task NotifyPersonnelAssigned(int incidentId, int personnelId);
}