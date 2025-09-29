using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Application.Services;

public class StationService : IStationService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<StationService> _logger;

    public StationService(IncidentManagementDbContext context, ILogger<StationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<StationDto>> GetStationsByAgencyAsync(int agencyId)
    {
        try
        {
            var stations = await _context.Stations
                .Include(s => s.Agency)
                .Where(s => s.AgencyId == agencyId)
                .ToListAsync();

            return stations.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stations for agency {AgencyId}", agencyId);
            throw;
        }
    }

    public async Task<StationDto?> GetStationByIdAsync(int id, int? userAgencyId = null)
    {
        try
        {
            var query = _context.Stations
                .Include(s => s.Agency)
                .AsQueryable();

            // Filter by agency if provided
            if (userAgencyId.HasValue)
            {
                query = query.Where(s => s.AgencyId == userAgencyId.Value);
            }

            var station = await query.FirstOrDefaultAsync(s => s.Id == id);
            return station != null ? MapToDto(station) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving station {StationId} for agency {AgencyId}", id, userAgencyId);
            throw;
        }
    }

    public async Task<StationDto> CreateStationAsync(CreateStationDto dto)
    {
        try
        {
            // Validate agency exists
            var agencyExists = await _context.Agencies.AnyAsync(a => a.Id == dto.AgencyId);
            if (!agencyExists)
            {
                throw new ArgumentException($"Agency with ID {dto.AgencyId} not found");
            }

            var station = new Station
            {
                Name = dto.Name,
                AgencyId = dto.AgencyId,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            _context.Stations.Add(station);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Station {StationName} created successfully with ID {StationId}", 
                dto.Name, station.Id);

            // Load the agency for the DTO
            await _context.Entry(station).Reference(s => s.Agency).LoadAsync();
            
            return MapToDto(station);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating station {StationName}", dto.Name);
            throw;
        }
    }

    public async Task<StationDto> UpdateStationAsync(int id, UpdateStationDto dto, int? userAgencyId = null)
    {
        try
        {
            var station = await _context.Stations
                .Include(s => s.Agency)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (station == null)
            {
                throw new ArgumentException($"Station with ID {id} not found");
            }

            // Validate user can access this station
            if (userAgencyId.HasValue && station.AgencyId != userAgencyId.Value)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this station");
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(dto.Name))
                station.Name = dto.Name;

            if (dto.Latitude.HasValue)
            {
                if (dto.Latitude.Value < -90 || dto.Latitude.Value > 90)
                {
                    throw new ArgumentException("Latitude must be between -90 and 90 degrees");
                }
                station.Latitude = dto.Latitude.Value;
            }

            if (dto.Longitude.HasValue)
            {
                if (dto.Longitude.Value < -180 || dto.Longitude.Value > 180)
                {
                    throw new ArgumentException("Longitude must be between -180 and 180 degrees");
                }
                station.Longitude = dto.Longitude.Value;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Station {StationId} updated successfully", id);
            return MapToDto(station);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating station {StationId}", id);
            throw;
        }
    }

    private static StationDto MapToDto(Station station)
    {
        return new StationDto
        {
            Id = station.Id,
            Name = station.Name,
            AgencyId = station.AgencyId,
            AgencyName = station.Agency?.Name ?? "",
            Latitude = station.Latitude,
            Longitude = station.Longitude
        };
    }
}
