using StationConfigurator.Services;
using StationConfigurator.Forms;

namespace StationConfigurator;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();

        // Initialize API client
        var apiClient = new ApiClient("http://localhost:5000");

        // Show login form
        using var loginForm = new LoginForm(apiClient);
        if (loginForm.ShowDialog() == DialogResult.OK)
        {
            // If login successful, show main form
            Application.Run(new MainForm(apiClient));
        }

        // Cleanup
        apiClient.Dispose();
    }
}