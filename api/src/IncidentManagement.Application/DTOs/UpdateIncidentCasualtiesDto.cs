namespace IncidentManagement.Application.DTOs;

public class UpdateIncidentCasualtiesDto
{
    public List<CreateInjuryDto> Injuries { get; set; } = new();
    public List<CreateDeathDto> Deaths { get; set; } = new();
}