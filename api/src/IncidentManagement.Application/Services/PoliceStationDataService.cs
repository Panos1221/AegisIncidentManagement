using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using IncidentManagement.Application.Configuration;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;

namespace IncidentManagement.Application.Services;

public class PoliceStationDataService : IPoliceStationDataService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<PoliceStationDataService> _logger;
    private readonly PoliceStationDataOptions _options;
    private readonly string _dataFilePath;

    public PoliceStationDataService(
        IncidentManagementDbContext context,
        ILogger<PoliceStationDataService> logger,
        IOptions<PoliceStationDataOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
        _dataFilePath = ResolveGeoJsonFilePath(_options.GeoJsonFilePath);
    }

    private string ResolveGeoJsonFilePath(string configuredPath)
    {
        if (string.IsNullOrEmpty(configuredPath))
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "policeStations-locations.geojson");
        }

        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                _logger.LogError("Police stations GeoJSON file not found at: {FilePath}", _dataFilePath);
                return;
            }

            var jsonContent = await File.ReadAllTextAsync(_dataFilePath);
            var geoJsonData = JsonSerializer.Deserialize<PoliceStationGeoJsonDto>(jsonContent);

            if (geoJsonData?.Features == null || !geoJsonData.Features.Any())
            {
                _logger.LogWarning("No police station features found in GeoJSON file");
                return;
            }

            // Get the Police agency
            var policeAgency = await _context.Agencies
                .FirstOrDefaultAsync(a => a.Name == "Hellenic Police");

            if (policeAgency == null)
            {
                _logger.LogError("Hellenic Police agency not found in database");
                return;
            }

            var policeStations = new List<PoliceStation>();
            var stations = new List<Station>();

            foreach (var feature in geoJsonData.Features)
            {
                if (feature.Geometry?.Coordinates?.Count > 0 && feature.Geometry.Coordinates[0]?.Count >= 2)
                {
                    // Police stations use MultiPoint geometry, so we take the first point
                    var coordinates = feature.Geometry.Coordinates[0];
                    var longitude = coordinates[0];
                    var latitude = coordinates[1];

                    // Create PoliceStation entity (detailed info)
                    var policeStation = new PoliceStation
                    {
                        Gid = feature.Properties.Gid,
                        OriginalId = feature.Properties.Id,
                        Name = feature.Properties.Name,
                        Address = feature.Properties.Address,
                        Sinoikia = feature.Properties.Sinoikia,
                        Diam = feature.Properties.Diam,
                        Longitude = longitude,
                        Latitude = latitude,
                        CreatedAt = DateTime.UtcNow
                    };

                    policeStations.Add(policeStation);

                    // Create Station entity for operational assignments (vehicles, personnel)
                    var station = new Station
                    {
                        Name = feature.Properties.Name,
                        AgencyId = policeAgency.Id,
                        Latitude = latitude,
                        Longitude = longitude
                    };

                    stations.Add(station);
                }
            }

            if (policeStations.Any())
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    await _context.Stations.AddRangeAsync(stations);
                    await _context.PoliceStations.AddRangeAsync(policeStations);

                    var savedChanges = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully loaded {Count} Police stations (unified: {StationCount}, detailed: {PoliceCount})",
                        policeStations.Count, stations.Count, policeStations.Count);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Failed to save Police stations to database");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No valid Police stations found in the GeoJSON file");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading Police station data");
            throw;
        }
    }

    public async Task<bool> IsDataAlreadyLoadedAsync()
    {
        return await _context.PoliceStations.AnyAsync();
    }
}