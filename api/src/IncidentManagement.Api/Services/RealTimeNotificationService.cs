using IncidentManagement.Application.Services;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Api.Services;

public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IncidentManagementDbContext _context;
    private readonly IHubContext<IncidentHub> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;

    public RealTimeNotificationService(
        IncidentManagementDbContext context,
        IHubContext<IncidentHub> hubContext,
        ILogger<RealTimeNotificationService> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyIncidentAssigned(int incidentId, int stationId)
    {
        var incident = await _context.Incidents.FindAsync(incidentId);
        if (incident == null) return;

        // Get all firefighters from the assigned station
        var stationUsers = await _context.Users
            .Where(u => u.StationId == stationId && u.Role == UserRole.Member && u.IsActive)
            .ToListAsync();

        foreach (var user in stationUsers)
        {
            // Create database notification
            var notification = new Notification
            {
                UserId = user.Id,
                Type = NotificationType.IncidentAssigned,
                Title = "New Incident Assigned",
                Message = $"Incident #{incident.Id} ({incident.MainCategory} - {incident.SubCategory}) has been assigned to your station.",
                IncidentId = incidentId
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        // Send real-time notification
        await BroadcastIncidentCreated(incidentId, new
        {
            Id = incident.Id,
            MainCategory = incident.MainCategory,
            SubCategory = incident.SubCategory,
            Notes = incident.Notes,
            Address = incident.Address,
            Priority = incident.Priority.ToString(),
            Status = incident.Status.ToString(),
            CreatedAt = incident.CreatedAt
        }, stationId);

        _logger.LogInformation("Incident {IncidentId} assigned to station {StationId} with real-time notifications", incidentId, stationId);
    }

    public async Task NotifyIncidentStatusUpdate(int incidentId, IncidentStatus newStatus)
    {
        var incident = await _context.Incidents
            .Include(i => i.Assignments)
            .FirstOrDefaultAsync(i => i.Id == incidentId);

        if (incident == null) return;

        // Get all users involved in this incident (station personnel + dispatchers)
        var stationUsers = await _context.Users
            .Where(u => u.StationId == incident.StationId && u.IsActive)
            .ToListAsync();

        var dispatchers = await _context.Users
            .Where(u => u.Role == UserRole.Dispatcher && u.IsActive)
            .ToListAsync();

        var allUsers = stationUsers.Concat(dispatchers).Distinct();

        foreach (var user in allUsers)
        {
            // Create database notification
            var notification = new Notification
            {
                UserId = user.Id,
                Type = NotificationType.IncidentStatusUpdate,
                Title = "Incident Status Updated",
                Message = $"Incident #{incident.Id} status changed to {newStatus}.",
                IncidentId = incidentId
            };

            _context.Notifications.Add(notification);
        }

        await _context.SaveChangesAsync();

        // Send real-time notification
        await BroadcastIncidentStatusChanged(incidentId, newStatus, incident.StationId);

        _logger.LogInformation("Incident {IncidentId} status updated to {Status} with real-time notifications", incidentId, newStatus);
    }

    public async Task NotifyVehicleAssigned(int incidentId, int vehicleId)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Station)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null) return;

        // Get personnel assigned to this vehicle
        var vehicleAssignments = await _context.VehicleAssignments
            .Include(va => va.Personnel)
            .ThenInclude(p => p.User)
            .Where(va => va.VehicleId == vehicleId && va.IsActive)
            .ToListAsync();

        foreach (var assignment in vehicleAssignments)
        {
            if (assignment.Personnel?.User != null)
            {
                // Create database notification
                var notification = new Notification
                {
                    UserId = assignment.Personnel.User.Id,
                    Type = NotificationType.VehicleAssigned,
                    Title = "Vehicle Assigned to Incident",
                    Message = $"Your vehicle {vehicle.Callsign} has been assigned to incident #{incidentId}.",
                    IncidentId = incidentId
                };

                _context.Notifications.Add(notification);
            }
        }

        await _context.SaveChangesAsync();

        // Send real-time notification
        await BroadcastResourceAssigned(incidentId, "Vehicle", vehicleId, vehicle.StationId, vehicle.Station.AgencyId);

        _logger.LogInformation("Vehicle {VehicleId} assigned to incident {IncidentId} with real-time notifications", vehicleId, incidentId);
    }

    public async Task NotifyPersonnelAssigned(int incidentId, int personnelId)
    {
        var personnel = await _context.Personnel
            .Include(p => p.User)
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.Id == personnelId);

        if (personnel?.User == null) return;

        // Create database notification
        var notification = new Notification
        {
            UserId = personnel.User.Id,
            Type = NotificationType.PersonnelAssigned,
            Title = "Assigned to Incident",
            Message = $"You have been assigned to incident #{incidentId}.",
            IncidentId = incidentId
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send real-time notification
        await BroadcastResourceAssigned(incidentId, "Personnel", personnelId, personnel.StationId, personnel.Station.AgencyId);

        _logger.LogInformation("Personnel {PersonnelId} assigned to incident {IncidentId} with real-time notifications", personnelId, incidentId);
    }

    public async Task SendIncidentNotification(int incidentId, string title, string message, int? targetUserId = null, int? targetStationId = null, int? targetAgencyId = null)
    {
        var notificationData = new
        {
            IncidentId = incidentId,
            Title = title,
            Message = message,
            Timestamp = DateTime.UtcNow
        };

        if (targetUserId.HasValue)
        {
            var userConnection = IncidentHub.GetUserConnection(targetUserId.Value);
            if (userConnection != null)
            {
                await _hubContext.Clients.Client(userConnection.ConnectionId)
                    .SendAsync("IncidentNotification", notificationData);
            }
        }
        else if (targetStationId.HasValue)
        {
            await _hubContext.Clients.Group($"Station_{targetStationId.Value}")
                .SendAsync("IncidentNotification", notificationData);
        }
        else if (targetAgencyId.HasValue)
        {
            await _hubContext.Clients.Group($"Agency_{targetAgencyId.Value}")
                .SendAsync("IncidentNotification", notificationData);
        }
    }

    public async Task SendIncidentUpdate(int incidentId, object updateData, int? targetStationId = null, int? targetAgencyId = null)
    {
        var data = new
        {
            IncidentId = incidentId,
            UpdateData = updateData,
            Timestamp = DateTime.UtcNow
        };

        if (targetStationId.HasValue)
        {
            await _hubContext.Clients.Group($"Station_{targetStationId.Value}")
                .SendAsync("IncidentUpdate", data);
        }
        else if (targetAgencyId.HasValue)
        {
            await _hubContext.Clients.Group($"Agency_{targetAgencyId.Value}")
                .SendAsync("IncidentUpdate", data);
        }
    }

    public async Task BroadcastIncidentCreated(int incidentId, object incidentData, int targetStationId)
    {
        var data = new
        {
            IncidentId = incidentId,
            IncidentData = incidentData,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("IncidentCreated", data);
    }

    public async Task BroadcastIncidentStatusChanged(int incidentId, IncidentStatus newStatus, int targetStationId)
    {
        var data = new
        {
            IncidentId = incidentId,
            NewStatus = newStatus.ToString(),
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("IncidentStatusChanged", data);
    }

    public async Task BroadcastResourceAssigned(int incidentId, string resourceType, int resourceId, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            IncidentId = incidentId,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Timestamp = DateTime.UtcNow
        };

        // Broadcast to both station and agency level
        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("ResourceAssigned", data);

        await _hubContext.Clients.Group($"Agency_{targetAgencyId}")
            .SendAsync("ResourceAssigned", data);
    }

    public async Task BroadcastAssignmentStatusChanged(int incidentId, int assignmentId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            IncidentId = incidentId,
            AssignmentId = assignmentId,
            NewStatus = newStatus,
            OldStatus = oldStatus,
            Timestamp = DateTime.UtcNow
        };

        // Broadcast to both station and agency level
        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("AssignmentStatusChanged", data);

        await _hubContext.Clients.Group($"Agency_{targetAgencyId}")
            .SendAsync("AssignmentStatusChanged", data);
    }

    public async Task BroadcastIncidentLogAdded(int incidentId, string message, DateTime at, string? by, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            IncidentId = incidentId,
            Message = message,
            At = at,
            By = by,
            Timestamp = DateTime.UtcNow
        };

        // Broadcast to both station and agency level
        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("IncidentLogAdded", data);

        await _hubContext.Clients.Group($"Agency_{targetAgencyId}")
            .SendAsync("IncidentLogAdded", data);
    }

    public async Task BroadcastVehicleAssignmentChanged(int vehicleId, int? personnelId, string action, int targetStationId)
    {
        var data = new
        {
            VehicleId = vehicleId,
            PersonnelId = personnelId,
            Action = action, // "assigned" or "unassigned"
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"Station_{targetStationId}")
            .SendAsync("VehicleAssignmentChanged", data);
    }

    public async Task BroadcastPersonnelStatusChanged(int personnelId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            PersonnelId = personnelId,
            NewStatus = newStatus,
            OldStatus = oldStatus,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("PersonnelStatusChanged", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("PersonnelStatusChanged", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("PersonnelStatusChanged", data);
    }

    public async Task BroadcastPersonnelCreated(object personnelData, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            PersonnelData = personnelData,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("PersonnelCreated", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("PersonnelCreated", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("PersonnelCreated", data);
    }

    public async Task BroadcastPersonnelUpdated(int personnelId, object updateData, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            PersonnelId = personnelId,
            UpdateData = updateData,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("PersonnelUpdated", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("PersonnelUpdated", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("PersonnelUpdated", data);
    }

    public async Task BroadcastPersonnelDeleted(int personnelId, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            PersonnelId = personnelId,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("PersonnelDeleted", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("PersonnelDeleted", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("PersonnelDeleted", data);
    }

    public async Task BroadcastVehicleStatusChanged(int vehicleId, string newStatus, string oldStatus, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            VehicleId = vehicleId,
            NewStatus = newStatus,
            OldStatus = oldStatus,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("VehicleStatusChanged", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("VehicleStatusChanged", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("VehicleStatusChanged", data);
    }

    public async Task BroadcastVehicleCreated(object vehicleData, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            VehicleData = vehicleData,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("VehicleCreated", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("VehicleCreated", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("VehicleCreated", data);
    }

    public async Task BroadcastVehicleUpdated(int vehicleId, object updateData, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            VehicleId = vehicleId,
            UpdateData = updateData,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("VehicleUpdated", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("VehicleUpdated", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("VehicleUpdated", data);
    }

    public async Task BroadcastVehicleDeleted(int vehicleId, int targetStationId, int targetAgencyId)
    {
        var data = new
        {
            VehicleId = vehicleId,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"RosterStation_{targetStationId}")
            .SendAsync("VehicleDeleted", data);

        await _hubContext.Clients.Group($"RosterAgency_{targetAgencyId}")
            .SendAsync("VehicleDeleted", data);

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("VehicleDeleted", data);
    }

    public async Task BroadcastToGlobalDispatchers(string eventType, object data)
    {
        var payload = new
        {
            EventType = eventType,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group("GlobalDispatchers")
            .SendAsync("GlobalDispatcherUpdate", payload);
    }

    public async Task BroadcastToRole(string role, string eventType, object data)
    {
        var payload = new
        {
            EventType = eventType,
            Data = data,
            Timestamp = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"Role_{role}")
            .SendAsync("RoleUpdate", payload);
    }
}