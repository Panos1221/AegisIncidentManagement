using System.Text.Json;
using System.Text.Json.Serialization;
using IncidentManagement.Application.Configuration;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentManagement.Application.Services;

public class HospitalDataService : IHospitalDataService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<HospitalDataService> _logger;
    private readonly HospitalDataOptions _options;

    public HospitalDataService(
        IncidentManagementDbContext context,
        ILogger<HospitalDataService> logger,
        IOptions<HospitalDataOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> IsDataLoadedAsync()
    {
        try
        {
            // Check if EKAB agency has any stations (similar to other services)
            var ekabAgency = await _context.Agencies
                .FirstOrDefaultAsync(a => a.Name == "EKAB");
            
            if (ekabAgency == null)
                return false;
                
            return await _context.Stations
                .AnyAsync(s => s.AgencyId == ekabAgency.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if hospital data is loaded");
            return false;
        }
    }

    public async Task LoadHospitalDataAsync()
    {
        if (!_options.EnableDataLoading)
        {
            _logger.LogInformation("Hospital data loading is disabled in configuration");
            return;
        }

        if (await IsDataLoadedAsync())
        {
            _logger.LogInformation("Hospital data already exists in database, skipping load");
            return;
        }

        var filePath = _options.GeoJsonFilePath;
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            _logger.LogWarning("Hospital GeoJSON file not found at path: {FilePath}", filePath);
            return;
        }

        _logger.LogInformation("Loading hospital data from {FilePath}", filePath);

        var retryCount = 0;
        const int maxRetries = 3;

        while (retryCount < maxRetries)
        {
            try
            {
                var jsonContent = await File.ReadAllTextAsync(filePath);
                var geoJsonData = JsonSerializer.Deserialize<GeoJsonFeatureCollection>(jsonContent);

                if (geoJsonData?.Features == null || !geoJsonData.Features.Any())
                {
                    _logger.LogWarning("No hospital features found in GeoJSON file");
                    return;
                }

                // Get the EKAB agency
                var ekabAgency = await _context.Agencies
                    .FirstOrDefaultAsync(a => a.Name == "EKAB");

                if (ekabAgency == null)
                {
                    _logger.LogError("EKAB agency not found in database");
                    return;
                }

                var hospitals = new List<Hospital>();
                var stations = new List<Station>();

                foreach (var feature in geoJsonData.Features)
                {
                    if (feature.Geometry?.Type != "Point" || 
                        feature.Geometry.Coordinates == null || 
                        feature.Geometry.Coordinates.Length < 2)
                    {
                        _logger.LogWarning("Skipping invalid hospital feature with invalid geometry");
                        continue;
                    }

                    var properties = feature.Properties;
                    var name = properties?.GetValueOrDefault("name")?.ToString() ?? 
                              properties?.GetValueOrDefault("Name")?.ToString() ?? 
                              "Unknown Hospital";
                    
                    var address = properties?.GetValueOrDefault("address")?.ToString() ?? "";
                    var city = properties?.GetValueOrDefault("city")?.ToString() ?? "";
                    var region = properties?.GetValueOrDefault("region")?.ToString() ?? "";

                    var latitude = Convert.ToDouble(feature.Geometry.Coordinates[1]);
                    var longitude = Convert.ToDouble(feature.Geometry.Coordinates[0]);

                    // Create Hospital entity (detailed info)
                    var hospital = new Hospital
                    {
                        Name = name,
                        Address = address,
                        City = city,
                        Region = region,
                        Latitude = latitude,
                        Longitude = longitude,
                        AgencyCode = "EKAB",
                        CreatedAt = DateTime.UtcNow
                    };

                    hospitals.Add(hospital);

                    // Create Station entity for operational assignments (vehicles, personnel)
                    var station = new Station
                    {
                        Name = name,
                        AgencyId = ekabAgency.Id,
                        Latitude = latitude,
                        Longitude = longitude
                    };

                    stations.Add(station);
                }

                if (hospitals.Any())
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    try
                    {
                        await _context.Stations.AddRangeAsync(stations);
                        await _context.Hospitals.AddRangeAsync(hospitals);

                        var savedChanges = await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Successfully loaded {Count} hospitals (unified: {StationCount}, detailed: {HospitalCount})",
                             hospitals.Count, stations.Count, hospitals.Count);
                     }
                     catch (Exception ex)
                     {
                         await transaction.RollbackAsync();
                         _logger.LogError(ex, "Failed to save hospitals to database");
                         throw;
                     }
                 }
                 else
                {
                    _logger.LogWarning("No valid hospitals found to load");
                }

                return; // Success, exit method
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, "Attempt {RetryCount} failed to load hospital data", retryCount);
                
                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Failed to load hospital data after {MaxRetries} attempts", maxRetries);
                    throw;
                }
                
                await Task.Delay(1000 * retryCount); // Exponential backoff
            }
        }
    }
}