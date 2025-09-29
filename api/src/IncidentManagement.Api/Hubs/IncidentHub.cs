using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace IncidentManagement.Api.Hubs;

[Authorize]
public class IncidentHub : Hub
{
    private readonly ILogger<IncidentHub> _logger;
    private static readonly Dictionary<string, UserConnectionInfo> _connections = new();
    private static readonly object _lock = new object();

    public IncidentHub(ILogger<IncidentHub> logger)
    {
        _logger = logger;
    }

    public async Task JoinRosterGroup()
    {
        var userAgencyId = Context.User?.FindFirst("AgencyId")?.Value;
        var userStationId = Context.User?.FindFirst("StationId")?.Value;

        if (!string.IsNullOrEmpty(userAgencyId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RosterAgency_{userAgencyId}");
        }

        if (!string.IsNullOrEmpty(userStationId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"RosterStation_{userStationId}");
        }

        _logger.LogInformation("User joined roster groups for Agency {AgencyId} and Station {StationId}", userAgencyId, userStationId);
    }

    public async Task JoinIncidentGroup(int incidentId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Incident_{incidentId}");
        _logger.LogInformation("User {UserId} joined incident group {IncidentId}", Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, incidentId);
    }

    public async Task LeaveIncidentGroup(int incidentId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Incident_{incidentId}");
        _logger.LogInformation("User {UserId} left incident group {IncidentId}", Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, incidentId);
    }

    public async Task JoinVehicleGroup(int vehicleId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
        _logger.LogInformation("User {UserId} joined vehicle group {VehicleId}", Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, vehicleId);
    }

    public async Task LeaveVehicleGroup(int vehicleId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Vehicle_{vehicleId}");
        _logger.LogInformation("User {UserId} left vehicle group {VehicleId}", Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, vehicleId);
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        var agencyId = Context.User?.FindFirst("AgencyId")?.Value;
        var stationId = Context.User?.FindFirst("StationId")?.Value;

        if (userId != null)
        {
            lock (_lock)
            {
                _connections[Context.ConnectionId] = new UserConnectionInfo
                {
                    UserId = int.Parse(userId),
                    UserRole = userRole,
                    AgencyId = !string.IsNullOrEmpty(agencyId) ? int.Parse(agencyId) : null,
                    StationId = !string.IsNullOrEmpty(stationId) ? int.Parse(stationId) : null,
                    ConnectionId = Context.ConnectionId,
                    ConnectedAt = DateTime.UtcNow
                };
            }

            // Join appropriate groups based on user's role and organization
            if (!string.IsNullOrEmpty(agencyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Agency_{agencyId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"RosterAgency_{agencyId}");
            }

            if (!string.IsNullOrEmpty(stationId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Station_{stationId}");
                await Groups.AddToGroupAsync(Context.ConnectionId, $"RosterStation_{stationId}");
            }

            // Join role-specific groups
            if (!string.IsNullOrEmpty(userRole))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{userRole}");

                // Dispatchers join global groups
                if (userRole == "Dispatcher")
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "GlobalDispatchers");
                }
            }

            _logger.LogInformation("User {UserId} connected with ConnectionId {ConnectionId}", userId, Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (_lock)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out var userInfo))
            {
                _connections.Remove(Context.ConnectionId);
                _logger.LogInformation("User {UserId} disconnected with ConnectionId {ConnectionId}", userInfo.UserId, Context.ConnectionId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }

    public static List<UserConnectionInfo> GetConnectedUsers()
    {
        lock (_lock)
        {
            return _connections.Values.ToList();
        }
    }

    public static List<UserConnectionInfo> GetConnectedUsersForAgency(int agencyId)
    {
        lock (_lock)
        {
            return _connections.Values
                .Where(c => c.AgencyId == agencyId)
                .ToList();
        }
    }

    public static List<UserConnectionInfo> GetConnectedUsersForStation(int stationId)
    {
        lock (_lock)
        {
            return _connections.Values
                .Where(c => c.StationId == stationId)
                .ToList();
        }
    }

    public static UserConnectionInfo? GetUserConnection(int userId)
    {
        lock (_lock)
        {
            return _connections.Values.FirstOrDefault(c => c.UserId == userId);
        }
    }
}

public class UserConnectionInfo
{
    public int UserId { get; set; }
    public string? UserRole { get; set; }
    public int? AgencyId { get; set; }
    public int? StationId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; set; }
}