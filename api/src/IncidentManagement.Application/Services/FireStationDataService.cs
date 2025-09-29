using System.Text.Json;
using IncidentManagement.Application.Configuration;
using IncidentManagement.Application.DTOs;
using IncidentManagement.Domain.Entities;
using IncidentManagement.Domain.Enums;
using IncidentManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IncidentManagement.Application.Services;

public class FireStationDataService : IFireStationDataService
{
    private readonly IncidentManagementDbContext _context;
    private readonly ILogger<FireStationDataService> _logger;
    private readonly FireStationDataOptions _options;
    private readonly string _jsonFilePath;

    public FireStationDataService(
        IncidentManagementDbContext context,
        ILogger<FireStationDataService> logger,
        IOptions<FireStationDataOptions> options)
    {
        _context = context;
        _logger = logger;
        _options = options.Value;
        
        // Resolve the JSON file path from configuration
        _jsonFilePath = ResolveJsonFilePath(_options.JsonFilePath);
    }

    /// <summary>
    /// Resolves the JSON file path, trying multiple locations if the configured path doesn't exist
    /// </summary>
    private string ResolveJsonFilePath(string configuredPath)
    {
        if (!string.IsNullOrEmpty(configuredPath))
        {
            // Try the configured path first (relative to current directory)
            var configuredFullPath = Path.IsPathRooted(configuredPath) 
                ? configuredPath 
                : Path.Combine(Directory.GetCurrentDirectory(), configuredPath);
                
            if (File.Exists(configuredFullPath))
            {
                _logger.LogInformation("Using configured fire station data file: {FilePath}", configuredFullPath);
                return configuredFullPath;
            }
        }
        
        // Fallback to multiple possible locations
        var possiblePaths = new[]
        {
            // Development environment - relative to the API project
            Path.Combine(Directory.GetCurrentDirectory(), "..", "IncidentManagement.Infrastructure", "Data", "fire_depts_districts.json"),
            // Alternative development path
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "api", "src", "IncidentManagement.Infrastructure", "Data", "fire_depts_districts.json"),
            // Production/deployment path - same directory as executable
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "fire_depts_districts.json"),
            // Fallback - current directory
            Path.Combine(Directory.GetCurrentDirectory(), "fire_depts_districts.json")
        };

        var foundPath = possiblePaths.FirstOrDefault(File.Exists);
        if (foundPath != null)
        {
            _logger.LogInformation("Found fire station data file at fallback location: {FilePath}", foundPath);
            return foundPath;
        }

        _logger.LogWarning("Fire station data file not found. Configured path: {ConfiguredPath}, Tried paths: {Paths}", 
            configuredPath, string.Join(", ", possiblePaths));
        
        return possiblePaths[0]; // Return first path as fallback
    }

    public async Task<bool> IsDataAlreadyLoadedAsync()
    {
        try
        {
            // Get the Fire agency ID
            var fireAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Name == "Hellenic Fire Service");
            if (fireAgency == null)
            {
                _logger.LogWarning("Fire agency not found in database. Cannot load fire station data.");
                return false;
            }
            
            var stationCount = await _context.Stations.Where(s => s.AgencyId == fireAgency.Id).CountAsync();
            
            var isLoaded = stationCount > 0;
            
            _logger.LogDebug("Fire station data check: {StationCount} fire agency stations found. Data loaded: {IsLoaded}", 
                stationCount, isLoaded);
            
            return isLoaded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if fire station data is already loaded. Assuming data is not loaded.");
            return false;
        }
    }

    public async Task LoadFireStationDataAsync()
    {
        if (!_options.EnableDataLoading)
        {
            _logger.LogInformation("Fire station data loading is disabled in configuration");
            return;
        }

        var attempt = 0;
        var maxAttempts = Math.Max(1, _options.MaxRetryAttempts);

        while (attempt < maxAttempts)
        {
            attempt++;
            
            try
            {
                _logger.LogInformation("Fire station data loading attempt {Attempt} of {MaxAttempts}", attempt, maxAttempts);

                // Check if data is already loaded
                if (await IsDataAlreadyLoadedAsync())
                {
                    _logger.LogInformation("Fire station data already exists in database. Skipping load.");
                    return;
                }

                // Check if JSON file exists
                if (!File.Exists(_jsonFilePath))
                {
                    _logger.LogWarning("Fire station JSON file not found at path: {FilePath}. Continuing without fire station boundaries.", _jsonFilePath);
                    return;
                }

                _logger.LogInformation("Loading fire station data from: {FilePath}", _jsonFilePath);

                // Read and parse JSON file
                var jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    _logger.LogWarning("Fire station JSON file is empty or contains only whitespace");
                    return;
                }

                var featureCollection = JsonSerializer.Deserialize<FireStationFeatureCollection>(jsonContent);

                if (featureCollection?.Features == null || !featureCollection.Features.Any())
                {
                    _logger.LogWarning("No fire station features found in JSON file");
                    return;
                }

                _logger.LogInformation("Found {FeatureCount} features in fire station data file", featureCollection.Features.Count);

                // Get the Fire agency ID
                var fireAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Name == "Hellenic Fire Service");
                if (fireAgency == null)
                {
                    _logger.LogError("Fire agency not found in database. Cannot load fire station data.");
                    return;
                }

                var fireStations = new List<FireStation>();
                var stations = new List<Station>();
                var stationBoundaries = new List<StationBoundary>();
                var processedCount = 0;
                var skippedCount = 0;

                foreach (var feature in featureCollection.Features)
                {
                    if (feature.Geometry?.Type != "Polygon")
                    {
                        _logger.LogDebug("Skipping feature with non-polygon geometry: {GeometryType}", feature.Geometry?.Type);
                        skippedCount++;
                        continue;
                    }

                    try
                    {
                        // Parse area - it might be a string or number
                        double area = 0;
                        if (feature.Properties.Area.ValueKind == JsonValueKind.Number)
                        {
                            area = feature.Properties.Area.GetDouble();
                        }
                        else if (feature.Properties.Area.ValueKind == JsonValueKind.String)
                        {
                            double.TryParse(feature.Properties.Area.GetString(), out area);
                        }

                        var stationName = feature.Properties.StationName ?? $"Station_{processedCount + 1}";

                        // Create FireStation entity for geographic boundaries
                        var fireStation = new FireStation
                        {
                            Name = stationName,
                            Region = feature.Properties.Region ?? "",
                            Area = area,
                            GeometryJson = JsonSerializer.Serialize(feature.Geometry),
                            CreatedAt = DateTime.UtcNow
                        };

                        fireStations.Add(fireStation);

                        // Create Station entity for operational assignments (vehicles, personnel)
                        // Calculate approximate center coordinates for the station location
                        var centerCoordinates = CalculatePolygonCenter(ParseCoordinates(feature.Geometry.Coordinates));
                        
                        var station = new Station
                        {
                            Name = stationName,
                            AgencyId = fireAgency.Id,
                            Latitude = centerCoordinates.latitude,
                            Longitude = centerCoordinates.longitude
                        };

                        stations.Add(station);

                        // Parse coordinates - handle nested array structure
                        var coordinateRings = ParseCoordinates(feature.Geometry.Coordinates);
                        
                        foreach (var ring in coordinateRings)
                        {
                            // Transform coordinates from projected system to WGS84 (lat/lng)
                            var transformedCoordinates = TransformCoordinates(ring);
                            
                            if (transformedCoordinates.Any())
                            {
                                var boundary = new StationBoundary
                                {
                                    FireStation = fireStation,
                                    CoordinatesJson = JsonSerializer.Serialize(transformedCoordinates)
                                };

                                stationBoundaries.Add(boundary);
                            }
                        }

                        processedCount++;
                        
                        if (processedCount % 10 == 0)
                        {
                            _logger.LogDebug("Processed {ProcessedCount} fire station features", processedCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process fire station feature: {StationName}", 
                            feature.Properties.StationName ?? "Unknown");
                        skippedCount++;
                        continue;
                    }
                }

                if (!fireStations.Any())
                {
                    _logger.LogWarning("No valid fire station features could be processed from the JSON file");
                    return;
                }

                // Save to database in a transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    await _context.Stations.AddRangeAsync(stations);
                    await _context.FireStations.AddRangeAsync(fireStations);
                    await _context.StationBoundaries.AddRangeAsync(stationBoundaries);
                    
                    var savedChanges = await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation("Successfully loaded fire station data: {ProcessedCount} processed, {SkippedCount} skipped. " +
                        "Created {StationCount} stations, {FireStationCount} fire stations, {BoundaryCount} boundaries. " +
                        "Database changes: {SavedChanges}", 
                        processedCount, skippedCount, stations.Count, fireStations.Count, stationBoundaries.Count, savedChanges);
                    
                    return; // Success - exit retry loop
                }
                catch (Exception dbEx)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Failed to save fire station data to database", dbEx);
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse fire station JSON data on attempt {Attempt}", attempt);
                
                if (attempt >= maxAttempts)
                {
                    throw new InvalidOperationException("Invalid fire station data format after all retry attempts", ex);
                }
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Fire station JSON file not found: {FilePath}", _jsonFilePath);
                throw; // Don't retry for file not found
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied reading fire station JSON file: {FilePath}", _jsonFilePath);
                throw; // Don't retry for access denied
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading fire station data on attempt {Attempt} of {MaxAttempts}", attempt, maxAttempts);
                
                if (attempt >= maxAttempts)
                {
                    throw new InvalidOperationException($"Failed to load fire station data after {maxAttempts} attempts", ex);
                }
                
                // Wait before retry
                if (attempt < maxAttempts && _options.RetryDelayMs > 0)
                {
                    _logger.LogInformation("Waiting {DelayMs}ms before retry attempt {NextAttempt}", _options.RetryDelayMs, attempt + 1);
                    await Task.Delay(_options.RetryDelayMs);
                }
            }
        }
    }

    /// <summary>
    /// Parses coordinates from JsonElement handling various nested structures
    /// </summary>
    private List<List<List<double>>> ParseCoordinates(JsonElement coordinatesElement)
    {
        var result = new List<List<List<double>>>();
        
        try
        {
            if (coordinatesElement.ValueKind == JsonValueKind.Array)
            {
                // Handle polygon coordinates - should be array of rings
                foreach (var ring in coordinatesElement.EnumerateArray())
                {
                    var coordinateRing = new List<List<double>>();
                    
                    if (ring.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var coordinate in ring.EnumerateArray())
                        {
                            var coord = new List<double>();
                            
                            if (coordinate.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var value in coordinate.EnumerateArray())
                                {
                                    if (value.ValueKind == JsonValueKind.Number)
                                    {
                                        coord.Add(value.GetDouble());
                                    }
                                    else if (value.ValueKind == JsonValueKind.String)
                                    {
                                        if (double.TryParse(value.GetString(), out double parsedValue))
                                        {
                                            coord.Add(parsedValue);
                                        }
                                    }
                                }
                            }
                            
                            if (coord.Count >= 2) // Valid coordinate pair
                            {
                                coordinateRing.Add(coord);
                            }
                        }
                    }
                    
                    if (coordinateRing.Count > 0)
                    {
                        result.Add(coordinateRing);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse coordinates");
        }
        
        return result;
    }

    /// <summary>
    /// Transforms coordinates from Greek Grid (EPSG:2100) to WGS84 (EPSG:4326)
    /// This is a simplified transformation - in production, use a proper coordinate transformation library
    /// </summary>
    private List<List<double>> TransformCoordinates(List<List<double>> coordinates)
    {
        var transformedCoordinates = new List<List<double>>();

        foreach (var coordinate in coordinates)
        {
            if (coordinate.Count >= 2)
            {
                var x = coordinate[0]; // Easting
                var y = coordinate[1]; // Northing

                // Improved approximation for Greek Grid (EPSG:2100) to WGS84 transformation
                // Greek Grid uses Transverse Mercator projection with specific parameters
                                
                // Constants for Greek Grid transformation (approximate)
                const double falseEasting = 500000.0;
                const double falseNorthing = 0.0;
                const double centralMeridian = 24.0; // 24°E
                const double scaleFactor = 0.9996;
                const double originLatitude = 0.0;
                
                // Remove false easting/northing
                var adjustedX = x - falseEasting;
                var adjustedY = y - falseNorthing;
                
                // Simple inverse transformation (approximation)
                // In reality, this requires complex ellipsoidal calculations
                var longitude = centralMeridian + (adjustedX / (111320.0 * Math.Cos(Math.PI * 38.0 / 180.0)));
                var latitude = adjustedY / 111320.0;
                
                // Ensure coordinates are within reasonable bounds for Greece
                longitude = Math.Max(19.0, Math.Min(29.0, longitude));
                latitude = Math.Max(34.0, Math.Min(42.0, latitude));

                transformedCoordinates.Add(new List<double> { longitude, latitude });
            }
        }

        return transformedCoordinates;
    }

    /// <summary>
    /// Calculates the approximate center (centroid) of a polygon
    /// </summary>
    private (double latitude, double longitude) CalculatePolygonCenter(List<List<List<double>>> coordinateRings)
    {
        if (!coordinateRings.Any() || !coordinateRings[0].Any())
        {
            return (38.0, 24.0); // Default center of Greece
        }

        // Use the first (outer) ring for centroid calculation
        var outerRing = coordinateRings[0];
        
        // Transform coordinates first
        var transformedRing = TransformCoordinates(outerRing);
        
        if (!transformedRing.Any())
        {
            return (38.0, 24.0); // Default center of Greece
        }

        // Calculate simple centroid (average of all points)
        double sumLat = 0, sumLon = 0;
        int count = 0;

        foreach (var coordinate in transformedRing)
        {
            if (coordinate.Count >= 2)
            {
                sumLon += coordinate[0]; // longitude
                sumLat += coordinate[1]; // latitude
                count++;
            }
        }

        if (count == 0)
        {
            return (38.0, 24.0); // Default center of Greece
        }

        return (sumLat / count, sumLon / count);
    }

    /// <summary>
    /// Loads simple fire station locations from the JSON file into the database
    /// </summary>
    public async Task LoadFireStationLocationsAsync()
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                _logger.LogInformation("Starting fire station locations loading process (attempt {Attempt}/{MaxRetries})", retryCount + 1, maxRetries);

                // Only skip loading if data loading is disabled and data already exists
                if (!_options.EnableDataLoading && await IsStationDataAlreadyLoadedAsync())
                {
                    _logger.LogInformation("Fire station data loading is disabled and data already exists. Skipping load.");
                    return;
                }

                // Read and parse the GeoJSON file
                var jsonContent = await File.ReadAllTextAsync(_jsonFilePath);
                var geoJsonData = GeoJsonService.DeserializeFeatureCollection<FireStationGeoJsonFeatureCollection>(jsonContent);

                if (geoJsonData?.Features == null || !geoJsonData.Features.Any())
                {
                    _logger.LogWarning("No fire station locations found in GeoJSON file: {FilePath}", _jsonFilePath);
                    return;
                }

                _logger.LogInformation("Found {Count} fire station locations in GeoJSON file", geoJsonData.Features.Count);

                // Get the Fire agency ID
                var fireAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Name == "Hellenic Fire Service");
                if (fireAgency == null)
                {
                    _logger.LogError("Fire agency not found in database. Cannot load fire station locations.");
                    return;
                }

                // Get existing fire station names to avoid duplicates
                var existingFireStationNames = await _context.FireStations
                    .Select(s => s.Name.ToLower())
                    .ToListAsync();

                // Process and save data in a transaction
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var fireStations = new List<FireStation>();
                    var stations = new List<Station>();
                    var processedCount = 0;
                    var skippedCount = 0;

                    foreach (var feature in geoJsonData.Features)
                    {
                        if (feature.Geometry?.Coordinates?.Length < 2)
                        {
                            _logger.LogWarning("Skipping feature with invalid coordinates at index {Index}", processedCount);
                            skippedCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(feature.Properties.Name))
                        {
                            _logger.LogWarning("Skipping feature with empty name at index {Index}", processedCount);
                            skippedCount++;
                            continue;
                        }

                        var stationName = feature.Properties.Name.Trim();

                        // Check for duplicate names (case-insensitive)
                        if (existingFireStationNames.Contains(stationName.ToLower()))
                        {
                            _logger.LogDebug("Skipping duplicate fire station: {StationName}", stationName);
                            skippedCount++;
                            continue;
                        }

                        var latitude = feature.Geometry.Coordinates[1];
                        var longitude = feature.Geometry.Coordinates[0];

                        // Create FireStation entity (consolidated info for both location and boundaries)
                        var fireStation = new FireStation
                        {
                            Name = stationName,
                            Address = feature.Properties.Address,
                            City = feature.Properties.City,
                            Region = feature.Properties.Region,
                            Area = 0, // No area data in point GeoJSON
                            Latitude = latitude,
                            Longitude = longitude,
                            GeometryJson = "", // No boundary polygon data for individual stations
                            CreatedAt = DateTime.UtcNow
                        };

                        fireStations.Add(fireStation);

                        // Create Station entity for operational assignments (vehicles, personnel)
                        var station = new Station
                        {
                            Name = stationName,
                            Latitude = latitude,
                            Longitude = longitude,
                            AgencyId = fireAgency.Id
                        };

                        stations.Add(station);

                        // Add to existing names to prevent duplicates within the same batch
                        existingFireStationNames.Add(stationName.ToLower());
                        processedCount++;
                    }

                    if (fireStations.Any())
                    {
                        await _context.Stations.AddRangeAsync(stations);
                        await _context.FireStations.AddRangeAsync(fireStations);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Successfully loaded {Count} fire stations into database (stations: {StationCount}, fire stations: {FireStationCount}, skipped {SkippedCount} duplicates/invalid entries)",
                            processedCount, stations.Count, fireStations.Count, skippedCount);
                    }
                    else
                    {
                        _logger.LogWarning("No valid fire stations to save (skipped {SkippedCount} duplicates/invalid entries)", skippedCount);
                        await transaction.RollbackAsync();
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error occurred while saving fire station locations to database");
                    throw;
                }

                return; // Success, exit retry loop
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse fire station locations JSON file: {FilePath}", _jsonFilePath);
                throw; // Don't retry JSON parsing errors
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Fire station locations JSON file not found: {FilePath}", _jsonFilePath);
                throw; // Don't retry file not found errors
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Access denied to fire station locations JSON file: {FilePath}", _jsonFilePath);
                throw; // Don't retry access denied errors
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogError(ex, "Error loading fire station locations (attempt {Attempt}/{MaxRetries}): {Message}", retryCount, maxRetries, ex.Message);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError("Failed to load fire station locations after {MaxRetries} attempts", maxRetries);
                    throw;
                }

                // Wait before retrying (exponential backoff)
                var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                _logger.LogInformation("Retrying in {Delay} seconds...", delay.TotalSeconds);
                await Task.Delay(delay);
            }
        }
    }

    /// <summary>
    /// Checks if station data has already been loaded into the database
    /// </summary>
    private async Task<bool> IsStationDataAlreadyLoadedAsync()
    {
        // Get the Fire agency ID
        var fireAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Name == "Hellenic Fire Service");
        if (fireAgency == null)
        {
            _logger.LogWarning("Hellenic Fire Service not found in database. Cannot check fire station count.");
            return false;
        }
        
        var fireStationCount = await _context.Stations.Where(s => s.AgencyId == fireAgency.Id).CountAsync();
        var detailedFireStationCount = await _context.FireStations.CountAsync();
        return fireStationCount > 0 || detailedFireStationCount > 0;
    }
}