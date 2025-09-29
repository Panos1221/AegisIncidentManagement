namespace AegisDispatcher.Services
{
    public interface IAuditLogService
    {
        Task LogAssignmentAsync(int incidentId, string resourceType, int resourceId, string assignedBy, DateTime timestamp);
        Task LogStatusChangeAsync(int incidentId, string oldStatus, string newStatus, string changedBy, DateTime timestamp);
        Task LogIncidentCreatedAsync(int incidentId, string createdBy, DateTime timestamp);
        Task LogIncidentClosedAsync(int incidentId, string reason, string closedBy, DateTime timestamp);
        Task<List<AuditLogEntry>> GetAuditLogsAsync(int? incidentId = null, DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<AuditLogEntry>> GetUserAuditLogsAsync(string userEmail, DateTime? fromDate = null, DateTime? toDate = null);
        Task ClearOldLogsAsync(int retentionDays = 90);
    }

    public class AuditLogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public int? IncidentId { get; set; }
        public string? ResourceType { get; set; }
        public int? ResourceId { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string? AdditionalData { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
