using IncidentManagement.Domain.Enums;

namespace IncidentManagement.Application.DTOs;

public class AgencyDto
{
    public int Id { get; set; }
    public AgencyType Type { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateAgencyDto
{
    public AgencyType Type { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
    public bool IsActive { get; set; } = true;
}