using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Hosting;
using AegisDispatcher.Services;
using AegisDispatcher.Views;

namespace AegisDispatcher;

/// <summary>
/// Main application class with dependency injection setup
/// </summary>
public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        // Create host and configure services
        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Use Serilog for logging
            .ConfigureServices((context, services) =>
            {
                // Configuration
                services.AddSingleton<IConfiguration>(configuration);

                // HTTP Client
                services.AddHttpClient();

                // Register LoggingService first (depends only on ILogger<T>)
                services.AddSingleton<ILoggingService, LoggingService>();

                // Then register other services that depend on ILoggingService
                services.AddSingleton<ISettingsService, SettingsService>();
                services.AddSingleton<IAuditLogService, AuditLogService>();
                services.AddSingleton<IApiService, ApiService>();
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<IGeocodingService, GeocodingService>();

                // Windows
                services.AddTransient<LoginWindow>();
                services.AddTransient<MainWindow>();
                services.AddTransient<NewIncidentWindow>();
            })
            .Build();

        try
        {
            // Set ServiceProvider immediately after host creation
            ServiceProvider = _host.Services;
            
            await _host.StartAsync();

            // Initialize logging
            var loggingService = ServiceProvider.GetRequiredService<ILoggingService>();
            loggingService.LogInformation("AegisDispatcher application started - services initialized");

            // Initialize settings service
            var settingsService = ServiceProvider.GetRequiredService<ISettingsService>();
            await settingsService.LoadSettingsAsync();
            loggingService.LogDebug("Settings service initialized");

            // Try to restore previous session
            var authService = ServiceProvider.GetRequiredService<IAuthService>();
            loggingService.LogDebug("AuthService obtained");
            
            var user = await authService.RestoreSessionAsync();
            loggingService.LogDebug("Session restoration completed");

            // For debugging - let's try to show LoginWindow directly without session restore
            loggingService.LogDebug("About to create LoginWindow...");
            
            try 
            {
                var loginWindow = ServiceProvider.GetRequiredService<LoginWindow>();
                loggingService.LogDebug("LoginWindow created successfully");
                
                loginWindow.Show();
                loggingService.LogDebug("LoginWindow.Show() called");
                
                // Check if window is actually visible
                loggingService.LogDebug("LoginWindow IsVisible: {IsVisible}, WindowState: {WindowState}", 
                    loginWindow.IsVisible, loginWindow.WindowState);
            }
            catch (Exception windowEx)
            {
                loggingService.LogError(windowEx, "Failed to create or show LoginWindow");
                throw;
            }
            
            loggingService.LogDebug("Startup sequence completed");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Failed to start application: {ex.Message}\n\nDetails: {ex}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
        {
            // Save settings before exit
            var settingsService = ServiceProvider?.GetService<ISettingsService>();
            if (settingsService != null)
            {
                await settingsService.SaveSettingsAsync();
            }

            // Log application shutdown
            var loggingService = ServiceProvider?.GetService<ILoggingService>();
            loggingService?.LogInformation("AegisDispatcher application shutting down");

            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        }
        finally
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }
    }
}

