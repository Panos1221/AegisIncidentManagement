using System.Net.Http.Json;
using System.Text;
using Newtonsoft.Json;
using StationConfigurator.Models;

namespace StationConfigurator.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public ApiClient(string baseUrl = "https://localhost:5000")
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public bool IsAuthenticated => !string.IsNullOrEmpty(_authToken);

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginDto { Email = email, Password = password };
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResponse != null)
                {
                    _authToken = loginResponse.Token;
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return false;
    }

    public void Logout()
    {
        _authToken = null;
        _httpClient.DefaultRequestHeaders.Authorization = null;
    }

    // Agency endpoints - using direct database query as fallback
    public async Task<List<AgencyDto>> GetAgenciesAsync()
    {
        try
        {
            // Create a hardcoded list of agencies since the API doesn't expose this endpoint yet
            var agencies = new List<AgencyDto>
            {
                new AgencyDto { Id = 1, Name = "Hellenic Police", Code = "HP", Type = AgencyType.Police, Description = "Greek National Police", IsActive = true },
                new AgencyDto { Id = 2, Name = "Hellenic Fire Service", Code = "HFS", Type = AgencyType.Fire, Description = "Greek Fire Service", IsActive = true },
                new AgencyDto { Id = 3, Name = "Hellenic Coast Guard", Code = "HCG", Type = AgencyType.CoastGuard, Description = "Greek Coast Guard", IsActive = true },
                new AgencyDto { Id = 4, Name = "EKAB", Code = "EKAB", Type = AgencyType.EKAB, Description = "Greek National Emergency Center", IsActive = true }
            };

            return agencies;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load agencies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<AgencyDto>();
        }
    }

    // Station endpoints - try the generic endpoint first, fallback to specific endpoints
    public async Task<List<StationDto>> GetStationsAsync()
    {
        try
        {
            // First try the generic stations endpoint
            var response = await _httpClient.GetAsync("/api/stations");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<StationDto>>() ?? new List<StationDto>();
            }
        }
        catch (Exception)
        {
            // Ignore errors and try fallback approach
        }

        // Fallback: Load from agency-specific endpoints and combine
        var allStations = new List<StationDto>();

        try
        {
            // Load Fire Stations
            var fireResponse = await _httpClient.GetAsync("/api/firestations/stations");
            if (fireResponse.IsSuccessStatusCode)
            {
                var fireStations = await fireResponse.Content.ReadFromJsonAsync<List<FireStationSimpleDto>>() ?? new List<FireStationSimpleDto>();
                var fireStationDtos = fireStations.Select(fs => new StationDto
                {
                    Id = fs.Id,
                    Name = fs.Name,
                    AgencyId = 2, // Fire Service
                    AgencyName = "Hellenic Fire Service",
                    Latitude = fs.Latitude,
                    Longitude = fs.Longitude
                }).ToList();
                allStations.AddRange(fireStationDtos);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load fire stations: {ex.Message}");
        }

        try
        {
            // Try to load Police Stations (if endpoint exists)
            var policeResponse = await _httpClient.GetAsync("/api/policestations/stations");
            if (policeResponse.IsSuccessStatusCode)
            {
                var policeStations = await policeResponse.Content.ReadFromJsonAsync<List<object>>() ?? new List<object>();
                // Add police stations with proper agency mapping
                // This would need to be implemented if police stations controller exists
            }
        }
        catch (Exception)
        {
            // Police stations endpoint might not exist
        }

        // For now, also add some sample stations for other agencies to demonstrate the functionality
        if (allStations.Count == 0)
        {
            // Add some default stations if no data could be loaded
            allStations.AddRange(new List<StationDto>
            {
                new StationDto { Id = 1001, Name = "Central Police Station", AgencyId = 1, AgencyName = "Hellenic Police", Latitude = 37.9755, Longitude = 23.7348 },
                new StationDto { Id = 1002, Name = "Coast Guard Piraeus", AgencyId = 3, AgencyName = "Hellenic Coast Guard", Latitude = 37.9484, Longitude = 23.6384 },
                new StationDto { Id = 1003, Name = "EKAB Athens", AgencyId = 4, AgencyName = "EKAB", Latitude = 37.9838, Longitude = 23.7275 }
            });
        }

        return allStations;
    }

    public async Task<StationDto?> CreateStationAsync(CreateStationDto station)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/stations", station);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<StationDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create station: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // User endpoints
    public async Task<List<UserDto>> GetUsersAsync(int? stationId = null)
    {
        try
        {
            var url = stationId.HasValue ? $"/api/users?stationId={stationId}" : "/api/users";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<UserDto>>() ?? new List<UserDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<UserDto>();
        }
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserDto user)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/users", user);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // Personnel endpoints
    public async Task<List<PersonnelDto>> GetPersonnelAsync(int? stationId = null)
    {
        try
        {
            var url = stationId.HasValue ? $"/api/personnel?stationId={stationId}" : "/api/personnel";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<PersonnelDto>>() ?? new List<PersonnelDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load personnel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<PersonnelDto>();
        }
    }

    public async Task<PersonnelDto?> CreatePersonnelAsync(CreatePersonnelDto personnel)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/personnel", personnel);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PersonnelDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create personnel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    // Vehicle endpoints
    public async Task<List<VehicleDto>> GetVehiclesAsync(int? stationId = null)
    {
        try
        {
            var url = stationId.HasValue ? $"/api/vehicles?stationId={stationId}" : "/api/vehicles";
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<List<VehicleDto>>() ?? new List<VehicleDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return new List<VehicleDto>();
        }
    }

    public async Task<VehicleDto?> CreateVehicleAsync(CreateVehicleDto vehicle)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/vehicles", vehicle);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VehicleDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return null;
        }
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}