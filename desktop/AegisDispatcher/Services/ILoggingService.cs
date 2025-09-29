using Microsoft.Extensions.Logging;

namespace AegisDispatcher.Services
{
    public interface ILoggingService
    {
        void LogInformation(string message);
        void LogInformation(string message, params object[] args);
        void LogWarning(string message);
        void LogWarning(string message, params object[] args);
        void LogError(string message);
        void LogError(string message, params object[] args);
        void LogError(Exception exception, string message);
        void LogError(Exception exception, string message, params object[] args);
        void LogDebug(string message);
        void LogDebug(string message, params object[] args);
        void LogCritical(string message);
        void LogCritical(Exception exception, string message);
        
        // Specific application events
        void LogUserLogin(string email, string role, bool successful);
        void LogUserLogout(string email);
        void LogIncidentCreated(int incidentId, string createdBy);
        void LogIncidentStatusChange(int incidentId, string oldStatus, string newStatus, string changedBy);
        void LogResourceAssignment(int incidentId, string resourceType, int resourceId, string assignedBy);
        void LogApiCall(string endpoint, string method, int statusCode, long responseTime);
    }
}
