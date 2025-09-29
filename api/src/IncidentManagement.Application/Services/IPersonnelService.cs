using IncidentManagement.Application.DTOs;

namespace IncidentManagement.Application.Services;

public interface IPersonnelService
{
    Task<List<PersonnelDto>> GetAllPersonnelAsync();
    Task<PersonnelDto?> GetPersonnelByIdAsync(int id);
    Task<List<PersonnelDto>> GetCrewByStationAsync(int? stationId = null, bool? isActive = null);
    Task<Dictionary<int, List<PersonnelDto>>> GetCrewGroupedByStationAsync(bool? isActive = null);
    Task<PersonnelDto> CreatePersonnelAsync(CreatePersonnelDto dto);
    Task<PersonnelDto?> UpdatePersonnelAsync(int id, UpdatePersonnelDto dto);
    Task<bool> DeletePersonnelAsync(int id);
}