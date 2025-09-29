namespace IncidentManagement.Domain.Entities;

public class ShiftInstance
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string? SourceTemplateName { get; set; }
}