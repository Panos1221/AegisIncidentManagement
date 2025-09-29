namespace IncidentManagement.Application.DTOs;

public class IncidentLogDto
{
    public long Id { get; set; }
    public int IncidentId { get; set; }
    public DateTime At { get; set; }
    public string Message { get; set; } = "";
    public string? By { get; set; }
}

public class CreateIncidentLogDto
{
    public string Message { get; set; } = "";
    public string? By { get; set; }
}