using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Application.Services;

public class VehicleService : IVehicleService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<VehicleService> _logger;

    public VehicleService(IncidentManagementDbContext context, ILogger<VehicleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<VehicleDto>> GetVehiclesByStationAsync(int? stationId = null, int? userAgencyId = null)
    {
        try
        {
            var query = _context.Vehicles
                .Include(v => v.Station)
                .ThenInclude(s => s.Agency)
                .AsQueryable();

            // Filter by user's agency first
            if (userAgencyId.HasValue)
            {
                query = query.Where(v => v.Station.AgencyId == userAgencyId.Value);
            }

            // Then filter by station if specified
            if (stationId.HasValue)
            {
                query = query.Where(v => v.StationId == stationId.Value);
            }

            var vehicles = await query.ToListAsync();
            return vehicles.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles by station {StationId} for agency {AgencyId}", stationId, userAgencyId);
            throw;
        }
    }

    public async Task<VehicleDto> UpdateVehicleAsync(int id, UpdateVehicleDto dto)
    {
        try
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {id} not found");
            }

            // Validate station exists if station assignment is being changed
            if (dto.StationId.HasValue)
            {
                var stationExists = await _context.Stations.AnyAsync(s => s.Id == dto.StationId.Value);
                if (!stationExists)
                {
                    throw new ArgumentException($"Station with ID {dto.StationId.Value} not found");
                }
                vehicle.StationId = dto.StationId.Value;
            }

            // Update basic vehicle information
            if (!string.IsNullOrWhiteSpace(dto.Callsign))
            {
                // Validate callsign uniqueness
                var existingVehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.Callsign == dto.Callsign && v.Id != id);
                if (existingVehicle != null)
                {
                    throw new ArgumentException($"Vehicle with callsign '{dto.Callsign}' already exists");
                }
                vehicle.Callsign = dto.Callsign;
            }

            if (!string.IsNullOrWhiteSpace(dto.Type))
                vehicle.Type = dto.Type;

            if (!string.IsNullOrWhiteSpace(dto.PlateNumber))
                vehicle.PlateNumber = dto.PlateNumber;

            if (dto.WaterCapacityLiters.HasValue)
            {
                if (dto.WaterCapacityLiters.Value < 0)
                {
                    throw new ArgumentException("Water capacity cannot be negative");
                }
                vehicle.WaterCapacityLiters = dto.WaterCapacityLiters.Value;
            }

            // Update status and telemetry
            if (dto.Status.HasValue)
                vehicle.Status = dto.Status.Value;

            if (dto.WaterLevelLiters.HasValue)
            {
                if (dto.WaterLevelLiters.Value < 0)
                {
                    throw new ArgumentException("Water level cannot be negative");
                }
                vehicle.WaterLevelLiters = dto.WaterLevelLiters.Value;
            }

            if (dto.FoamLevelLiters.HasValue)
            {
                if (dto.FoamLevelLiters.Value < 0)
                {
                    throw new ArgumentException("Foam level cannot be negative");
                }
                vehicle.FoamLevelLiters = dto.FoamLevelLiters.Value;
            }

            if (dto.FuelLevelPercent.HasValue)
            {
                if (dto.FuelLevelPercent.Value < 0 || dto.FuelLevelPercent.Value > 100)
                {
                    throw new ArgumentException("Fuel level must be between 0 and 100 percent");
                }
                vehicle.FuelLevelPercent = dto.FuelLevelPercent.Value;
            }

            if (dto.BatteryVoltage.HasValue)
                vehicle.BatteryVoltage = dto.BatteryVoltage.Value;

            if (dto.PumpPressureKPa.HasValue)
                vehicle.PumpPressureKPa = dto.PumpPressureKPa.Value;

            if (dto.Latitude.HasValue)
            {
                if (dto.Latitude.Value < -90 || dto.Latitude.Value > 90)
                {
                    throw new ArgumentException("Latitude must be between -90 and 90 degrees");
                }
                vehicle.Latitude = dto.Latitude.Value;
            }

            if (dto.Longitude.HasValue)
            {
                if (dto.Longitude.Value < -180 || dto.Longitude.Value > 180)
                {
                    throw new ArgumentException("Longitude must be between -180 and 180 degrees");
                }
                vehicle.Longitude = dto.Longitude.Value;
            }

            // Update timestamp
            vehicle.LastTelemetryAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehicle {VehicleId} updated successfully", id);
            return MapToDto(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            throw;
        }
    }

    public async Task<Dictionary<int, List<VehicleDto>>> GetVehiclesGroupedByStationAsync()
    {
        try
        {
            var vehicles = await _context.Vehicles
                .Include(v => v.Station)
                .ToListAsync();

            var grouped = vehicles
                .GroupBy(v => v.StationId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(MapToDto).ToList()
                );

            return grouped;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles grouped by station");
            throw;
        }
    }

    public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
    {
        try
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Station)
                .FirstOrDefaultAsync(v => v.Id == id);

            return vehicle != null ? MapToDto(vehicle) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
            throw;
        }
    }

    public async Task<List<VehicleDto>> GetVehiclesAsync(VehicleStatus? status = null, int? userAgencyId = null)
    {
        try
        {
            var query = _context.Vehicles
                .Include(v => v.Station)
                .ThenInclude(s => s.Agency)
                .AsQueryable();

            // Filter by user's agency first
            if (userAgencyId.HasValue)
            {
                query = query.Where(v => v.Station.AgencyId == userAgencyId.Value);
            }

            // Then filter by status if specified
            if (status.HasValue)
            {
                query = query.Where(v => v.Status == status.Value);
            }

            var vehicles = await query.ToListAsync();
            return vehicles.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles with status {Status} for agency {AgencyId}", status, userAgencyId);
            throw;
        }
    }

    public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto dto)
    {
        try
        {
            // Validate station exists
            var stationExists = await _context.Stations.AnyAsync(s => s.Id == dto.StationId);
            if (!stationExists)
            {
                throw new ArgumentException($"Station with ID {dto.StationId} not found");
            }

            // Validate callsign uniqueness
            var existingVehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Callsign == dto.Callsign);
            if (existingVehicle != null)
            {
                throw new ArgumentException($"Vehicle with callsign '{dto.Callsign}' already exists");
            }

            var vehicle = new Vehicle
            {
                StationId = dto.StationId,
                Callsign = dto.Callsign,
                Type = dto.Type,
                PlateNumber = dto.PlateNumber,
                WaterCapacityLiters = dto.WaterCapacityLiters,
                Status = VehicleStatus.Available
            };

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehicle {Callsign} created successfully with ID {VehicleId}", 
                dto.Callsign, vehicle.Id);

            return MapToDto(vehicle);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle {Callsign}", dto.Callsign);
            throw;
        }
    }

    private static VehicleDto MapToDto(Vehicle vehicle)
    {
        return new VehicleDto
        {
            Id = vehicle.Id,
            StationId = vehicle.StationId,
            Callsign = vehicle.Callsign,
            Type = vehicle.Type,
            Status = vehicle.Status,
            PlateNumber = vehicle.PlateNumber,
            WaterLevelLiters = vehicle.WaterLevelLiters,
            WaterCapacityLiters = vehicle.WaterCapacityLiters,
            FoamLevelLiters = vehicle.FoamLevelLiters,
            FuelLevelPercent = vehicle.FuelLevelPercent,
            BatteryVoltage = vehicle.BatteryVoltage,
            PumpPressureKPa = vehicle.PumpPressureKPa,
            Latitude = vehicle.Latitude,
            Longitude = vehicle.Longitude,
            LastTelemetryAt = vehicle.LastTelemetryAt,
            Station = vehicle.Station != null ? new StationDto
            {
                Id = vehicle.Station.Id,
                Name = vehicle.Station.Name,
                AgencyId = vehicle.Station.AgencyId,
                AgencyName = vehicle.Station.Agency?.Name ?? "",
                Latitude = vehicle.Station.Latitude,
                Longitude = vehicle.Station.Longitude
            } : null
        };
    }
}