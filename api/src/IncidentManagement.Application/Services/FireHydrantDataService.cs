using IncidentManagement.Application.Configuration;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IncidentManagement.Application.Services;

public class FireHydrantDataService : IFireHydrantDataService
{
    private readonly IncidentManagementDbContext _context;
    private readonly FireHydrantDataOptions _options;
    private readonly ILogger<FireHydrantDataService> _logger;

    public FireHydrantDataService(
        IncidentManagementDbContext context,
        IOptions<FireHydrantDataOptions> options,
        ILogger<FireHydrantDataService> logger)
    {
        _context = context;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IEnumerable<FireHydrant>> LoadFireHydrantsFromGeoJsonAsync()
    {
        try
        {
            if (!File.Exists(_options.GeoJsonFilePath))
            {
                _logger.LogWarning("Fire hydrant GeoJSON file not found at: {FilePath}", _options.GeoJsonFilePath);
                return Enumerable.Empty<FireHydrant>();
            }

            _logger.LogInformation("Loading fire hydrants from: {FilePath}", Path.GetFullPath(_options.GeoJsonFilePath));
            var jsonContent = await File.ReadAllTextAsync(_options.GeoJsonFilePath);
            _logger.LogInformation("GeoJSON file size: {Size} characters", jsonContent.Length);
            
            var geoJsonData = GeoJsonService.DeserializeFeatureCollection<GeoJsonFeatureCollection>(jsonContent);
            _logger.LogInformation("Deserialized GeoJSON data. Features count: {Count}", geoJsonData?.Features?.Count ?? 0);

            if (geoJsonData?.Features == null)
            {
                _logger.LogWarning("No features found in GeoJSON file");
                return Enumerable.Empty<FireHydrant>();
            }

            var fireHydrants = new List<FireHydrant>();
            var now = DateTime.UtcNow;

            foreach (var feature in geoJsonData.Features)
            {
                if (feature.Geometry?.Type != "Point" || feature.Geometry.Coordinates?.Length != 2)
                    continue;

                var fireHydrant = new FireHydrant
                {
                    ExternalId = feature.Id ?? "",
                    Longitude = feature.Geometry.Coordinates[0],
                    Latitude = feature.Geometry.Coordinates[1],
                    Position = feature.Properties?.GetValueOrDefault("fire_hydrant:position")?.ToString(),
                    Type = feature.Properties?.GetValueOrDefault("fire_hydrant:type")?.ToString(),
                    AdditionalProperties = JsonSerializer.Serialize(feature.Properties ?? new Dictionary<string, object>()),
                    CreatedAt = now,
                    UpdatedAt = now,
                    IsActive = true
                };

                fireHydrants.Add(fireHydrant);
            }

            _logger.LogInformation("Loaded {Count} fire hydrants from GeoJSON file", fireHydrants.Count);
            return fireHydrants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading fire hydrants from GeoJSON file");
            throw;
        }
    }

    public async Task<bool> SeedFireHydrantsAsync()
    {
        try
        {
            if (!_options.EnableDataLoading)
            {
                _logger.LogInformation("Fire hydrant data loading is disabled");
                return false;
            }

            // Check if fire hydrants already exist
            var existingCount = await _context.FireHydrants.CountAsync();
            if (existingCount > 0)
            {
                _logger.LogInformation("Fire hydrants already exist in database ({Count} records)", existingCount);
                return false;
            }

            var fireHydrants = await LoadFireHydrantsFromGeoJsonAsync();
            if (!fireHydrants.Any())
            {
                _logger.LogWarning("No fire hydrants to seed");
                return false;
            }

            await _context.FireHydrants.AddRangeAsync(fireHydrants);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Successfully seeded {Count} fire hydrants", fireHydrants.Count());
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding fire hydrants");
            throw;
        }
    }

    public async Task<IEnumerable<FireHydrant>> GetFireHydrantsAsync()
    {
        return await _context.FireHydrants
            .Where(fh => fh.IsActive)
            .OrderBy(fh => fh.Id)
            .ToListAsync();
    }

    public async Task<FireHydrant?> GetFireHydrantByIdAsync(int id)
    {
        return await _context.FireHydrants
            .FirstOrDefaultAsync(fh => fh.Id == id && fh.IsActive);
    }
}