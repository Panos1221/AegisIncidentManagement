using Microsoft.EntityFrameworkCore;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Application.Services;

public class PatrolZoneService : IPatrolZoneService
{
    private readonly IncidentManagementDbContext _context;

    public PatrolZoneService(IncidentManagementDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PatrolZoneDto>> GetAllAsync(int agencyId)
    {
        var patrolZones = await _context.PatrolZones
            .Include(pz => pz.Agency)
            .Include(pz => pz.Station)
            .Include(pz => pz.CreatedByUser)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.Vehicle)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.AssignedByUser)
            .Where(pz => pz.AgencyId == agencyId)
            .ToListAsync();

        return patrolZones.Select(MapToDto);
    }

    public async Task<PatrolZoneDto?> GetByIdAsync(int id, int agencyId)
    {
        var patrolZone = await _context.PatrolZones
            .Include(pz => pz.Agency)
            .Include(pz => pz.Station)
            .Include(pz => pz.CreatedByUser)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.Vehicle)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.AssignedByUser)
            .FirstOrDefaultAsync(pz => pz.Id == id && pz.AgencyId == agencyId);

        return patrolZone != null ? MapToDto(patrolZone) : null;
    }

    public async Task<PatrolZoneDto> CreateAsync(CreatePatrolZoneDto createDto, int agencyId, int userId)
    {
        // Validate that the station belongs to the agency
        var station = await _context.Stations
            .FirstOrDefaultAsync(s => s.Id == createDto.StationId && s.AgencyId == agencyId);
        
        if (station == null)
        {
            throw new ArgumentException("Station not found or does not belong to your agency");
        }

        var patrolZone = new PatrolZone
        {
            Name = createDto.Name,
            Description = createDto.Description,
            AgencyId = agencyId,
            StationId = createDto.StationId,
            BoundaryCoordinates = createDto.BoundaryCoordinates,
            CenterLatitude = createDto.CenterLatitude,
            CenterLongitude = createDto.CenterLongitude,
            Priority = createDto.Priority,
            Color = createDto.Color,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.PatrolZones.Add(patrolZone);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(patrolZone.Id, agencyId) ?? throw new InvalidOperationException("Failed to retrieve created patrol zone");
    }

    public async Task<PatrolZoneDto?> UpdateAsync(int id, UpdatePatrolZoneDto updateDto, int agencyId)
    {
        var patrolZone = await _context.PatrolZones
            .FirstOrDefaultAsync(pz => pz.Id == id && pz.AgencyId == agencyId);

        if (patrolZone == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(updateDto.Name))
            patrolZone.Name = updateDto.Name;
        
        if (updateDto.Description != null)
            patrolZone.Description = updateDto.Description;
        
        if (!string.IsNullOrEmpty(updateDto.BoundaryCoordinates))
            patrolZone.BoundaryCoordinates = updateDto.BoundaryCoordinates;
        
        if (updateDto.CenterLatitude.HasValue)
            patrolZone.CenterLatitude = updateDto.CenterLatitude.Value;
        
        if (updateDto.CenterLongitude.HasValue)
            patrolZone.CenterLongitude = updateDto.CenterLongitude.Value;
        
        if (updateDto.Priority.HasValue)
            patrolZone.Priority = updateDto.Priority.Value;
        
        if (updateDto.IsActive.HasValue)
            patrolZone.IsActive = updateDto.IsActive.Value;
        
        if (updateDto.Color != null)
            patrolZone.Color = updateDto.Color;

        patrolZone.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetByIdAsync(id, agencyId);
    }

    public async Task<bool> DeleteAsync(int id, int agencyId)
    {
        var patrolZone = await _context.PatrolZones
            .FirstOrDefaultAsync(pz => pz.Id == id && pz.AgencyId == agencyId);

        if (patrolZone == null)
        {
            return false;
        }

        _context.PatrolZones.Remove(patrolZone);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PatrolZoneDto>> GetByStationIdAsync(int stationId, int agencyId)
    {
        var patrolZones = await _context.PatrolZones
            .Include(pz => pz.Agency)
            .Include(pz => pz.Station)
            .Include(pz => pz.CreatedByUser)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.Vehicle)
            .Include(pz => pz.VehicleAssignments.Where(va => va.IsActive))
                .ThenInclude(va => va.AssignedByUser)
            .Where(pz => pz.StationId == stationId && pz.AgencyId == agencyId)
            .ToListAsync();

        return patrolZones.Select(MapToDto);
    }

    public async Task<PatrolZoneAssignmentDto> AssignVehicleAsync(CreatePatrolZoneAssignmentDto assignmentDto, int agencyId, int userId)
    {
        // Validate patrol zone belongs to agency
        var patrolZone = await _context.PatrolZones
            .FirstOrDefaultAsync(pz => pz.Id == assignmentDto.PatrolZoneId && pz.AgencyId == agencyId);
        
        if (patrolZone == null)
        {
            throw new ArgumentException("Patrol zone not found or does not belong to your agency");
        }

        // Validate vehicle belongs to agency
        var vehicle = await _context.Vehicles
            .Include(v => v.Station)
            .FirstOrDefaultAsync(v => v.Id == assignmentDto.VehicleId && v.Station!.AgencyId == agencyId);
        
        if (vehicle == null)
        {
            throw new ArgumentException("Vehicle not found or does not belong to your agency");
        }

        // Check if vehicle is already assigned to another active patrol zone
        var existingAssignment = await _context.PatrolZoneAssignments
            .FirstOrDefaultAsync(pza => pza.VehicleId == assignmentDto.VehicleId && pza.IsActive);
        
        if (existingAssignment != null)
        {
            throw new InvalidOperationException("Vehicle is already assigned to another patrol zone");
        }

        var assignment = new PatrolZoneAssignment
        {
            PatrolZoneId = assignmentDto.PatrolZoneId,
            VehicleId = assignmentDto.VehicleId,
            AssignedByUserId = userId,
            Notes = assignmentDto.Notes,
            AssignedAt = DateTime.UtcNow
        };

        _context.PatrolZoneAssignments.Add(assignment);
        await _context.SaveChangesAsync();

        return await GetAssignmentByIdAsync(assignment.Id, agencyId) ?? throw new InvalidOperationException("Failed to retrieve created assignment");
    }

    public async Task<bool> UnassignVehicleAsync(int assignmentId, int agencyId, int userId)
    {
        var assignment = await _context.PatrolZoneAssignments
            .Include(pza => pza.PatrolZone)
            .FirstOrDefaultAsync(pza => pza.Id == assignmentId && pza.PatrolZone.AgencyId == agencyId && pza.IsActive);

        if (assignment == null)
        {
            return false;
        }

        assignment.IsActive = false;
        assignment.UnassignedAt = DateTime.UtcNow;
        assignment.UnassignedByUserId = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<PatrolZoneAssignmentDto>> GetActiveAssignmentsAsync(int patrolZoneId, int agencyId)
    {
        var assignments = await _context.PatrolZoneAssignments
            .Include(pza => pza.PatrolZone)
            .Include(pza => pza.Vehicle)
            .Include(pza => pza.AssignedByUser)
            .Where(pza => pza.PatrolZoneId == patrolZoneId && pza.PatrolZone.AgencyId == agencyId && pza.IsActive)
            .ToListAsync();

        return assignments.Select(MapAssignmentToDto);
    }

    public async Task<IEnumerable<PatrolZoneAssignmentDto>> GetVehicleAssignmentHistoryAsync(int vehicleId, int agencyId)
    {
        var assignments = await _context.PatrolZoneAssignments
            .Include(pza => pza.PatrolZone)
            .Include(pza => pza.Vehicle)
                .ThenInclude(v => v.Station)
            .Include(pza => pza.AssignedByUser)
            .Include(pza => pza.UnassignedByUser)
            .Where(pza => pza.VehicleId == vehicleId && pza.Vehicle!.Station!.AgencyId == agencyId)
            .OrderByDescending(pza => pza.AssignedAt)
            .ToListAsync();

        return assignments.Select(MapAssignmentToDto);
    }

    private async Task<PatrolZoneAssignmentDto?> GetAssignmentByIdAsync(int id, int agencyId)
    {
        var assignment = await _context.PatrolZoneAssignments
            .Include(pza => pza.PatrolZone)
            .Include(pza => pza.Vehicle)
            .Include(pza => pza.AssignedByUser)
            .Include(pza => pza.UnassignedByUser)
            .FirstOrDefaultAsync(pza => pza.Id == id && pza.PatrolZone.AgencyId == agencyId);

        return assignment != null ? MapAssignmentToDto(assignment) : null;
    }

    private static PatrolZoneDto MapToDto(PatrolZone patrolZone)
    {
        return new PatrolZoneDto
        {
            Id = patrolZone.Id,
            Name = patrolZone.Name,
            Description = patrolZone.Description,
            AgencyId = patrolZone.AgencyId,
            AgencyName = patrolZone.Agency.Name,
            StationId = patrolZone.StationId,
            StationName = patrolZone.Station.Name,
            BoundaryCoordinates = patrolZone.BoundaryCoordinates,
            CenterLatitude = patrolZone.CenterLatitude,
            CenterLongitude = patrolZone.CenterLongitude,
            Priority = patrolZone.Priority,
            IsActive = patrolZone.IsActive,
            Color = patrolZone.Color,
            CreatedAt = patrolZone.CreatedAt,
            UpdatedAt = patrolZone.UpdatedAt,
            CreatedByUserId = patrolZone.CreatedByUserId,
            CreatedByUserName = patrolZone.CreatedByUser.Name,
            VehicleAssignments = patrolZone.VehicleAssignments.Select(MapAssignmentToDto).ToList()
        };
    }

    private static PatrolZoneAssignmentDto MapAssignmentToDto(PatrolZoneAssignment assignment)
    {
        return new PatrolZoneAssignmentDto
        {
            Id = assignment.Id,
            PatrolZoneId = assignment.PatrolZoneId,
            PatrolZoneName = assignment.PatrolZone.Name,
            VehicleId = assignment.VehicleId,
            VehicleCallsign = assignment.Vehicle.Callsign,
            VehicleType = assignment.Vehicle.Type,
            AssignedAt = assignment.AssignedAt,
            UnassignedAt = assignment.UnassignedAt,
            IsActive = assignment.IsActive,
            AssignedByUserId = assignment.AssignedByUserId,
            AssignedByUserName = assignment.AssignedByUser.Name,
            UnassignedByUserId = assignment.UnassignedByUserId,
            UnassignedByUserName = assignment.UnassignedByUser?.Name,
            Notes = assignment.Notes
        };
    }
}