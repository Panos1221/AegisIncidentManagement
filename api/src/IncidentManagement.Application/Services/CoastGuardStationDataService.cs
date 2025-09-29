using System.Text.Json;
using IncidentManagement.Application.Configuration;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentManagement.Application.Services;

public class CoastGuardStationDataService : ICoastGuardStationDataService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<CoastGuardStationDataService> _logger;
    private readonly CoastGuardStationDataOptions _options;

    public CoastGuardStationDataService(
        IncidentManagementDbContext context,
        ILogger<CoastGuardStationDataService> logger,
        IOptions<CoastGuardStationDataOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task LoadCoastGuardStationDataAsync()
    {
        try
        {
            if (!_options.EnableDataLoading)
            {
                _logger.LogInformation("Coast Guard station data loading is disabled");
                return;
            }

            if (await IsDataAlreadyLoadedAsync())
            {
                _logger.LogInformation("Coast Guard station data already loaded");
                return;
            }

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _options.GeoJsonFilePath);

            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Coast Guard station GeoJSON file not found at: {FilePath}", filePath);
                return;
            }

            _logger.LogInformation("Loading Coast Guard station data from: {FilePath}", filePath);

            var jsonContent = await File.ReadAllTextAsync(filePath);
            var geoJsonData = JsonSerializer.Deserialize<CoastGuardStationGeoJsonDto>(jsonContent);

            if (geoJsonData?.Features == null)
            {
                _logger.LogWarning("Invalid GeoJSON format in Coast Guard station file");
                return;
            }

            // Get the Coast Guard agency
            var coastGuardAgency = await _context.Agencies
                .FirstOrDefaultAsync(a => a.Name == "Hellenic Coast Guard");

            if (coastGuardAgency == null)
            {
                _logger.LogError("Hellenic Coast Guard agency not found in database");
                return;
            }

            var coastGuardStations = new List<CoastGuardStation>();
            var stations = new List<Station>();

            foreach (var feature in geoJsonData.Features)
            {
                if (feature.Geometry?.Coordinates?.Count >= 2)
                {
                    var latitude = feature.Geometry.Coordinates[1];
                    var longitude = feature.Geometry.Coordinates[0];

                    // Create CoastGuardStation entity (detailed info)
                    var coastGuardStation = new CoastGuardStation
                    {
                        Name = feature.Properties.Name,
                        NameGr = feature.Properties.NameGr,
                        Address = feature.Properties.Address,
                        Area = feature.Properties.Area,
                        Type = feature.Properties.Type,
                        Telephone = feature.Properties.Telephone,
                        Email = feature.Properties.Email,
                        Longitude = longitude,
                        Latitude = latitude,
                        CreatedAt = DateTime.UtcNow
                    };

                    coastGuardStations.Add(coastGuardStation);

                    // Create Station entity for operational assignments (vehicles, personnel)
                    var station = new Station
                    {
                        Name = feature.Properties.Name,
                        AgencyId = coastGuardAgency.Id,
                        Latitude = latitude,
                        Longitude = longitude
                    };

                    stations.Add(station);
                }
            }

            if (coastGuardStations.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await _context.Stations.AddRangeAsync(stations);
                    await _context.CoastGuardStations.AddRangeAsync(coastGuardStations);

                    var savedChanges = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully loaded {Count} Coast Guard stations (unified: {StationCount}, detailed: {CoastGuardCount})",
                        coastGuardStations.Count, stations.Count, coastGuardStations.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to save Coast Guard stations to database");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No valid Coast Guard stations found in the GeoJSON file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Coast Guard station data");
            throw;
        }
    }

    public async Task<bool> IsDataAlreadyLoadedAsync()
    {
        return await _context.CoastGuardStations.AnyAsync();
    }
}