namespace IncidentManagement.Application.DTOs;

public class IncidentCommanderDto
{
    public int Id { get; set; }
    public int IncidentId { get; set; }
    public int PersonnelId { get; set; }
    public string PersonnelName { get; set; } = "";
    public string PersonnelBadgeNumber { get; set; } = "";
    public string PersonnelRank { get; set; } = "";
    public string? Observations { get; set; }
    public DateTime AssignedAt { get; set; }
    public int AssignedByUserId { get; set; }
    public string AssignedByName { get; set; } = "";
}

public class CreateIncidentCommanderDto
{
    public int PersonnelId { get; set; }
    public string? Observations { get; set; }
    public int AssignedByUserId { get; set; }
}

public class UpdateIncidentCommanderDto
{
    public string? Observations { get; set; }
}