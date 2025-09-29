namespace IncidentManagement.Application.DTOs;

public class CallerDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public DateTime CalledAt { get; set; }
    public string? Notes { get; set; }
}

public class CreateCallerDto
{
    public string Name { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public DateTime? CalledAt { get; set; }
    public string? Notes { get; set; }
}