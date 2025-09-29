using System.Security.Claims;
using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Api.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationMiddleware> _logger;

    public AuthorizationMiddleware(RequestDelegate next, ILogger<AuthorizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip authorization for authentication endpoints and health checks
        var path = context.Request.Path.Value?.ToLower();
        if (path?.Contains("/auth/") == true ||
            path?.Contains("/health") == true ||
            path?.Contains("/swagger") == true ||
            path?.Contains("/api/fire-districts") == true ||
            path?.Contains("/api/incidenttypes") == true)
        {
            await _next(context);
            return;
        }

        // Only apply to API endpoints
        if (path?.StartsWith("/api/") != true)
        {
            await _next(context);
            return;
        }

        // Check if user is authenticated
        if (!context.User.Identity?.IsAuthenticated == true)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Authentication required");
            return;
        }

        // Get user claims
        var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
        var userAgencyId = context.User.FindFirst("AgencyId")?.Value;
        var userStationId = context.User.FindFirst("StationId")?.Value;

        if (string.IsNullOrEmpty(userRole) || string.IsNullOrEmpty(userAgencyId))
        {
            _logger.LogWarning("User {UserId} has incomplete role or agency information", 
                context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Incomplete user authorization data");
            return;
        }

        // Add authorization context to request headers for controllers to use
        context.Items["UserRole"] = userRole;
        context.Items["UserAgencyId"] = userAgencyId;
        context.Items["UserStationId"] = userStationId;

        // Log access for audit purposes
        _logger.LogDebug("API access: {Method} {Path} by {Role} from Agency {AgencyId}", 
            context.Request.Method, path, userRole, userAgencyId);

        await _next(context);
    }
}
