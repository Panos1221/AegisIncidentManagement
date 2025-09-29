using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.IO;

namespace AegisDispatcher.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggingService _loggingService;
        private readonly string _settingsFilePath;
        private readonly string _userSettingsDirectory;
        private Dictionary<string, object> _userSettings;
        private readonly object _settingsLock = new object();

        public SettingsService(IConfiguration configuration, ILoggingService loggingService)
        {
            _configuration = configuration;
            _loggingService = loggingService;

            // User settings stored in AppData
            _userSettingsDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "AegisDispatcher");
            _settingsFilePath = Path.Combine(_userSettingsDirectory, "user-settings.json");

            Directory.CreateDirectory(_userSettingsDirectory);

            _userSettings = new Dictionary<string, object>();
            // Note: Settings will be loaded asynchronously when LoadSettingsAsync() is called
        }

        #region Application Settings

        public string GetApiBaseUrl()
        {
            return GetSetting("ApiSettings:BaseUrl", _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5000/api");
        }

        public void SetApiBaseUrl(string baseUrl)
        {
            SetSetting("ApiSettings:BaseUrl", baseUrl);
        }

        public TimeSpan GetApiTimeout()
        {
            var timeoutStr = GetSetting("ApiSettings:Timeout", _configuration["ApiSettings:Timeout"] ?? "00:01:00");
            return TimeSpan.TryParse(timeoutStr, out var timeout) ? timeout : TimeSpan.FromMinutes(1);
        }

        public void SetApiTimeout(TimeSpan timeout)
        {
            SetSetting("ApiSettings:Timeout", timeout.ToString());
        }

        #endregion

        #region User Preferences

        public string GetTheme()
        {
            return GetSetting("App:Theme", _configuration["App:Theme"] ?? "Dark");
        }

        public void SetTheme(string theme)
        {
            SetSetting("App:Theme", theme);
        }

        public string GetDefaultAgency()
        {
            return GetSetting("App:DefaultAgency", _configuration["App:DefaultAgency"] ?? "fire");
        }

        public void SetDefaultAgency(string agency)
        {
            SetSetting("App:DefaultAgency", agency);
        }

        public int GetAutoRefreshInterval()
        {
            var intervalStr = GetSetting("App:AutoRefreshInterval", _configuration["App:AutoRefreshInterval"] ?? "30000");
            return int.TryParse(intervalStr, out var interval) ? interval : 30000;
        }

        public void SetAutoRefreshInterval(int intervalMs)
        {
            SetSetting("App:AutoRefreshInterval", intervalMs.ToString());
        }

        public string GetMapProvider()
        {
            return GetSetting("App:MapProvider", _configuration["App:MapProvider"] ?? "OpenStreetMap");
        }

        public void SetMapProvider(string provider)
        {
            SetSetting("App:MapProvider", provider);
        }

        #endregion

        #region Window Settings

        public bool GetRememberWindowPosition()
        {
            return GetSetting("Window:RememberPosition", true);
        }

        public void SetRememberWindowPosition(bool remember)
        {
            SetSetting("Window:RememberPosition", remember);
        }

        public WindowSettings? GetWindowSettings()
        {
            var settingsJson = GetSetting<string>("Window:Settings", null);
            if (string.IsNullOrEmpty(settingsJson))
                return null;

            try
            {
                return JsonSerializer.Deserialize<WindowSettings>(settingsJson);
            }
            catch (JsonException ex)
            {
                _loggingService.LogWarning("Failed to deserialize window settings: {Error}", ex.Message);
                return null;
            }
        }

        public void SetWindowSettings(WindowSettings settings)
        {
            try
            {
                var settingsJson = JsonSerializer.Serialize(settings);
                SetSetting("Window:Settings", settingsJson);
            }
            catch (JsonException ex)
            {
                _loggingService.LogError(ex, "Failed to serialize window settings");
            }
        }

        #endregion

        #region Notification Settings

        public bool GetSoundEnabled()
        {
            return GetSetting("Notifications:SoundEnabled", true);
        }

        public void SetSoundEnabled(bool enabled)
        {
            SetSetting("Notifications:SoundEnabled", enabled);
        }

        public bool GetNotificationsEnabled()
        {
            return GetSetting("Notifications:Enabled", true);
        }

        public void SetNotificationsEnabled(bool enabled)
        {
            SetSetting("Notifications:Enabled", enabled);
        }

        public int GetNotificationDuration()
        {
            return GetSetting("Notifications:Duration", 5);
        }

        public void SetNotificationDuration(int durationSeconds)
        {
            SetSetting("Notifications:Duration", durationSeconds);
        }

        #endregion

        #region Logging Settings

        public string GetLogLevel()
        {
            return GetSetting("Logging:LogLevel", "Information");
        }

        public void SetLogLevel(string level)
        {
            SetSetting("Logging:LogLevel", level);
        }

        public int GetLogRetentionDays()
        {
            return GetSetting("Logging:RetentionDays", 30);
        }

        public void SetLogRetentionDays(int days)
        {
            SetSetting("Logging:RetentionDays", days);
        }

        #endregion

        #region Generic Settings

        public T GetSetting<T>(string key, T defaultValue)
        {
            lock (_settingsLock)
            {
                if (_userSettings.TryGetValue(key, out var value))
                {
                    try
                    {
                        if (value is T directValue)
                            return directValue;

                        if (value is JsonElement jsonElement)
                            return JsonSerializer.Deserialize<T>(jsonElement.GetRawText()) ?? defaultValue;

                        return (T)Convert.ChangeType(value, typeof(T)) ?? defaultValue;
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogWarning("Failed to convert setting {Key} to type {Type}: {Error}", key, typeof(T).Name, ex.Message);
                        return defaultValue;
                    }
                }

                return defaultValue;
            }
        }

        public void SetSetting<T>(string key, T value)
        {
            lock (_settingsLock)
            {
                _userSettings[key] = value ?? throw new ArgumentNullException(nameof(value));
            }

            // Auto-save settings when changed
            _ = Task.Run(SaveSettingsAsync);
        }

        public bool HasSetting(string key)
        {
            lock (_settingsLock)
            {
                return _userSettings.ContainsKey(key);
            }
        }

        public void RemoveSetting(string key)
        {
            lock (_settingsLock)
            {
                _userSettings.Remove(key);
            }

            _ = Task.Run(SaveSettingsAsync);
        }

        #endregion

        #region File Operations

        public async Task SaveSettingsAsync()
        {
            try
            {
                Dictionary<string, object> settingsToSave;
                lock (_settingsLock)
                {
                    settingsToSave = new Dictionary<string, object>(_userSettings);
                }

                var json = JsonSerializer.Serialize(settingsToSave, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(_settingsFilePath, json);
                _loggingService.LogDebug("User settings saved to {FilePath}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to save user settings");
            }
        }

        public async Task LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(_settingsFilePath))
                {
                    _loggingService.LogInformation("No user settings file found, using defaults");
                    return;
                }

                var json = await File.ReadAllTextAsync(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (settings != null)
                {
                    lock (_settingsLock)
                    {
                        _userSettings.Clear();
                        foreach (var kvp in settings)
                        {
                            _userSettings[kvp.Key] = kvp.Value;
                        }
                    }
                }

                _loggingService.LogInformation("User settings loaded from {FilePath}", _settingsFilePath);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to load user settings, using defaults");
                lock (_settingsLock)
                {
                    _userSettings.Clear();
                }
            }
        }

        public async Task ResetSettingsAsync()
        {
            try
            {
                lock (_settingsLock)
                {
                    _userSettings.Clear();
                }

                if (File.Exists(_settingsFilePath))
                {
                    File.Delete(_settingsFilePath);
                }

                _loggingService.LogInformation("User settings reset to defaults");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to reset user settings");
            }
        }

        public async Task<bool> ExportSettingsAsync(string filePath)
        {
            try
            {
                Dictionary<string, object> settingsToExport;
                lock (_settingsLock)
                {
                    settingsToExport = new Dictionary<string, object>(_userSettings);
                }

                var json = JsonSerializer.Serialize(settingsToExport, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await File.WriteAllTextAsync(filePath, json);
                _loggingService.LogInformation("Settings exported to {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to export settings to {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> ImportSettingsAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _loggingService.LogWarning("Settings import file not found: {FilePath}", filePath);
                    return false;
                }

                var json = await File.ReadAllTextAsync(filePath);
                var importedSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                if (importedSettings != null)
                {
                    lock (_settingsLock)
                    {
                        foreach (var kvp in importedSettings)
                        {
                            _userSettings[kvp.Key] = kvp.Value;
                        }
                    }

                    await SaveSettingsAsync();
                    _loggingService.LogInformation("Settings imported from {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to import settings from {FilePath}", filePath);
                return false;
            }
        }

        #endregion
    }
}
