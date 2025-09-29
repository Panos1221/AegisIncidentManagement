using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.IO;
using Microsoft.Extensions.Configuration;
using AegisDispatcher.Models;

namespace AegisDispatcher.Services
{
    public interface IAuthService
    {
        event EventHandler<User?>? UserChanged;
        Task<User?> LoginAsync(LoginRequest loginRequest);
        void Logout();
        User? GetCurrentUser();
        bool IsAuthenticated { get; }
        bool IsDispatcher();
        bool CanCreateIncidents();
        bool CanAssignResources();
        string? GetStoredToken();
        Task<User?> RestoreSessionAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private User? _currentUser;
        private string? _currentToken;

        public event EventHandler<User?>? UserChanged;

        public bool IsAuthenticated => _currentUser != null && !string.IsNullOrEmpty(_currentToken);

        public AuthService(IApiService apiService, IConfiguration configuration, ILoggingService loggingService)
        {
            _apiService = apiService;
            _configuration = configuration;
            _loggingService = loggingService;
            _loggingService.LogDebug("AuthService initialized");
        }

        public async Task<User?> LoginAsync(LoginRequest loginRequest)
        {
            try
            {
                _loggingService.LogInformation("Authentication attempt for email: {Email}", loginRequest.Email);
                
                var response = await _apiService.LoginAsync(loginRequest);
                
                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    _currentToken = response.Token;
                    _currentUser = MapLoginResponseToUser(response);
                    
                    // Store token securely
                    StoreToken(_currentToken);
                    
                    // Set API token for future requests
                    _apiService.SetAuthToken(_currentToken);
                    
                    _loggingService.LogUserLogin(loginRequest.Email, _currentUser.Role.ToString(), true);
                    
                    UserChanged?.Invoke(this, _currentUser);
                    return _currentUser;
                }
                
                _loggingService.LogUserLogin(loginRequest.Email, "Unknown", false);
                return null;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Authentication failed for email: {Email}", loginRequest.Email);
                _loggingService.LogUserLogin(loginRequest.Email, "Unknown", false);
                throw new AuthenticationException($"Login failed: {ex.Message}", ex);
            }
        }

        public void Logout()
        {
            var userEmail = _currentUser?.Email ?? "Unknown";
            
            _currentUser = null;
            _currentToken = null;
            
            // Clear stored token
            ClearStoredToken();
            
            // Clear API token
            _apiService.ClearAuthToken();
            
            _loggingService.LogUserLogout(userEmail);
            UserChanged?.Invoke(this, null);
        }

        public User? GetCurrentUser()
        {
            return _currentUser;
        }

        public bool IsDispatcher()
        {
            if (_currentUser == null) return false;
            
            return _currentUser.Role == UserRole.Dispatcher ||
                   _currentUser.Role == UserRole.FireDispatcher ||
                   _currentUser.Role == UserRole.CoastGuardDispatcher ||
                   _currentUser.Role == UserRole.EKABDispatcher;
        }

        public bool CanCreateIncidents()
        {
            return IsDispatcher();
        }

        public bool CanAssignResources()
        {
            return IsDispatcher();
        }

        public string? GetStoredToken()
        {
            try
            {
                // In a real application, you'd want to use secure storage
                // For simplicity, we'll use a file in the user's app data folder
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "AegisDispatcher");
                
                var tokenFile = Path.Combine(appDataPath, "token.dat");
                
                if (File.Exists(tokenFile))
                {
                    return File.ReadAllText(tokenFile);
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<User?> RestoreSessionAsync()
        {
            try
            {
                var storedToken = GetStoredToken();
                
                if (!string.IsNullOrEmpty(storedToken))
                {
                    // Validate token by trying to decode it
                    var user = GetUserFromToken(storedToken);
                    
                    if (user != null && !IsTokenExpired(storedToken))
                    {
                        _currentToken = storedToken;
                        _currentUser = user;
                        _apiService.SetAuthToken(storedToken);
                        
                        UserChanged?.Invoke(this, _currentUser);
                        return _currentUser;
                    }
                    else
                    {
                        // Token is expired or invalid, clear it
                        ClearStoredToken();
                    }
                }
                
                return null;
            }
            catch
            {
                ClearStoredToken();
                return null;
            }
        }

        private User MapLoginResponseToUser(LoginResponse response)
        {
            return new User
            {
                Id = response.Id,
                Email = response.Email,
                Name = response.Name,
                Role = response.Role,
                AgencyId = response.AgencyId,
                AgencyName = response.AgencyName,
                StationId = response.StationId,
                StationName = response.StationName,
                IsActive = response.IsActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        private void StoreToken(string token)
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "AegisDispatcher");
                
                Directory.CreateDirectory(appDataPath);
                
                var tokenFile = Path.Combine(appDataPath, "token.dat");
                File.WriteAllText(tokenFile, token);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to store authentication token");
            }
        }

        private void ClearStoredToken()
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                    "AegisDispatcher");
                
                var tokenFile = Path.Combine(appDataPath, "token.dat");
                
                if (File.Exists(tokenFile))
                {
                    File.Delete(tokenFile);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to clear stored authentication token");
            }
        }

        private User? GetUserFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
                var emailClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value;
                var nameClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value;
                var roleClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "role")?.Value;
                var agencyIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "AgencyId")?.Value;
                var stationIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "StationId")?.Value;
                
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                    return null;
                
                Enum.TryParse<UserRole>(roleClaim, out var role);
                int.TryParse(agencyIdClaim, out var agencyId);
                int.TryParse(stationIdClaim, out var stationId);
                
                return new User
                {
                    Id = userId,
                    Email = emailClaim ?? "",
                    Name = nameClaim ?? "",
                    Role = role,
                    AgencyId = agencyId > 0 ? agencyId : null,
                    StationId = stationId > 0 ? stationId : null,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch
            {
                return null;
            }
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadJwtToken(token);
                
                return jsonToken.ValidTo < DateTime.UtcNow;
            }
            catch
            {
                return true; // Consider it expired if we can't parse it
            }
        }
    }

    public class AuthenticationException : Exception
    {
        public AuthenticationException(string message) : base(message) { }
        public AuthenticationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
