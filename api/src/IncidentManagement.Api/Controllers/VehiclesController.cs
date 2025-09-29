using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IncidentManagement.Infrastructure.Data;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Application.Services;
using IncidentManagement.Api.Services;

namespace IncidentManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiclesController : BaseController
{
    private readonly IncidentManagementDbContext _context;
    private readonly IVehicleService _vehicleService;
    private readonly IRealTimeNotificationService _realTimeService;

    public VehiclesController(IncidentManagementDbContext context, IVehicleService vehicleService, IRealTimeNotificationService realTimeService)
    {
        _context = context;
        _vehicleService = vehicleService;
        _realTimeService = realTimeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles(
        [FromQuery] int? stationId,
        [FromQuery] VehicleStatus? status)
    {
        try
        {
            var userAgencyId = GetCurrentUserAgencyId();
            if (!userAgencyId.HasValue)
            {
                return Unauthorized("User agency information not found");
            }

            List<VehicleDto> vehicles;

            if (stationId.HasValue)
            {
                // Validate user can access this station
                var stationExists = await _context.Stations
                    .AnyAsync(s => s.Id == stationId.Value && s.AgencyId == userAgencyId.Value);
                
                if (!stationExists)
                {
                    return Forbid("You do not have permission to access this station");
                }

                // Use service for station filtering with agency filtering
                vehicles = await _vehicleService.GetVehiclesByStationAsync(stationId.Value, userAgencyId.Value);
                
                // Apply status filter if provided
                if (status.HasValue)
                {
                    vehicles = vehicles.Where(v => v.Status == status.Value).ToList();
                }
            }
            else
            {
                // For regular members (non-dispatchers), filter by their assigned station
                if (!IsDispatcher())
                {
                    var userStationId = GetCurrentUserStationId();
                    if (!userStationId.HasValue)
                    {
                        return Unauthorized("User station information not found");
                    }
                    
                    // Use service for station filtering with agency filtering
                    vehicles = await _vehicleService.GetVehiclesByStationAsync(userStationId.Value, userAgencyId.Value);
                    
                    // Apply status filter if provided
                    if (status.HasValue)
                    {
                        vehicles = vehicles.Where(v => v.Status == status.Value).ToList();
                    }
                }
                else
                {
                    // Dispatchers can see all vehicles in their agency
                    vehicles = await _vehicleService.GetVehiclesAsync(status, userAgencyId.Value);
                }
            }

            return Ok(vehicles);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving vehicles", error = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetVehicle(int id)
    {
        try
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
            if (vehicle == null)
                return NotFound(new { message = $"Vehicle with ID {id} not found" });

            return Ok(vehicle);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the vehicle", error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> CreateVehicle(CreateVehicleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vehicle = await _vehicleService.CreateVehicleAsync(dto);

            // Get station info for broadcasting
            var station = await _context.Stations.FindAsync(vehicle.StationId);
            if (station != null)
            {
                await _realTimeService.BroadcastVehicleCreated(vehicle, vehicle.StationId, station.AgencyId);
            }

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, vehicle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the vehicle", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<VehicleDto>> UpdateVehicleDetails(int id, UpdateVehicleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, dto);

            // Get station info for broadcasting
            var station = await _context.Stations.FindAsync(updatedVehicle.StationId);
            if (station != null)
            {
                await _realTimeService.BroadcastVehicleUpdated(updatedVehicle.Id, updatedVehicle, updatedVehicle.StationId, station.AgencyId);
            }

            return Ok(updatedVehicle);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the vehicle", error = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateVehicleTelemetry(int id, UpdateVehicleDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // For PATCH, we only update telemetry fields, not basic vehicle info
            var telemetryDto = new UpdateVehicleDto
            {
                Status = dto.Status,
                WaterLevelLiters = dto.WaterLevelLiters,
                FoamLevelLiters = dto.FoamLevelLiters,
                FuelLevelPercent = dto.FuelLevelPercent,
                BatteryVoltage = dto.BatteryVoltage,
                PumpPressureKPa = dto.PumpPressureKPa,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            };

            var updatedVehicle = await _vehicleService.UpdateVehicleAsync(id, telemetryDto);

            // Get station info for broadcasting telemetry updates
            var station = await _context.Stations.FindAsync(updatedVehicle.StationId);
            if (station != null)
            {
                await _realTimeService.BroadcastVehicleUpdated(updatedVehicle.Id, updatedVehicle, updatedVehicle.StationId, station.AgencyId);
            }

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating vehicle telemetry", error = ex.Message });
        }
    }

    [HttpPost("{id}/telemetry")]
    public async Task<IActionResult> AddTelemetry(int id, UpdateVehicleDto dto)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
            return NotFound();

        // Update vehicle snapshot
        if (dto.WaterLevelLiters.HasValue)
            vehicle.WaterLevelLiters = dto.WaterLevelLiters.Value;
        if (dto.FuelLevelPercent.HasValue)
            vehicle.FuelLevelPercent = dto.FuelLevelPercent.Value;
        if (dto.BatteryVoltage.HasValue)
            vehicle.BatteryVoltage = dto.BatteryVoltage.Value;
        if (dto.PumpPressureKPa.HasValue)
            vehicle.PumpPressureKPa = dto.PumpPressureKPa.Value;
        if (dto.Latitude.HasValue)
            vehicle.Latitude = dto.Latitude.Value;
        if (dto.Longitude.HasValue)
            vehicle.Longitude = dto.Longitude.Value;

        vehicle.LastTelemetryAt = DateTime.UtcNow;

        // Store historical record
        var telemetry = new VehicleTelemetry
        {
            VehicleId = id,
            RecordedAt = DateTime.UtcNow,
            WaterLevelLiters = dto.WaterLevelLiters,
            FuelLevelPercent = dto.FuelLevelPercent,
            BatteryVoltage = dto.BatteryVoltage,
            PumpPressureKPa = dto.PumpPressureKPa,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude
        };

        _context.VehicleTelemetry.Add(telemetry);
        await _context.SaveChangesAsync();

        return Ok();
    }
}