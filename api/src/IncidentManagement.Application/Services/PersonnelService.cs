using Microsoft.EntityFrameworkCore;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Application.Services;

public class PersonnelService : IPersonnelService
{
    private readonly IncidentManagementDbContext _context;

    public PersonnelService(IncidentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<List<PersonnelDto>> GetAllPersonnelAsync()
    {
        var personnel = await _context.Personnel
            .Include(p => p.Agency)
            .Include(p => p.Station)
            .ToListAsync();

        return personnel.Select(MapToDto).ToList();
    }

    public async Task<PersonnelDto?> GetPersonnelByIdAsync(int id)
    {
        var personnel = await _context.Personnel
            .Include(p => p.Agency)
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.Id == id);

        return personnel != null ? MapToDto(personnel) : null;
    }

    public async Task<List<PersonnelDto>> GetCrewByStationAsync(int? stationId = null, bool? isActive = null)
    {
        var query = _context.Personnel
            .Include(p => p.Agency)
            .Include(p => p.Station)
            .AsQueryable();

        if (stationId.HasValue)
            query = query.Where(p => p.StationId == stationId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var personnel = await query
            .OrderBy(p => p.Station!.Name)
            .ThenBy(p => p.Rank)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return personnel.Select(MapToDto).ToList();
    }

    public async Task<Dictionary<int, List<PersonnelDto>>> GetCrewGroupedByStationAsync(bool? isActive = null)
    {
        var query = _context.Personnel
            .Include(p => p.Agency)
            .Include(p => p.Station)
            .AsQueryable();

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var personnel = await query
            .OrderBy(p => p.Station!.Name)
            .ThenBy(p => p.Rank)
            .ThenBy(p => p.Name)
            .ToListAsync();

        return personnel
            .GroupBy(p => p.StationId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(MapToDto).ToList()
            );
    }

    public async Task<PersonnelDto> CreatePersonnelAsync(CreatePersonnelDto dto)
    {
        var personnel = new Personnel
        {
            StationId = dto.StationId,
            AgencyId = dto.AgencyId,
            Name = dto.Name,
            Rank = dto.Rank,
            BadgeNumber = dto.BadgeNumber,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _context.Personnel.Add(personnel);
        await _context.SaveChangesAsync();

        // Reload with agency and station information
        await _context.Entry(personnel)
            .Reference(p => p.Agency)
            .LoadAsync();
        
        await _context.Entry(personnel)
            .Reference(p => p.Station)
            .LoadAsync();

        return MapToDto(personnel);
    }

    public async Task<PersonnelDto?> UpdatePersonnelAsync(int id, UpdatePersonnelDto dto)
    {
        var personnel = await _context.Personnel
            .Include(p => p.Agency)
            .Include(p => p.Station)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (personnel == null)
            return null;

        if (!string.IsNullOrEmpty(dto.Name))
            personnel.Name = dto.Name;

        if (!string.IsNullOrEmpty(dto.Rank))
            personnel.Rank = dto.Rank;

        if (dto.BadgeNumber != null)
            personnel.BadgeNumber = dto.BadgeNumber;

        if (dto.IsActive.HasValue)
            personnel.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync();

        return MapToDto(personnel);
    }

    public async Task<bool> DeletePersonnelAsync(int id)
    {
        var personnel = await _context.Personnel.FindAsync(id);
        if (personnel == null)
            return false;

        _context.Personnel.Remove(personnel);
        await _context.SaveChangesAsync();

        return true;
    }

    private static PersonnelDto MapToDto(Personnel personnel)
    {
        return new PersonnelDto
        {
            Id = personnel.Id,
            StationId = personnel.StationId,
            Name = personnel.Name,
            Rank = personnel.Rank,
            BadgeNumber = personnel.BadgeNumber,
            IsActive = personnel.IsActive,
            AgencyId = personnel.AgencyId,
            AgencyName = personnel.Agency?.Name ?? "",
            Station = personnel.Station != null ? new StationDto
            {
                Id = personnel.Station.Id,
                Name = personnel.Station.Name,
                AgencyId = personnel.Station.AgencyId,
                AgencyName = personnel.Station.Agency?.Name ?? "",
                Latitude = personnel.Station.Latitude,
                Longitude = personnel.Station.Longitude
            } : null
        };
    }
}