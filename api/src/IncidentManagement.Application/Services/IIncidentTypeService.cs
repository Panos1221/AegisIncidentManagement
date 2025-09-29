using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Services;

public interface IIncidentTypeService
{
    Task<List<IncidentTypesByAgencyDto>> GetAllIncidentTypesAsync();
    Task<IncidentTypesByAgencyDto?> GetIncidentTypesByAgencyAsync(string agencyName);
}