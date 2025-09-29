namespace AegisDispatcher.Services
{
    public interface ISettingsService
    {
        // Application Settings
        string GetApiBaseUrl();
        void SetApiBaseUrl(string baseUrl);
        TimeSpan GetApiTimeout();
        void SetApiTimeout(TimeSpan timeout);
        
        // User Preferences  
        string GetTheme();
        void SetTheme(string theme);
        string GetDefaultAgency();
        void SetDefaultAgency(string agency);
        int GetAutoRefreshInterval();
        void SetAutoRefreshInterval(int intervalMs);
        string GetMapProvider();
        void SetMapProvider(string provider);
        
        // Window Settings
        bool GetRememberWindowPosition();
        void SetRememberWindowPosition(bool remember);
        WindowSettings? GetWindowSettings();
        void SetWindowSettings(WindowSettings settings);
        
        // Notification Settings
        bool GetSoundEnabled();
        void SetSoundEnabled(bool enabled);
        bool GetNotificationsEnabled();
        void SetNotificationsEnabled(bool enabled);
        int GetNotificationDuration();
        void SetNotificationDuration(int durationSeconds);
        
        // Logging Settings
        string GetLogLevel();
        void SetLogLevel(string level);
        int GetLogRetentionDays();
        void SetLogRetentionDays(int days);
        
        // Generic Settings
        T GetSetting<T>(string key, T defaultValue);
        void SetSetting<T>(string key, T value);
        bool HasSetting(string key);
        void RemoveSetting(string key);
        
        // File Operations
        Task SaveSettingsAsync();
        Task LoadSettingsAsync();
        Task ResetSettingsAsync();
        Task<bool> ExportSettingsAsync(string filePath);
        Task<bool> ImportSettingsAsync(string filePath);
    }

    public class WindowSettings
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsMaximized { get; set; }
    }
}
