using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IncidentManagement.Api.Controllers;

public abstract class BaseController : ControllerBase
{
    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    protected int? GetCurrentUserAgencyId()
    {
        var agencyIdClaim = User.FindFirst("AgencyId")?.Value;
        return int.TryParse(agencyIdClaim, out var agencyId) ? agencyId : null;
    }

    protected int? GetCurrentUserStationId()
    {
        var stationIdClaim = User.FindFirst("StationId")?.Value;
        return int.TryParse(stationIdClaim, out var stationId) ? stationId : null;
    }

    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value;
    }

    protected bool IsDispatcher()
    {
        var role = GetCurrentUserRole();
        return role?.Contains("Dispatcher") == true;
    }

    protected bool IsMember()
    {
        var role = GetCurrentUserRole();
        return role == "Member";
    }

    protected bool CanAccessAgency(int agencyId)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        return userAgencyId.HasValue && userAgencyId.Value == agencyId;
    }

    protected bool CanAccessStation(int stationId)
    {
        // Dispatchers can access all stations in their agency
        if (IsDispatcher())
        {
            // This will be validated at the service level to ensure station belongs to user's agency
            return true;
        }
        
        // Regular members can only access their assigned station
        var userStationId = GetCurrentUserStationId();
        return userStationId.HasValue && userStationId.Value == stationId;
    }

    protected bool CanManageStationResources()
    {
        // Only regular agency members (not dispatchers) can manage station resources
        return IsMember();
    }

    protected bool CanCreateIncidents()
    {
        return IsDispatcher();
    }

    protected bool CanAssignResources()
    {
        return IsDispatcher();
    }
}