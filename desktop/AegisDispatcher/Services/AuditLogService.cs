using System.Text.Json;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Globalization;

namespace AegisDispatcher.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly ILoggingService _loggingService;
        private readonly string _auditLogDirectory;
        private readonly string _auditLogFile;
        private readonly object _fileLock = new object();

        public AuditLogService(ILoggingService loggingService, IConfiguration configuration)
        {
            _loggingService = loggingService;
            
            // Create audit log directory
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "AegisDispatcher");
            _auditLogDirectory = Path.Combine(appDataPath, "AuditLogs");
            
            Directory.CreateDirectory(_auditLogDirectory);
            
            // Daily audit log files
            _auditLogFile = Path.Combine(_auditLogDirectory, $"audit-{DateTime.Now:yyyy-MM-dd}.json");
            
            _loggingService.LogInformation("AuditLogService initialized. Log directory: {Directory}", _auditLogDirectory);
        }

        public async Task LogAssignmentAsync(int incidentId, string resourceType, int resourceId, string assignedBy, DateTime timestamp)
        {
            var entry = new AuditLogEntry
            {
                Id = GenerateId(),
                Timestamp = timestamp,
                Action = "RESOURCE_ASSIGNED",
                IncidentId = incidentId,
                ResourceType = resourceType,
                ResourceId = resourceId,
                PerformedBy = assignedBy,
                AdditionalData = JsonSerializer.Serialize(new { IncidentId = incidentId, ResourceType = resourceType, ResourceId = resourceId }),
                IpAddress = GetLocalIpAddress(),
                UserAgent = "AegisDispatcher/1.0.0"
            };

            await WriteAuditLogAsync(entry);
            _loggingService.LogInformation("Audit: Resource assignment logged - {ResourceType} {ResourceId} assigned to incident {IncidentId} by {AssignedBy}", 
                resourceType, resourceId, incidentId, assignedBy);
        }

        public async Task LogStatusChangeAsync(int incidentId, string oldStatus, string newStatus, string changedBy, DateTime timestamp)
        {
            var entry = new AuditLogEntry
            {
                Id = GenerateId(),
                Timestamp = timestamp,
                Action = "STATUS_CHANGED",
                IncidentId = incidentId,
                OldValue = oldStatus,
                NewValue = newStatus,
                PerformedBy = changedBy,
                AdditionalData = JsonSerializer.Serialize(new { IncidentId = incidentId, From = oldStatus, To = newStatus }),
                IpAddress = GetLocalIpAddress(),
                UserAgent = "AegisDispatcher/1.0.0"
            };

            await WriteAuditLogAsync(entry);
            _loggingService.LogInformation("Audit: Status change logged - Incident {IncidentId} status changed from {OldStatus} to {NewStatus} by {ChangedBy}", 
                incidentId, oldStatus, newStatus, changedBy);
        }

        public async Task LogIncidentCreatedAsync(int incidentId, string createdBy, DateTime timestamp)
        {
            var entry = new AuditLogEntry
            {
                Id = GenerateId(),
                Timestamp = timestamp,
                Action = "INCIDENT_CREATED",
                IncidentId = incidentId,
                PerformedBy = createdBy,
                AdditionalData = JsonSerializer.Serialize(new { IncidentId = incidentId }),
                IpAddress = GetLocalIpAddress(),
                UserAgent = "AegisDispatcher/1.0.0"
            };

            await WriteAuditLogAsync(entry);
            _loggingService.LogInformation("Audit: Incident creation logged - Incident {IncidentId} created by {CreatedBy}", incidentId, createdBy);
        }

        public async Task LogIncidentClosedAsync(int incidentId, string reason, string closedBy, DateTime timestamp)
        {
            var entry = new AuditLogEntry
            {
                Id = GenerateId(),
                Timestamp = timestamp,
                Action = "INCIDENT_CLOSED",
                IncidentId = incidentId,
                NewValue = reason,
                PerformedBy = closedBy,
                AdditionalData = JsonSerializer.Serialize(new { IncidentId = incidentId, Reason = reason }),
                IpAddress = GetLocalIpAddress(),
                UserAgent = "AegisDispatcher/1.0.0"
            };

            await WriteAuditLogAsync(entry);
            _loggingService.LogInformation("Audit: Incident closure logged - Incident {IncidentId} closed with reason {Reason} by {ClosedBy}", 
                incidentId, reason, closedBy);
        }

        public async Task<List<AuditLogEntry>> GetAuditLogsAsync(int? incidentId = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var entries = new List<AuditLogEntry>();
            
            try
            {
                var startDate = fromDate ?? DateTime.Now.Date.AddDays(-30);
                var endDate = toDate ?? DateTime.Now.Date.AddDays(1);

                // Read from multiple daily log files if needed
                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    var dailyLogFile = Path.Combine(_auditLogDirectory, $"audit-{date:yyyy-MM-dd}.json");
                    
                    if (File.Exists(dailyLogFile))
                    {
                        var dailyEntries = await ReadDailyLogAsync(dailyLogFile);
                        entries.AddRange(dailyEntries);
                    }
                }

                // Filter by incident ID if specified
                if (incidentId.HasValue)
                {
                    entries = entries.Where(e => e.IncidentId == incidentId.Value).ToList();
                }

                // Filter by date range
                entries = entries.Where(e => e.Timestamp >= startDate && e.Timestamp <= endDate).ToList();

                return entries.OrderByDescending(e => e.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error reading audit logs");
                return new List<AuditLogEntry>();
            }
        }

        public async Task<List<AuditLogEntry>> GetUserAuditLogsAsync(string userEmail, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var allEntries = await GetAuditLogsAsync(null, fromDate, toDate);
            return allEntries.Where(e => e.PerformedBy.Equals(userEmail, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public async Task ClearOldLogsAsync(int retentionDays = 90)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                var files = Directory.GetFiles(_auditLogDirectory, "audit-*.json");
                
                var deletedCount = 0;
                foreach (var file in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    if (fileName.StartsWith("audit-") && fileName.Length >= 16) // audit-yyyy-MM-dd format
                    {
                        var dateStr = fileName.Substring(6); // Remove "audit-" prefix
                        if (DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fileDate))
                        {
                            if (fileDate < cutoffDate.Date)
                            {
                                File.Delete(file);
                                deletedCount++;
                            }
                        }
                    }
                }

                _loggingService.LogInformation("Audit log cleanup completed. Deleted {DeletedCount} old log files older than {RetentionDays} days", 
                    deletedCount, retentionDays);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error during audit log cleanup");
            }
        }

        private async Task WriteAuditLogAsync(AuditLogEntry entry)
        {
            try
            {
                var json = JsonSerializer.Serialize(entry, new JsonSerializerOptions 
                { 
                    WriteIndented = false,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                lock (_fileLock)
                {
                    File.AppendAllText(_auditLogFile, json + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error writing audit log entry");
            }
        }

        private async Task<List<AuditLogEntry>> ReadDailyLogAsync(string filePath)
        {
            var entries = new List<AuditLogEntry>();
            
            try
            {
                var lines = await File.ReadAllLinesAsync(filePath);
                
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    
                    try
                    {
                        var entry = JsonSerializer.Deserialize<AuditLogEntry>(line, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        });
                        
                        if (entry != null)
                        {
                            entries.Add(entry);
                        }
                    }
                    catch (JsonException)
                    {
                        // Skip malformed lines
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error reading daily audit log: {FilePath}", filePath);
            }
            
            return entries;
        }

        private static int GenerateId()
        {
            return Math.Abs(Guid.NewGuid().GetHashCode());
        }

        private static string GetLocalIpAddress()
        {
            return "127.0.0.1"; // For desktop app, we'll use localhost
        }
    }
}
