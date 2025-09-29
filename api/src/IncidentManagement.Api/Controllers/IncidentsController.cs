using Microsoft.AspNetCore.Mvc;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Application.Services;
using IncidentManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace IncidentManagement.Api.Controllers;

/// <summary>
/// Controller for managing incidents with agency-based filtering
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IncidentsController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly IRealTimeNotificationService _realTimeService;

    public IncidentsController(IncidentManagementDbContext context, IRealTimeNotificationService realTimeService)
    {
        _context = context;
        _realTimeService = realTimeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<IncidentDto>>> GetIncidents(
        [FromQuery] int? stationId,
        [FromQuery] IncidentStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var userAgencyId = GetCurrentUserAgencyId();
        var userStationId = GetCurrentUserStationId();
        var userRole = GetCurrentUserRole();

        if (userAgencyId == null)
        {
            return Unauthorized("User agency information not found");
        }

        var query = _context.Incidents
            .Include(i => i.Assignments)
            .Include(i => i.Logs)
            .Include(i => i.Callers)
            .Include(i => i.CreatedBy)
            .Include(i => i.ClosedBy)
            .Include(i => i.Station)
            .Include(i => i.Involvement)
            .Include(i => i.Commanders)
                .ThenInclude(c => c.Personnel)
            .Include(i => i.Commanders)
                .ThenInclude(c => c.AssignedBy)
            .Include(i => i.Injuries)
            .Include(i => i.Deaths)
            .Include(i => i.Fire)
            .Include(i => i.Damage)
            .AsQueryable();

        // Apply role-based filtering
        if (IsMember() && userStationId.HasValue)
        {
            // Agency members see incidents for their station OR incidents where their station's vehicles are assigned
            query = query.Where(i =>
                (i.StationId == userStationId.Value && i.AgencyId == userAgencyId) ||
                (i.AgencyId == userAgencyId && i.Assignments.Any(a =>
                    a.ResourceType == ResourceType.Vehicle &&
                    _context.Vehicles.Any(v => v.Id == a.ResourceId && v.StationId == userStationId.Value)
                ))
            );
        }
        else if (IsDispatcher())
        {
            // Dispatchers see all incidents from their agency
            query = query.Where(i => i.AgencyId == userAgencyId);
        }
        else
        {
            // Fallback: filter by agency at minimum
            query = query.Where(i => i.AgencyId == userAgencyId);
        }

        if (stationId.HasValue)
            query = query.Where(i => i.StationId == stationId.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (from.HasValue)
            query = query.Where(i => i.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(i => i.CreatedAt <= to.Value);

        var incidents = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();


        var result = incidents.Select(i => new IncidentDto
        {
            Id = i.Id,
            AgencyId = i.AgencyId,
            StationId = i.StationId,
            MainCategory = i.MainCategory,
            SubCategory = i.SubCategory,
            Address = i.Address,
            Street = i.Street,
            StreetNumber = i.StreetNumber,
            City = i.City,
            Region = i.Region,
            PostalCode = i.PostalCode,
            Country = i.Country,
            Latitude = i.Latitude,
            Longitude = i.Longitude,
            Status = i.Status,
            Priority = i.Priority,
            Notes = i.Notes,
            CreatedByUserId = i.CreatedByUserId,
            CreatedByName = i.CreatedBy?.Name ?? "",
            CreatedAt = i.CreatedAt,
            IsClosed = i.IsClosed,
            ClosureReason = i.ClosureReason,
            ClosedAt = i.ClosedAt,
            ClosedByUserId = i.ClosedByUserId,
            ClosedByName = i.ClosedBy?.Name ?? "",
            ParticipationType = DetermineParticipationType(i, userStationId),
            Assignments = i.Assignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                IncidentId = a.IncidentId,
                ResourceType = a.ResourceType,
                ResourceId = a.ResourceId,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                DispatchedAt = a.DispatchedAt,
                EnRouteAt = a.EnRouteAt,
                OnSceneAt = a.OnSceneAt,
                CompletedAt = a.CompletedAt
            }).ToList(),
            Logs = i.Logs.Select(l => new IncidentLogDto
            {
                Id = l.Id,
                IncidentId = l.IncidentId,
                At = l.At,
                Message = l.Message,
                By = l.By
            }).ToList(),
            Callers = i.Callers.Select(c => new CallerDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                CalledAt = c.CalledAt,
                Notes = c.Notes
            }).ToList(),

            // New detailed incident information
            Involvement = i.Involvement != null ? new IncidentInvolvementDto
            {
                Id = i.Involvement.Id,
                IncidentId = i.Involvement.IncidentId,
                FireTrucksNumber = i.Involvement.FireTrucksNumber,
                FirePersonnel = i.Involvement.FirePersonnel,
                OtherAgencies = i.Involvement.OtherAgencies,
                ServiceActions = i.Involvement.ServiceActions,
                RescuedPeople = i.Involvement.RescuedPeople,
                RescueInformation = i.Involvement.RescueInformation,
                CreatedAt = i.Involvement.CreatedAt,
                UpdatedAt = i.Involvement.UpdatedAt
            } : null,

            Commanders = i.Commanders.Select(c => new IncidentCommanderDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                PersonnelId = c.PersonnelId,
                PersonnelName = c.Personnel?.Name ?? "",
                PersonnelBadgeNumber = c.Personnel?.BadgeNumber ?? "",
                PersonnelRank = c.Personnel?.Rank ?? "",
                Observations = c.Observations,
                AssignedAt = c.AssignedAt,
                AssignedByUserId = c.AssignedByUserId,
                AssignedByName = c.AssignedBy?.Name ?? ""
            }).ToList(),

            Injuries = i.Injuries.Select(inj => new InjuryDto
            {
                Id = inj.Id,
                Name = inj.Name,
                Type = inj.Type,
                Description = inj.Description,
                CreatedAt = inj.CreatedAt
            }).ToList(),

            Deaths = i.Deaths.Select(d => new DeathDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                Description = d.Description,
                CreatedAt = d.CreatedAt
            }).ToList(),

            Fire = i.Fire != null ? new IncidentFireDto
            {
                Id = i.Fire.Id,
                IncidentId = i.Fire.IncidentId,
                BurnedArea = i.Fire.BurnedArea,
                BurnedItems = i.Fire.BurnedItems,
                CreatedAt = i.Fire.CreatedAt,
                UpdatedAt = i.Fire.UpdatedAt
            } : null,

            Damage = i.Damage != null ? new IncidentDamageDto
            {
                Id = i.Damage.Id,
                IncidentId = i.Damage.IncidentId,
                OwnerName = i.Damage.OwnerName,
                TenantName = i.Damage.TenantName,
                DamageAmount = i.Damage.DamageAmount,
                SavedProperty = i.Damage.SavedProperty,
                IncidentCause = i.Damage.IncidentCause,
                CreatedAt = i.Damage.CreatedAt,
                UpdatedAt = i.Damage.UpdatedAt
            } : null
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<IncidentDto>> GetIncident(int id)
    {
        var incident = await _context.Incidents
            .Include(i => i.Assignments)
            .Include(i => i.Logs)
            .Include(i => i.Callers)
            .Include(i => i.CreatedBy)
            .Include(i => i.ClosedBy)
            .Include(i => i.Station)
            .Include(i => i.Involvement)
            .Include(i => i.Commanders)
                .ThenInclude(c => c.Personnel)
            .Include(i => i.Commanders)
                .ThenInclude(c => c.AssignedBy)
            .Include(i => i.Injuries)
            .Include(i => i.Deaths)
            .Include(i => i.Fire)
            .Include(i => i.Damage)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null)
            return NotFound();

        // Check if user can view this incident
        var userAgencyId = GetCurrentUserAgencyId();
        var userStationId = GetCurrentUserStationId();

        // Check agency access first
        if (incident.AgencyId != userAgencyId)
        {
            return Forbid("You can only view incidents in your agency");
        }

        if (IsMember() && userStationId.HasValue)
        {
            // Agency members can view incidents at their station OR incidents where their station's vehicles are assigned
            bool isStationIncident = incident.StationId == userStationId.Value;
            bool hasStationVehicleAssigned = incident.Assignments.Any(a =>
                a.ResourceType == ResourceType.Vehicle &&
                _context.Vehicles.Any(v => v.Id == a.ResourceId && v.StationId == userStationId.Value)
            );

            if (!isStationIncident && !hasStationVehicleAssigned)
            {
                return Forbid("You can only view incidents assigned to your station or where your vehicles are assigned");
            }
        }

        var result = new IncidentDto
        {
            Id = incident.Id,
            AgencyId = incident.AgencyId,
            StationId = incident.StationId,
            MainCategory = incident.MainCategory,
            SubCategory = incident.SubCategory,
            Address = incident.Address,
            Street = incident.Street,
            StreetNumber = incident.StreetNumber,
            City = incident.City,
            Region = incident.Region,
            PostalCode = incident.PostalCode,
            Country = incident.Country,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            Status = incident.Status,
            Priority = incident.Priority,
            Notes = incident.Notes,
            CreatedByUserId = incident.CreatedByUserId,
            CreatedByName = incident.CreatedBy?.Name ?? "",
            CreatedAt = incident.CreatedAt,
            IsClosed = incident.IsClosed,
            ClosureReason = incident.ClosureReason,
            ClosedAt = incident.ClosedAt,
            ClosedByUserId = incident.ClosedByUserId,
            ClosedByName = incident.ClosedBy?.Name ?? "",
            ParticipationType = DetermineParticipationType(incident, userStationId),
            Assignments = incident.Assignments.Select(a => new AssignmentDto
            {
                Id = a.Id,
                IncidentId = a.IncidentId,
                ResourceType = a.ResourceType,
                ResourceId = a.ResourceId,
                Status = a.Status,
                CreatedAt = a.CreatedAt,
                DispatchedAt = a.DispatchedAt,
                EnRouteAt = a.EnRouteAt,
                OnSceneAt = a.OnSceneAt,
                CompletedAt = a.CompletedAt
            }).ToList(),
            Logs = incident.Logs.Select(l => new IncidentLogDto
            {
                Id = l.Id,
                IncidentId = l.IncidentId,
                At = l.At,
                Message = l.Message,
                By = l.By
            }).ToList(),
            Callers = incident.Callers.Select(c => new CallerDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                CalledAt = c.CalledAt,
                Notes = c.Notes
            }).ToList(),

            // New detailed incident information
            Involvement = incident.Involvement != null ? new IncidentInvolvementDto
            {
                Id = incident.Involvement.Id,
                IncidentId = incident.Involvement.IncidentId,
                FireTrucksNumber = incident.Involvement.FireTrucksNumber,
                FirePersonnel = incident.Involvement.FirePersonnel,
                OtherAgencies = incident.Involvement.OtherAgencies,
                ServiceActions = incident.Involvement.ServiceActions,
                RescuedPeople = incident.Involvement.RescuedPeople,
                RescueInformation = incident.Involvement.RescueInformation,
                CreatedAt = incident.Involvement.CreatedAt,
                UpdatedAt = incident.Involvement.UpdatedAt
            } : null,

            Commanders = incident.Commanders.Select(c => new IncidentCommanderDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                PersonnelId = c.PersonnelId,
                PersonnelName = c.Personnel?.Name ?? "",
                PersonnelBadgeNumber = c.Personnel?.BadgeNumber ?? "",
                PersonnelRank = c.Personnel?.Rank ?? "",
                Observations = c.Observations,
                AssignedAt = c.AssignedAt,
                AssignedByUserId = c.AssignedByUserId,
                AssignedByName = c.AssignedBy?.Name ?? ""
            }).ToList(),

            Injuries = incident.Injuries.Select(inj => new InjuryDto
            {
                Id = inj.Id,
                Name = inj.Name,
                Type = inj.Type,
                Description = inj.Description,
                CreatedAt = inj.CreatedAt
            }).ToList(),

            Deaths = incident.Deaths.Select(d => new DeathDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type,
                Description = d.Description,
                CreatedAt = d.CreatedAt
            }).ToList(),

            Fire = incident.Fire != null ? new IncidentFireDto
            {
                Id = incident.Fire.Id,
                IncidentId = incident.Fire.IncidentId,
                BurnedArea = incident.Fire.BurnedArea,
                BurnedItems = incident.Fire.BurnedItems,
                CreatedAt = incident.Fire.CreatedAt,
                UpdatedAt = incident.Fire.UpdatedAt
            } : null,

            Damage = incident.Damage != null ? new IncidentDamageDto
            {
                Id = incident.Damage.Id,
                IncidentId = incident.Damage.IncidentId,
                OwnerName = incident.Damage.OwnerName,
                TenantName = incident.Damage.TenantName,
                DamageAmount = incident.Damage.DamageAmount,
                SavedProperty = incident.Damage.SavedProperty,
                IncidentCause = incident.Damage.IncidentCause,
                CreatedAt = incident.Damage.CreatedAt,
                UpdatedAt = incident.Damage.UpdatedAt
            } : null
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<IncidentDto>> CreateIncident(CreateIncidentDto dto)
    {
        // Only dispatchers can create incidents
        if (!CanCreateIncidents())
        {
            return Forbid("Only dispatchers can create incidents");
        }

        // Get the station to determine the agency
        var station = await _context.Stations.FindAsync(dto.StationId);
        if (station == null)
        {
            return BadRequest("Invalid station ID");
        }

        var incident = new Incident
        {
            AgencyId = station.AgencyId,
            StationId = dto.StationId,
            MainCategory = dto.MainCategory,
            SubCategory = dto.SubCategory,
            Address = dto.Address,
            Street = dto.Street,
            StreetNumber = dto.StreetNumber,
            City = dto.City,
            Region = dto.Region,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Status = IncidentStatus.Created,
            Priority = dto.Priority,
            Notes = dto.Notes,
            CreatedByUserId = dto.CreatedByUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Incidents.Add(incident);
        await _context.SaveChangesAsync();

        // Add callers if provided
        if (dto.Callers.Any())
        {
            var callers = dto.Callers.Select(c => new Caller
            {
                IncidentId = incident.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                CalledAt = c.CalledAt ?? DateTime.UtcNow,
                Notes = c.Notes
            }).ToList();

            _context.Callers.AddRange(callers);
            await _context.SaveChangesAsync();
        }

        // Add initial log entry
        var log = new IncidentLog
        {
            IncidentId = incident.Id,
            Message = $"Incident Created: {incident.MainCategory} - {incident.SubCategory} - Priority: {incident.Priority}",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(log);

        // Automatically transition to OnGoing status
        incident.Status = IncidentStatus.OnGoing;
        var statusLog = new IncidentLog
        {
            IncidentId = incident.Id,
            Message = "Status changed from Created to OnGoing",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(statusLog);
        await _context.SaveChangesAsync();

        // Notify station personnel about the new incident
        await _realTimeService.NotifyIncidentAssigned(incident.Id, incident.StationId);

        // Load callers for the response
        var incidentCallers = await _context.Callers
            .Where(c => c.IncidentId == incident.Id)
            .ToListAsync();

        return CreatedAtAction(nameof(GetIncident), new { id = incident.Id }, new IncidentDto
        {
            Id = incident.Id,
            AgencyId = incident.AgencyId,
            StationId = incident.StationId,
            MainCategory = incident.MainCategory,
            SubCategory = incident.SubCategory,
            Address = incident.Address,
            Street = incident.Street,
            StreetNumber = incident.StreetNumber,
            City = incident.City,
            Region = incident.Region,
            PostalCode = incident.PostalCode,
            Country = incident.Country,
            Latitude = incident.Latitude,
            Longitude = incident.Longitude,
            Status = incident.Status,
            Priority = incident.Priority,
            Notes = incident.Notes,
            CreatedByUserId = incident.CreatedByUserId,
            CreatedAt = incident.CreatedAt,
            Callers = incidentCallers.Select(c => new CallerDto
            {
                Id = c.Id,
                IncidentId = c.IncidentId,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                CalledAt = c.CalledAt,
                Notes = c.Notes
            }).ToList()
        });
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateIncidentStatus(int id, UpdateIncidentStatusDto dto)
    {
        // Only dispatchers can update incident status
        if (!CanAssignResources())
        {
            return Forbid("Only dispatchers can update incident status");
        }
        var incident = await _context.Incidents
            .Include(i => i.Assignments)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null)
            return NotFound();

        var oldStatus = incident.Status;
        incident.Status = dto.Status;

        // Set closed time if status is closed
        if (dto.Status == IncidentStatus.Closed)
        {
            incident.IsClosed = true;
            incident.ClosedAt = DateTime.UtcNow;

            // Automatically set all assigned vehicles to "Finished" status
            var vehicleAssignments = incident.Assignments
                .Where(a => a.ResourceType == ResourceType.Vehicle && a.Status != "Finished")
                .ToList();

            foreach (var assignment in vehicleAssignments)
            {
                var oldAssignmentStatus = assignment.Status;
                assignment.Status = "Finished";
                assignment.CompletedAt = DateTime.UtcNow;

                // Update the vehicle status to Available (since it's no longer assigned to any incident)
                var vehicle = await _context.Vehicles.FindAsync(assignment.ResourceId);
                if (vehicle != null)
                {
                    vehicle.Status = VehicleStatus.Available;
                }

                // Add log entry for each vehicle status change
                var vehicleLog = new IncidentLog
                {
                    IncidentId = id,
                    Message = $"Vehicle {assignment.ResourceId} status automatically changed from {oldAssignmentStatus} to Finished due to incident closure",
                    At = DateTime.UtcNow,
                    By = "System"
                };
                _context.IncidentLogs.Add(vehicleLog);
            }
        }

        // Add log entry for status change
        var log = new IncidentLog
        {
            IncidentId = incident.Id,
            Message = $"Status changed from {oldStatus} to {dto.Status}",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(log);

        await _context.SaveChangesAsync();

        // Notify relevant users about status change
        await _realTimeService.NotifyIncidentStatusUpdate(id, dto.Status);

        return NoContent();
    }

    [HttpPost("{id}/assign")]
    public async Task<ActionResult<AssignmentDto>> AssignResource(int id, CreateAssignmentDto dto)
    {
        // Only dispatchers can assign resources
        if (!CanAssignResources())
        {
            return Forbid("Only dispatchers can assign resources to incidents");
        }
        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null)
            return NotFound();

        // Check if the resource is already assigned to an active incident with non-finished status
        var existingAssignment = await _context.Assignments
            .Include(a => a.Incident)
            .FirstOrDefaultAsync(a => 
                a.ResourceType == dto.ResourceType && 
                a.ResourceId == dto.ResourceId &&
                a.Incident.Status != IncidentStatus.Closed &&
                a.Incident.Status != IncidentStatus.FullyControlled &&
                a.Status != "Finished");

        if (existingAssignment != null)
        {
            var resourceTypeName = dto.ResourceType == ResourceType.Vehicle ? "Vehicle" : "Personnel";
            return Conflict($"{resourceTypeName} {dto.ResourceId} is already assigned to incident #{existingAssignment.IncidentId}");
        }

        // Allow assignment even if the resource was previously assigned to this incident with finished status
        // This creates a new assignment entry for tracking purposes

        // Check if the resource is already assigned to this incident with non-finished status
        var duplicateAssignment = await _context.Assignments
            .FirstOrDefaultAsync(a => 
                a.IncidentId == id &&
                a.ResourceType == dto.ResourceType && 
                a.ResourceId == dto.ResourceId &&
                a.Status != "Finished");

        if (duplicateAssignment != null)
        {
            var resourceTypeName = dto.ResourceType == ResourceType.Vehicle ? "Vehicle" : "Personnel";
            return Conflict($"{resourceTypeName} {dto.ResourceId} is already assigned to this incident");
        }

        var assignment = new Assignment
        {
            IncidentId = id,
            ResourceType = dto.ResourceType,
            ResourceId = dto.ResourceId,
            Status = "Notified",
            CreatedAt = DateTime.UtcNow,
            DispatchedAt = DateTime.UtcNow
        };

        _context.Assignments.Add(assignment);

        // Add log entry
        var resourceName = dto.ResourceType == ResourceType.Vehicle ? "Vehicle" : "Personnel";
        var log = new IncidentLog
        {
            IncidentId = id,
            Message = $"{resourceName} {dto.ResourceId} assigned to incident",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(log);

        await _context.SaveChangesAsync();

        // Send notifications
        if (dto.ResourceType == ResourceType.Vehicle)
        {
            await _realTimeService.NotifyVehicleAssigned(id, dto.ResourceId);
        }
        else
        {
            await _realTimeService.NotifyPersonnelAssigned(id, dto.ResourceId);
        }

        return Ok(new AssignmentDto
        {
            Id = assignment.Id,
            IncidentId = assignment.IncidentId,
            ResourceType = assignment.ResourceType,
            ResourceId = assignment.ResourceId,
            Status = assignment.Status,
            CreatedAt = assignment.CreatedAt,
            DispatchedAt = assignment.DispatchedAt
        });
    }

    [HttpPost("{id}/logs")]
    public async Task<ActionResult<IncidentLogDto>> AddLog(int id, CreateIncidentLogDto dto)
    {
        var incident = await _context.Incidents
            .Include(i => i.Station)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null)
            return NotFound();

        // Check if user can add logs to this incident
        var userAgencyId = GetCurrentUserAgencyId();
        var userStationId = GetCurrentUserStationId();

        // Check agency access first
        if (incident.AgencyId != userAgencyId)
        {
            return Forbid("You can only add notes to incidents in your agency");
        }

        if (IsMember() && userStationId.HasValue)
        {
            // Agency members can add logs to incidents at their station OR incidents where their station's vehicles are assigned
            bool isStationIncident = incident.StationId == userStationId.Value;
            bool hasStationVehicleAssigned = incident.Assignments.Any(a =>
                a.ResourceType == ResourceType.Vehicle &&
                _context.Vehicles.Any(v => v.Id == a.ResourceId && v.StationId == userStationId.Value)
            );

            if (!isStationIncident && !hasStationVehicleAssigned)
            {
                return Forbid("You can only add notes to incidents assigned to your station or where your vehicles are assigned");
            }
        }

        var log = new IncidentLog
        {
            IncidentId = id,
            Message = dto.Message,
            By = dto.By,
            At = DateTime.UtcNow
        };

        _context.IncidentLogs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(new IncidentLogDto
        {
            Id = log.Id,
            IncidentId = log.IncidentId,
            At = log.At,
            Message = log.Message,
            By = log.By
        });
    }

    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetIncidentStatistics([FromQuery] int? stationId)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var userStationId = GetCurrentUserStationId();
            var userRole = GetCurrentUserRole();

            if (userAgencyId == null)
            {
                return Unauthorized("User agency information not found");
            }

            var query = _context.Incidents.Include(i => i.Station).AsQueryable();

            // Apply role-based filtering
            if (IsMember() && userStationId.HasValue)
            {
                query = query.Where(i => i.StationId == userStationId.Value && i.AgencyId == userAgencyId);
            }
            else if (IsDispatcher())
            {
                query = query.Where(i => i.AgencyId == userAgencyId);
            }
            else
            {
                query = query.Where(i => i.AgencyId == userAgencyId);
            }

            if (stationId.HasValue)
                query = query.Where(i => i.StationId == stationId.Value);

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

            // Get basic counts first
            var total = await query.CountAsync();
            var active = await query.CountAsync(i => i.Status != IncidentStatus.Closed);
            var thisMonth = await query.CountAsync(i => i.CreatedAt >= startOfMonth);
            var thisWeek = await query.CountAsync(i => i.CreatedAt >= startOfWeek);

            // Get closed incidents for average calculation
            var closedIncidents = await query
                .Where(i => i.ClosedAt.HasValue)
                .Select(i => new { i.CreatedAt, i.ClosedAt })
                .ToListAsync();

            var averageResponseTime = closedIncidents.Any()
                ? closedIncidents.Average(i => (i.ClosedAt!.Value - i.CreatedAt).TotalMinutes)
                : (double?)null;

            var statistics = new
            {
                Total = total,
                Active = active,
                ThisMonth = thisMonth,
                ThisWeek = thisWeek,
                ByStatus = await query
                    .GroupBy(i => i.Status)
                    .Select(g => new { Status = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync(),
                ByPriority = await query
                    .GroupBy(i => i.Priority)
                    .Select(g => new { Priority = g.Key.ToString(), Count = g.Count() })
                    .ToListAsync(),
                ByCategory = await query
                    .GroupBy(i => i.MainCategory)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(10)
                    .ToListAsync(),
                AverageResponseTime = averageResponseTime
            };

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
        }
    }

    [HttpGet("stats-simple")]
    public async Task<ActionResult<object>> GetSimpleStatistics()
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            var userStationId = GetCurrentUserStationId();
            var userRole = GetCurrentUserRole();

            if (userAgencyId == null)
            {
                return Unauthorized("User agency information not found");
            }

            var query = _context.Incidents.Include(i => i.Station).AsQueryable();

            // Apply role-based filtering
            if (IsMember() && userStationId.HasValue)
            {
                query = query.Where(i => i.StationId == userStationId.Value && i.AgencyId == userAgencyId);
            }
            else if (IsDispatcher())
            {
                query = query.Where(i => i.AgencyId == userAgencyId);
            }
            else
            {
                query = query.Where(i => i.AgencyId == userAgencyId);
            }

            var total = await query.CountAsync();
            var active = await query.CountAsync(i => i.Status != IncidentStatus.Closed);

            return Ok(new
            {
                total,
                active,
                timestamp = DateTime.UtcNow,
                message = "Simple statistics working"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpDelete("{id}/assignments/{assignmentId}")]
    public async Task<IActionResult> UnassignResource(int id, int assignmentId)
    {
        // Only dispatchers can unassign resources
        if (!CanAssignResources())
        {
            return Forbid("Only dispatchers can unassign resources from incidents");
        }

        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null)
            return NotFound("Incident not found");

        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment == null)
            return NotFound("Assignment not found");

        if (assignment.IncidentId != id)
            return BadRequest("Assignment does not belong to this incident");

        // Add log entry
        var resourceName = assignment.ResourceType == ResourceType.Vehicle ? "Vehicle" : "Personnel";
        var log = new IncidentLog
        {
            IncidentId = id,
            Message = $"{resourceName} {assignment.ResourceId} unassigned from incident",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(log);

        // Remove the assignment
        _context.Assignments.Remove(assignment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}/assignments/{assignmentId}/status")]
    public async Task<IActionResult> UpdateAssignmentStatus(int id, int assignmentId, [FromBody] UpdateAssignmentStatusDto dto)
    {
        // Only dispatchers can update assignment status
        if (!CanAssignResources())
        {
            return Forbid("Only dispatchers can update assignment status");
        }

        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null)
            return NotFound("Incident not found");

        var assignment = await _context.Assignments.FindAsync(assignmentId);
        if (assignment == null)
            return NotFound("Assignment not found");

        if (assignment.IncidentId != id)
            return BadRequest("Assignment does not belong to this incident");

        var oldStatus = assignment.Status;
        assignment.Status = dto.Status;

        // Set timestamp based on status
        var now = DateTime.UtcNow;
        switch (dto.Status.ToLower())
        {
            case "created":
            case "notified":
                assignment.DispatchedAt = now;
                break;
            case "dispatched":
                assignment.DispatchedAt = now;
                break;
            case "enroute":
            case "en route":
                assignment.EnRouteAt = now;
                break;
            case "onscene":
            case "on scene":
                assignment.OnSceneAt = now;
                break;
            case "completed":
            case "finished":
                assignment.CompletedAt = now;
                break;
        }

        // Add log entry
        var resourceName = assignment.ResourceType == ResourceType.Vehicle ? "Vehicle" : "Personnel";
        var log = new IncidentLog
        {
            IncidentId = id,
            Message = $"{resourceName} {assignment.ResourceId} status changed from {oldStatus} to {dto.Status}",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(log);

        await _context.SaveChangesAsync();

        // Send real-time notifications for assignment status change
        await _realTimeService.BroadcastAssignmentStatusChanged(id, assignmentId, dto.Status, oldStatus, incident.StationId, incident.AgencyId);

        // Send real-time notification for new incident log
        await _realTimeService.BroadcastIncidentLogAdded(id, log.Message, log.At, log.By, incident.StationId, incident.AgencyId);

        return NoContent();
    }

    /// <summary>
    /// Close an incident with a closure reason
    /// </summary>
    /// <param name="id">Incident ID</param>
    /// <param name="dto">Close incident data</param>
    /// <returns>No content</returns>
    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseIncident(int id, CloseIncidentDto dto)
    {
        // Only dispatchers can close incidents
        if (!CanCreateIncidents())
        {
            return Forbid("Only dispatchers can close incidents");
        }

        var incident = await _context.Incidents
            .Include(i => i.Logs)
            .Include(i => i.Assignments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null)
            return NotFound();

        // Check if user can modify this incident
        var userAgencyId = GetCurrentUserAgencyId();
        if (incident.AgencyId != userAgencyId)
        {
            return Forbid("You can only close incidents in your agency");
        }

        // Check if incident is already closed
        if (incident.IsClosed)
        {
            return BadRequest("Incident is already closed");
        }

        // Close the incident
        incident.IsClosed = true;
        incident.ClosureReason = dto.ClosureReason;
        incident.ClosedAt = DateTime.UtcNow;
        incident.ClosedByUserId = dto.ClosedByUserId;
        
        // Automatically set status to FullyControlled
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.FullyControlled;

        // Automatically set all assigned vehicles to "Finished" status
        var vehicleAssignments = incident.Assignments
            .Where(a => a.ResourceType == ResourceType.Vehicle && a.Status != "Finished")
            .ToList();

        var vehicleStatusChanges = new List<(Assignment assignment, string oldStatus)>();

        foreach (var assignment in vehicleAssignments)
        {
            var oldAssignmentStatus = assignment.Status;
            assignment.Status = "Finished";
            assignment.CompletedAt = DateTime.UtcNow;

            // Update the vehicle status to Available (since it's no longer assigned to any incident)
            var vehicle = await _context.Vehicles.FindAsync(assignment.ResourceId);
            if (vehicle != null)
            {
                vehicle.Status = VehicleStatus.Available;
            }

            vehicleStatusChanges.Add((assignment, oldAssignmentStatus));

            // Add log entry for each vehicle status change
            var vehicleLog = new IncidentLog
            {
                IncidentId = id,
                Message = $"Vehicle {assignment.ResourceId} status automatically changed from {oldAssignmentStatus} to Finished due to incident closure",
                At = DateTime.UtcNow,
                By = "System"
            };
            _context.IncidentLogs.Add(vehicleLog);
        }

        // Add log entry for closure
        var closureReasonText = dto.ClosureReason switch
        {
            IncidentClosureReason.Action => "ΕΝΕΡΓΕΙΑ (Action)",
            IncidentClosureReason.WithoutAction => "ΑΝΕΥ ΕΝΕΡΓΕΙΑΣ (Without Action)",
            IncidentClosureReason.PreArrival => "ΠΡΟ ΑΦΙΞΕΩΣ (Pre-Arrival)",
            IncidentClosureReason.Cancelled => "ΑΚΥΡΟ (Cancelled)",
            IncidentClosureReason.FalseAlarm => "ΨΕΥΔΗΣ ΑΝΑΓΓΕΛΙΑ (False Alarm)",
            _ => dto.ClosureReason.ToString()
        };

        var closureLog = new IncidentLog
        {
            IncidentId = id,
            Message = $"Incident closed with reason: {closureReasonText}",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(closureLog);

        // Add log entry for status change
        var statusLog = new IncidentLog
        {
            IncidentId = id,
            Message = $"Status changed from {oldStatus} to {incident.Status} due to incident closure",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(statusLog);

        await _context.SaveChangesAsync();

        // Send real-time notifications
        await _realTimeService.NotifyIncidentStatusUpdate(id, incident.Status);
        await _realTimeService.BroadcastIncidentLogAdded(id, closureLog.Message, closureLog.At, closureLog.By, incident.StationId, incident.AgencyId);
        await _realTimeService.BroadcastIncidentLogAdded(id, statusLog.Message, statusLog.At, statusLog.By, incident.StationId, incident.AgencyId);

        // Send real-time notifications for vehicle status changes
        foreach (var (assignment, previousStatus) in vehicleStatusChanges)
        {
            await _realTimeService.BroadcastAssignmentStatusChanged(id, assignment.Id, "Finished", previousStatus, incident.StationId, incident.AgencyId);
            
            // Find the corresponding vehicle log and broadcast it
            var vehicleLog = _context.IncidentLogs
                .Where(l => l.IncidentId == id && l.Message.Contains($"Vehicle {assignment.ResourceId}"))
                .OrderByDescending(l => l.At)
                .FirstOrDefault();
            
            if (vehicleLog != null)
            {
                await _realTimeService.BroadcastIncidentLogAdded(id, vehicleLog.Message, vehicleLog.At, vehicleLog.By, incident.StationId, incident.AgencyId);
            }
        }

        return NoContent();
    }

    /// <summary>
    /// Reopen a closed incident
    /// </summary>
    /// <param name="id">Incident ID</param>
    /// <returns>No content</returns>
    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> ReopenIncident(int id)
    {
        // Only dispatchers can reopen incidents
        if (!CanCreateIncidents())
        {
            return Forbid("Only dispatchers can reopen incidents");
        }

        var incident = await _context.Incidents
            .Include(i => i.Logs)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (incident == null)
            return NotFound();

        // Check if user can modify this incident
        var userAgencyId = GetCurrentUserAgencyId();
        if (incident.AgencyId != userAgencyId)
        {
            return Forbid("You can only reopen incidents in your agency");
        }

        // Check if incident is actually closed
        if (!incident.IsClosed)
        {
            return BadRequest("Incident is not closed");
        }

        // Reopen the incident
        incident.IsClosed = false;
        incident.ClosureReason = null;
        incident.ClosedAt = null;
        incident.ClosedByUserId = null;
        
        // Set status back to OnGoing
        var oldStatus = incident.Status;
        incident.Status = IncidentStatus.OnGoing;

        // Add log entry for reopening
        var reopenLog = new IncidentLog
        {
            IncidentId = id,
            Message = "Incident reopened by dispatcher",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(reopenLog);

        // Add log entry for status change
        var statusLog = new IncidentLog
        {
            IncidentId = id,
            Message = $"Status changed from {oldStatus} to {incident.Status} due to incident reopening",
            At = DateTime.UtcNow,
            By = "System"
        };
        _context.IncidentLogs.Add(statusLog);

        await _context.SaveChangesAsync();

        // Send real-time notifications
        await _realTimeService.BroadcastIncidentLogAdded(id, reopenLog.Message, reopenLog.At, reopenLog.By, incident.StationId, incident.AgencyId);
        await _realTimeService.BroadcastIncidentLogAdded(id, statusLog.Message, statusLog.At, statusLog.By, incident.StationId, incident.AgencyId);
        await _realTimeService.NotifyIncidentStatusUpdate(id, incident.Status);

        return NoContent();
    }

    private string DetermineParticipationType(Incident incident, int? userStationId)
    {
        if (!userStationId.HasValue)
            return "Primary";

        if (incident.StationId == userStationId.Value)
            return "Primary";

        var hasStationVehicleAssigned = incident.Assignments.Any(a =>
            a.ResourceType == ResourceType.Vehicle &&
            _context.Vehicles.Any(v => v.Id == a.ResourceId && v.StationId == userStationId.Value)
        );

        return hasStationVehicleAssigned ? "Reinforcement" : "Primary";
    }

    // Incident Involvement endpoints
    [HttpPut("{id}/involvement")]
    public async Task<ActionResult> UpdateIncidentInvolvement(int id, UpdateIncidentInvolvementDto dto)
    {
        var incident = await _context.Incidents.Include(i => i.Involvement).FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null) return NotFound();

        if (incident.Involvement == null)
        {
            incident.Involvement = new IncidentInvolvement
            {
                IncidentId = id,
                CreatedAt = DateTime.UtcNow
            };
            _context.IncidentInvolvements.Add(incident.Involvement);
        }

        incident.Involvement.FireTrucksNumber = dto.FireTrucksNumber;
        incident.Involvement.FirePersonnel = dto.FirePersonnel;
        incident.Involvement.OtherAgencies = dto.OtherAgencies;
        incident.Involvement.ServiceActions = dto.ServiceActions;
        incident.Involvement.RescuedPeople = dto.RescuedPeople;
        incident.Involvement.RescueInformation = dto.RescueInformation;
        incident.Involvement.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        return NoContent();
    }

    // Incident Commander endpoints
    [HttpPost("{id}/commanders")]
    public async Task<ActionResult<IncidentCommanderDto>> AddIncidentCommander(int id, CreateIncidentCommanderDto dto)
    {
        var incident = await _context.Incidents.FindAsync(id);
        if (incident == null) return NotFound();

        var personnel = await _context.Personnel.FindAsync(dto.PersonnelId);
        if (personnel == null) return BadRequest("Personnel not found");

        var commander = new IncidentCommander
        {
            IncidentId = id,
            PersonnelId = dto.PersonnelId,
            Observations = dto.Observations,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = dto.AssignedByUserId
        };

        _context.IncidentCommanders.Add(commander);
        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        var result = new IncidentCommanderDto
        {
            Id = commander.Id,
            IncidentId = commander.IncidentId,
            PersonnelId = commander.PersonnelId,
            PersonnelName = personnel.Name,
            PersonnelBadgeNumber = personnel.BadgeNumber ?? "",
            PersonnelRank = personnel.Rank ?? "",
            Observations = commander.Observations,
            AssignedAt = commander.AssignedAt,
            AssignedByUserId = commander.AssignedByUserId
        };

        return CreatedAtAction(nameof(GetIncident), new { id }, result);
    }

    [HttpPut("{id}/commanders/{commanderId}")]
    public async Task<ActionResult> UpdateIncidentCommander(int id, int commanderId, UpdateIncidentCommanderDto dto)
    {
        var commander = await _context.IncidentCommanders.FirstOrDefaultAsync(c => c.Id == commanderId && c.IncidentId == id);
        if (commander == null) return NotFound();

        commander.Observations = dto.Observations;
        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        return NoContent();
    }

    [HttpDelete("{id}/commanders/{commanderId}")]
    public async Task<ActionResult> RemoveIncidentCommander(int id, int commanderId)
    {
        var commander = await _context.IncidentCommanders.FirstOrDefaultAsync(c => c.Id == commanderId && c.IncidentId == id);
        if (commander == null) return NotFound();

        _context.IncidentCommanders.Remove(commander);
        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        return NoContent();
    }

    // Incident Casualties endpoints
    [HttpPut("{id}/casualties")]
    public async Task<ActionResult> UpdateIncidentCasualties(int id, UpdateIncidentCasualtiesDto dto)
    {
        var incident = await _context.Incidents
            .Include(i => i.Injuries)
            .Include(i => i.Deaths)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null) return NotFound();

        // Handle injury records
        if (dto.Injuries != null)
        {
            // Remove all existing injuries
            _context.Injuries.RemoveRange(incident.Injuries);

            // Add new injuries
            foreach (var injuryDto in dto.Injuries)
            {
                var injury = new Injury
                {
                    IncidentId = id,
                    Name = injuryDto.Name,
                    Type = injuryDto.Type,
                    Description = injuryDto.Description,
                    CreatedAt = DateTime.UtcNow
                };
                incident.Injuries.Add(injury);
            }
        }

        // Handle death records
        if (dto.Deaths != null)
        {
            // Remove all existing deaths
            _context.Deaths.RemoveRange(incident.Deaths);

            // Add new deaths
            foreach (var deathDto in dto.Deaths)
            {
                var death = new Death
                {
                    IncidentId = id,
                    Name = deathDto.Name,
                    Type = deathDto.Type,
                    Description = deathDto.Description,
                    CreatedAt = DateTime.UtcNow
                };
                incident.Deaths.Add(death);
            }
        }

        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "casualties_updated" });

        return NoContent();
    }

    // Incident Fire endpoints
    [HttpPut("{id}/fire")]
    public async Task<ActionResult> UpdateIncidentFire(int id, UpdateIncidentFireDto dto)
    {
        var incident = await _context.Incidents.Include(i => i.Fire).FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null) return NotFound();

        if (incident.Fire == null)
        {
            incident.Fire = new IncidentFire
            {
                IncidentId = id,
                CreatedAt = DateTime.UtcNow
            };
            _context.IncidentFires.Add(incident.Fire);
        }

        incident.Fire.BurnedArea = dto.BurnedArea;
        incident.Fire.BurnedItems = dto.BurnedItems;
        incident.Fire.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        return NoContent();
    }

    // Incident Damage endpoints
    [HttpPut("{id}/damage")]
    public async Task<ActionResult> UpdateIncidentDamage(int id, UpdateIncidentDamageDto dto)
    {
        var incident = await _context.Incidents.Include(i => i.Damage).FirstOrDefaultAsync(i => i.Id == id);
        if (incident == null) return NotFound();

        if (incident.Damage == null)
        {
            incident.Damage = new IncidentDamage
            {
                IncidentId = id,
                CreatedAt = DateTime.UtcNow
            };
            _context.IncidentDamages.Add(incident.Damage);
        }

        incident.Damage.OwnerName = dto.OwnerName;
        incident.Damage.TenantName = dto.TenantName;
        incident.Damage.DamageAmount = dto.DamageAmount;
        incident.Damage.SavedProperty = dto.SavedProperty;
        incident.Damage.IncidentCause = dto.IncidentCause;
        incident.Damage.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        await _realTimeService.SendIncidentUpdate(id, new { type = "involvement_updated" });

        return NoContent();
    }
}