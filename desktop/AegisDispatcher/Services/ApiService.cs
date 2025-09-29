using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using AegisDispatcher.Models;

namespace AegisDispatcher.Services
{
    public interface IApiService
    {
        Task<LoginResponse?> LoginAsync(LoginRequest loginRequest);
        Task<List<Incident>> GetIncidentsAsync(int? stationId = null, IncidentStatus? status = null, DateTime? from = null, DateTime? to = null);
        Task<Incident?> GetIncidentAsync(int id);
        Task<Incident> CreateIncidentAsync(CreateIncident incident);
        Task<bool> UpdateIncidentStatusAsync(int id, IncidentStatus status);
        Task<bool> AssignResourceAsync(CreateAssignment assignment);
        Task<bool> UpdateAssignmentStatusAsync(int incidentId, int assignmentId, string status);
        Task<List<Station>> GetStationsAsync();
        Task<List<Vehicle>> GetVehiclesAsync(int? stationId = null);
        Task<List<Personnel>> GetPersonnelAsync(int? stationId = null);
        Task<IncidentTypesByAgency?> GetIncidentTypesAsync(string agencyName);
        Task<StationAssignmentResponse?> FindStationByLocationAsync(StationAssignmentRequest request);
        
        // Additional endpoints for map functionality
        Task<List<FireHydrant>> GetFireHydrantsAsync();
        Task<List<Ship>> GetShipsAsync();
        Task<List<CoastGuardStation>> GetCoastGuardStationsAsync();
        Task<List<PoliceStation>> GetPoliceStationsAsync();
        Task<List<Hospital>> GetHospitalsAsync();
        Task<List<PatrolZone>> GetPatrolZonesAsync(int? agencyId = null, int? stationId = null);
        Task<PatrolZone> CreatePatrolZoneAsync(CreatePatrolZone patrolZone);
        Task<bool> UpdatePatrolZoneAsync(int id, PatrolZone patrolZone);
        Task<bool> DeletePatrolZoneAsync(int id);
        Task<List<FireStationBoundary>> GetFireStationBoundariesAsync();
        
        void SetAuthToken(string token);
        void ClearAuthToken();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILoggingService _loggingService;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseUrl;

        public ApiService(HttpClient httpClient, IConfiguration configuration, ILoggingService loggingService)
        {
            _httpClient = httpClient;
            _loggingService = loggingService;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api";
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.Timeout = TimeSpan.Parse(configuration["ApiSettings:Timeout"] ?? "00:01:00");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };

            _loggingService.LogInformation("ApiService initialized with base URL: {BaseUrl}", _baseUrl);
        }

        public void SetAuthToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public void ClearAuthToken()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _loggingService.LogDebug("Login attempt for email: {Email}", loginRequest.Email);
                
                var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/auth/login", content);
                stopwatch.Stop();
                
