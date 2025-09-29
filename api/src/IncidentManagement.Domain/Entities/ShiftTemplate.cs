namespace IncidentManagement.Domain.Entities;

public class ShiftTemplate
{
    public int Id { get; set; }
    public int StationId { get; set; }
    public Station? Station { get; set; }
    public string Name { get; set; } = "24on/48off";
    public TimeSpan Duration { get; set; }
    public string RRule { get; set; } = "";
}