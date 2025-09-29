namespace IncidentManagement.Domain.Entities;

public class Caller
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public Incident? Incident { get; set; }
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public DateTime CalledAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}