                _loggingService.LogApiCall("/api/auth/login", "POST", (int)response.StatusCode, stopwatch.ElapsedMilliseconds);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _jsonOptions);
                    _loggingService.LogInformation("Login successful for email: {Email}", loginRequest.Email);
                    return loginResponse;
                }
                
                _loggingService.LogWarning("Login failed for email: {Email}, Status: {StatusCode}", loginRequest.Email, response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _loggingService.LogError(ex, "Login request failed for email: {Email}", loginRequest.Email);
                throw new ApiException($"Login failed: {ex.Message}", ex);
            }
        }

        public async Task<List<Incident>> GetIncidentsAsync(int? stationId = null, IncidentStatus? status = null, DateTime? from = null, DateTime? to = null)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (stationId.HasValue)
                    queryParams.Add($"stationId={stationId.Value}");
                if (status.HasValue)
                    queryParams.Add($"status={status.Value}");
                if (from.HasValue)
                    queryParams.Add($"from={from.Value:yyyy-MM-ddTHH:mm:ss}");
                if (to.HasValue)
                    queryParams.Add($"to={to.Value:yyyy-MM-ddTHH:mm:ss}");
                
                var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var fullUrl = $"/api/incidents{query}";
                
                _loggingService.LogInformation("GetIncidentsAsync: Making API call to {Url}", fullUrl);
                
                var response = await _httpClient.GetAsync(fullUrl);
                
                _loggingService.LogInformation("GetIncidentsAsync: API response status {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _loggingService.LogInformation("GetIncidentsAsync: Response content length {Length}", content.Length);
                    
                    var incidents = JsonSerializer.Deserialize<List<Incident>>(content, _jsonOptions) ?? new List<Incident>();
                    _loggingService.LogInformation("GetIncidentsAsync: Deserialized {Count} incidents", incidents.Count);
                    
                    return incidents;
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                _loggingService.LogWarning("GetIncidentsAsync: API call failed with status {StatusCode}, content: {Content}", response.StatusCode, errorContent);
                
                return new List<Incident>();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "GetIncidentsAsync: Exception occurred");
                throw new ApiException($"Failed to get incidents: {ex.Message}", ex);
            }
        }

        public async Task<Incident?> GetIncidentAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/incidents/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<Incident>(content, _jsonOptions);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get incident {id}: {ex.Message}", ex);
            }
        }

        public async Task<Incident> CreateIncidentAsync(CreateIncident incident)
        {
            try
            {
                var json = JsonSerializer.Serialize(incident, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/incidents", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    // The API returns a direct Incident object, not wrapped in ApiResponse
                    var createdIncident = JsonSerializer.Deserialize<Incident>(responseContent, _jsonOptions);
                    return createdIncident ?? throw new ApiException("No incident data returned");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApiException($"Failed to create incident: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException($"Failed to create incident: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateIncidentStatusAsync(int id, IncidentStatus status)
        {
            try
            {
                var updateData = new { status = status };
                var json = JsonSerializer.Serialize(updateData, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/incidents/{id}/status", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to update incident status: {ex.Message}", ex);
            }
        }

        public async Task<bool> AssignResourceAsync(CreateAssignment assignment)
        {
            try
            {
                _loggingService.LogInformation("AssignResourceAsync: Starting assignment for incident {IncidentId}, resource {ResourceId} ({ResourceType})",
                    assignment.IncidentId, assignment.ResourceId, assignment.ResourceType);

                var json = JsonSerializer.Serialize(assignment, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _loggingService.LogDebug("AssignResourceAsync: Sending request to /api/incidents/{IncidentId}/assign", assignment.IncidentId);
                var response = await _httpClient.PostAsync($"/api/incidents/{assignment.IncidentId}/assign", content);

                _loggingService.LogInformation("AssignResourceAsync: API response status {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _loggingService.LogInformation("AssignResourceAsync: Successfully assigned resource {ResourceId} to incident {IncidentId}",
                        assignment.ResourceId, assignment.IncidentId);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _loggingService.LogError("AssignResourceAsync: Failed to assign resource. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new ApiException($"Assignment failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (ApiException)
            {
                throw; // Re-throw API exceptions as-is
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "AssignResourceAsync: Exception occurred during assignment");
                throw new ApiException($"Failed to assign resource: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdateAssignmentStatusAsync(int incidentId, int assignmentId, string status)
        {
            try
            {
                _loggingService.LogInformation("UpdateAssignmentStatusAsync: Starting status update for incident {IncidentId}, assignment {AssignmentId}, status {Status}",
                    incidentId, assignmentId, status);

                var json = JsonSerializer.Serialize(new { status }, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _loggingService.LogDebug("UpdateAssignmentStatusAsync: Sending request to /api/incidents/{IncidentId}/assignments/{AssignmentId}/status", incidentId, assignmentId);
                var response = await _httpClient.PutAsync($"/api/incidents/{incidentId}/assignments/{assignmentId}/status", content);

                _loggingService.LogInformation("UpdateAssignmentStatusAsync: API response status {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    _loggingService.LogInformation("UpdateAssignmentStatusAsync: Successfully updated assignment {AssignmentId} to status {Status}",
                        assignmentId, status);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _loggingService.LogError("UpdateAssignmentStatusAsync: Failed to update assignment status. Status: {StatusCode}, Error: {Error}",
                        response.StatusCode, errorContent);
                    throw new ApiException($"Assignment status update failed: {response.StatusCode} - {errorContent}");
                }
            }
            catch (ApiException)
            {
                throw; // Re-throw API exceptions as-is
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "UpdateAssignmentStatusAsync: Exception occurred during assignment status update");
                throw new ApiException($"Failed to update assignment status: {ex.Message}", ex);
            }
        }

        public async Task<List<Station>> GetStationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/stations");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Station>>(content, _jsonOptions) ?? new List<Station>();
                }
                
                return new List<Station>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get stations: {ex.Message}", ex);
            }
        }

        public async Task<List<Vehicle>> GetVehiclesAsync(int? stationId = null)
        {
            try
            {
                var query = stationId.HasValue ? $"?stationId={stationId.Value}" : "";
                var response = await _httpClient.GetAsync($"/api/vehicles{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Vehicle>>(content, _jsonOptions) ?? new List<Vehicle>();
                }
                
                return new List<Vehicle>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get vehicles: {ex.Message}", ex);
            }
        }

        public async Task<List<Personnel>> GetPersonnelAsync(int? stationId = null)
        {
            try
            {
                var query = stationId.HasValue ? $"?stationId={stationId.Value}" : "";
                var response = await _httpClient.GetAsync($"/api/personnel{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Personnel>>(content, _jsonOptions) ?? new List<Personnel>();
                }
                
                return new List<Personnel>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get personnel: {ex.Message}", ex);
            }
        }

        public async Task<IncidentTypesByAgency?> GetIncidentTypesAsync(string agencyName)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/incidenttypes/agency/{Uri.EscapeDataString(agencyName)}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<IncidentTypesByAgency>>(content, _jsonOptions);
                    return apiResponse?.Data;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get incident types: {ex.Message}", ex);
            }
        }

        public async Task<StationAssignmentResponse?> FindStationByLocationAsync(StationAssignmentRequest request)
        {
            try
            {
                _loggingService.LogInformation("FindStationByLocationAsync: Starting station assignment for coordinates {Latitude}, {Longitude}, agency type: {AgencyType}",
                    request.Latitude, request.Longitude, request.AgencyType);

                var json = JsonSerializer.Serialize(request, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _loggingService.LogDebug("FindStationByLocationAsync: Sending request to /api/stationassignment/find-by-location with payload: {Payload}", json);
                
                var response = await _httpClient.PostAsync("/api/stationassignment/find-by-location", content);
                
                _loggingService.LogInformation("FindStationByLocationAsync: API response status {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _loggingService.LogDebug("FindStationByLocationAsync: Response content: {Content}", responseContent);
                    
                    var result = JsonSerializer.Deserialize<StationAssignmentResponse>(responseContent, _jsonOptions);
                    
                    if (result != null)
                    {
                        _loggingService.LogInformation("FindStationByLocationAsync: Successfully found station {StationId} ({StationName}) at {Distance}m using {Method}",
                            result.StationId, result.StationName, result.Distance, result.AssignmentMethod);
                    }
                    else
                    {
                        _loggingService.LogWarning("FindStationByLocationAsync: Response was successful but deserialized to null");
                    }
                    
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _loggingService.LogWarning("FindStationByLocationAsync: API call failed with status {StatusCode}, error: {Error}",
                        response.StatusCode, errorContent);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "FindStationByLocationAsync: Exception occurred while finding station by location");
                throw new ApiException($"Failed to find station by location: {ex.Message}", ex);
            }
        }

        public async Task<List<FireHydrant>> GetFireHydrantsAsync()
        {
            try
            {
                _loggingService.LogInformation("ApiService: Making fire hydrants API call to /api/firehydrants");
                
                var response = await _httpClient.GetAsync("/api/firehydrants");
                
                _loggingService.LogInformation("ApiService: Fire hydrants API response - Status: {StatusCode}, Content Length: {Length}", 
                    response.StatusCode, response.Content.Headers.ContentLength);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _loggingService.LogInformation("ApiService: Fire hydrants response content: {Content}", 
                        content.Length > 100 ? content.Substring(0, 100) + "..." : content);
                    
                    var result = JsonSerializer.Deserialize<List<FireHydrant>>(content, _jsonOptions) ?? new List<FireHydrant>();
                    _loggingService.LogInformation("ApiService: Deserialized {Count} fire hydrants", result.Count);
                    return result;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _loggingService.LogWarning("ApiService: Fire hydrants API failed - Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                }
                
                return new List<FireHydrant>();
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "ApiService: Fire hydrants API call failed: {Message}", ex.Message);
                throw new ApiException($"Failed to get fire hydrants: {ex.Message}", ex);
            }
        }

        public async Task<List<Ship>> GetShipsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/ships");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Ship>>(content, _jsonOptions) ?? new List<Ship>();
                }
                
                return new List<Ship>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get ships: {ex.Message}", ex);
            }
        }

        public async Task<List<CoastGuardStation>> GetCoastGuardStationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/coastguardstations");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<CoastGuardStation>>(content, _jsonOptions) ?? new List<CoastGuardStation>();
                }
                
                return new List<CoastGuardStation>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get coast guard stations: {ex.Message}", ex);
            }
        }

        public async Task<List<PoliceStation>> GetPoliceStationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/policestations");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<PoliceStation>>(content, _jsonOptions) ?? new List<PoliceStation>();
                }
                
                return new List<PoliceStation>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get police stations: {ex.Message}", ex);
            }
        }

        public async Task<List<Hospital>> GetHospitalsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/hospitals");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Hospital>>(content, _jsonOptions) ?? new List<Hospital>();
                }
                
                return new List<Hospital>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get hospitals: {ex.Message}", ex);
            }
        }

        public async Task<List<PatrolZone>> GetPatrolZonesAsync(int? agencyId = null, int? stationId = null)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (agencyId.HasValue)
                    queryParams.Add($"agencyId={agencyId.Value}");
                if (stationId.HasValue)
                    queryParams.Add($"stationId={stationId.Value}");
                
                var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/api/patrolzones{query}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<PatrolZone>>(content, _jsonOptions) ?? new List<PatrolZone>();
                }
                
                return new List<PatrolZone>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get patrol zones: {ex.Message}", ex);
            }
        }

        public async Task<PatrolZone> CreatePatrolZoneAsync(CreatePatrolZone patrolZone)
        {
            try
            {
                var json = JsonSerializer.Serialize(patrolZone, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync("/api/patrolzones", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse<PatrolZone>>(responseContent, _jsonOptions);
                    return apiResponse?.Data ?? throw new ApiException("No patrol zone data returned");
                }
                
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new ApiException($"Failed to create patrol zone: {response.StatusCode} - {errorContent}");
            }
            catch (Exception ex) when (!(ex is ApiException))
            {
                throw new ApiException($"Failed to create patrol zone: {ex.Message}", ex);
            }
        }

        public async Task<bool> UpdatePatrolZoneAsync(int id, PatrolZone patrolZone)
        {
            try
            {
                var json = JsonSerializer.Serialize(patrolZone, _jsonOptions);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync($"/api/patrolzones/{id}", content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to update patrol zone: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeletePatrolZoneAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/patrolzones/{id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to delete patrol zone: {ex.Message}", ex);
            }
        }

        public async Task<List<FireStationBoundary>> GetFireStationBoundariesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/FireStations/boundaries");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<FireStationBoundary>>(content, _jsonOptions) ?? new List<FireStationBoundary>();
                }
                
                return new List<FireStationBoundary>();
            }
            catch (Exception ex)
            {
                throw new ApiException($"Failed to get fire station boundaries: {ex.Message}", ex);
            }
        }
    }

    public class ApiException : Exception
    {
        public ApiException(string message) : base(message) { }
        public ApiException(string message, Exception innerException) : base(message, innerException) { }
    }
}
