using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IncidentManagement.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IncidentManagementDbContext _context;

    public NotificationService(IncidentManagementDbContext context)
    {
        _context = context;
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
    }

    public async Task NotifyPersonnelAssigned(int incidentId, int personnelId)
    {
        var personnel = await _context.Personnel
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == personnelId);
        
        if (personnel?.User == null) return;

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
    }
}