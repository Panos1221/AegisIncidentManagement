using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AegisDispatcher.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ILogger<LoggingService> _logger;

        public LoggingService(ILogger<LoggingService> logger)
        {
            _logger = logger;
        }

        public void LogInformation(string message)
        {
            _logger.LogInformation(message);
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.LogInformation(message, args);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.LogWarning(message, args);
        }

        public void LogError(string message)
        {
            _logger.LogError(message);
        }

        public void LogError(string message, params object[] args)
        {
            _logger.LogError(message, args);
        }

        public void LogError(Exception exception, string message)
        {
            _logger.LogError(exception, message);
        }

        public void LogError(Exception exception, string message, params object[] args)
        {
            _logger.LogError(exception, message, args);
        }

        public void LogDebug(string message)
        {
            _logger.LogDebug(message);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.LogDebug(message, args);
        }

        public void LogCritical(string message)
        {
            _logger.LogCritical(message);
        }

        public void LogCritical(Exception exception, string message)
        {
            _logger.LogCritical(exception, message);
        }

        #region Application-Specific Events

        public void LogUserLogin(string email, string role, bool successful)
        {
            if (successful)
            {
                _logger.LogInformation("User login successful: {Email} (Role: {Role})", email, role);
            }
            else
            {
                _logger.LogWarning("User login failed: {Email}", email);
            }
        }

        public void LogUserLogout(string email)
        {
            _logger.LogInformation("User logged out: {Email}", email);
        }

        public void LogIncidentCreated(int incidentId, string createdBy)
        {
            _logger.LogInformation("Incident created: ID={IncidentId}, CreatedBy={CreatedBy}", incidentId, createdBy);
        }

        public void LogIncidentStatusChange(int incidentId, string oldStatus, string newStatus, string changedBy)
        {
            _logger.LogInformation("Incident status changed: ID={IncidentId}, From={OldStatus}, To={NewStatus}, ChangedBy={ChangedBy}", 
                incidentId, oldStatus, newStatus, changedBy);
        }

        public void LogResourceAssignment(int incidentId, string resourceType, int resourceId, string assignedBy)
        {
            _logger.LogInformation("Resource assigned: IncidentId={IncidentId}, ResourceType={ResourceType}, ResourceId={ResourceId}, AssignedBy={AssignedBy}", 
                incidentId, resourceType, resourceId, assignedBy);
        }

        public void LogApiCall(string endpoint, string method, int statusCode, long responseTime)
        {
            var level = statusCode >= 400 ? LogLevel.Warning : LogLevel.Debug;
            _logger.Log(level, "API call: {Method} {Endpoint} returned {StatusCode} in {ResponseTime}ms", 
                method, endpoint, statusCode, responseTime);
        }

        #endregion
    }
}
