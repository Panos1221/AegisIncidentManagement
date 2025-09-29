namespace IncidentManagement.Domain.Entities;

public class IncidentLog
{
    public long Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }
    public DateTime At { get; set; } = DateTime.UtcNow;
    public string Message { get; set; } = "";
    public string? By { get; set; }
}