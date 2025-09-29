using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;
using AegisDispatcher.Views;

namespace AegisDispatcher
{
    public partial class MainWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IApiService _apiService;
        private readonly IServiceProvider _serviceProvider;
        private DispatcherTimer _clockTimer;
        private DispatcherTimer _refreshTimer;

        // Parameterless constructor for XAML instantiation
        public MainWindow()
        {
            InitializeComponent();
            
            // Get services from the static ServiceProvider
            _authService = App.ServiceProvider?.GetService<IAuthService>() 
                ?? throw new InvalidOperationException("AuthService not available");
            _apiService = App.ServiceProvider?.GetService<IApiService>() 
                ?? throw new InvalidOperationException("ApiService not available");
            _serviceProvider = App.ServiceProvider 
                ?? throw new InvalidOperationException("ServiceProvider not available");

            Initialize();
        }

        // Constructor for dependency injection (if needed)
        public MainWindow(IAuthService authService, IApiService apiService, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            
            _authService = authService;
            _apiService = apiService;
            _serviceProvider = serviceProvider;

            Initialize();
        }

        private void Initialize()
        {

            // Initialize timers
            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _clockTimer.Tick += UpdateClock;
            _clockTimer.Start();

            _refreshTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _refreshTimer.Tick += AutoRefresh;
            _refreshTimer.Start();

            // Initialize UI
            InitializeWindow();
            
            // Add keyboard event handler
            this.KeyDown += MainWindow_KeyDown;
            
            // Defer data loading until window is fully loaded
            Loaded += OnWindowLoaded;
        }

        private async void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWindowLoaded; // Remove the event handler to prevent multiple calls
            
            // Small delay to ensure all child controls are loaded
            await Task.Delay(100);
            
            // Load data
            LoadInitialData();
        }

        private void InitializeWindow()
        {
            // Set user information in menu bar
            var user = _authService.GetCurrentUser();
            if (user != null)
            {
                UserNameText.Text = user.Name;
                AgencyNameText.Text = user.AgencyName;
                
                // Set agency-specific color for the agency name
                SetAgencyColor(user.AgencyName);
            }

            // Set initial status
            UpdateStatus("Ready");
            UpdateConnectionStatus(true);

            // Initialize search box placeholder behavior
            InitializeSearchBox();
        }

        private void SetAgencyColor(string agencyName)
        {
            var agencyColor = GetAgencyColor(agencyName);
            AgencyNameText.Foreground = new System.Windows.Media.SolidColorBrush(agencyColor);
        }

        private System.Windows.Media.Color GetAgencyColor(string agencyName)
        {
            if (string.IsNullOrEmpty(agencyName))
                return System.Windows.Media.Colors.LightGray;

            // Match agency names and return appropriate colors
            switch (agencyName.ToLower())
            {
                case "hellenic fire service":
                case "fire department":
                case var name when name.Contains("fire"):
                    return System.Windows.Media.Color.FromRgb(220, 38, 38); // Red (#DC2626)
                
                case "hellenic police":
                case "police":
                case var name when name.Contains("police"):
                    return System.Windows.Media.Color.FromRgb(30, 64, 175); // Blue (#1E40AF)
                
                case "hellenic coast guard":
                case "coast guard":
                case var name when name.Contains("coast"):
                    return System.Windows.Media.Color.FromRgb(14, 165, 233); // Light Blue (#0EA5E9)
                
                case "ekab":
                case var name when name.Contains("ekab"):
                    return System.Windows.Media.Color.FromRgb(234, 179, 8); // Yellow (#EAB308)
                
                default:
                    return System.Windows.Media.Colors.LightGray; // Default fallback
            }
        }

        private void InitializeSearchBox()
        {
            if (SearchBox.Text == "Search incidents...")
            {
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private async void LoadInitialData()
        {
            UpdateStatus("Loading incidents...");
            ShowProgress(true);

            try
            {
                // Initialize control panels
                InitializeControlPanels();
                
                // Load incidents
                await RefreshIncidents();
                
                // Update status
                UpdateStatus("Ready");
                
                // Update incident count
                UpdateIncidentCount();
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading data: {ex.Message}");
                UpdateConnectionStatus(false);
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private void InitializeControlPanels()
        {
            // Wire up events for the control panels
            if (IncidentsPanel != null)
            {
                IncidentsPanel.IncidentSelected += OnIncidentSelected;
            }

            if (MapPanel != null)
            {
                // Set the current user for agency-specific filtering
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null)
                {
                    MapPanel.SetCurrentUser(currentUser);
                }
                
                MapPanel.LocationSelected += OnLocationSelected;
                MapPanel.IncidentClicked += OnMapIncidentClicked;
            }

            if (ActionResourcesPanel != null)
            {
                ActionResourcesPanel.ResourceSelected += OnResourceSelected;
                ActionResourcesPanel.StatusChanged += OnResourceStatusChanged;
            }

            // Initialize MapFiltersPanel
            if (MapFiltersPanel != null)
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser != null)
                {
                    MapFiltersPanel.SetCurrentUser(currentUser);
                }
            }
        }

        private async Task RefreshIncidents()
        {
            try
            {
                var incidents = await _apiService.GetIncidentsAsync();
                
                // Refresh the incidents panel
                if (IncidentsPanel != null)
                {
                    await IncidentsPanel.RefreshAsync();
                }
                
                UpdateIncidentCount(incidents?.Count ?? 0);
                UpdateConnectionStatus(true);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Failed to refresh incidents: {ex.Message}");
                UpdateConnectionStatus(false);
            }
        }

        #region Event Handlers

        private async void NewIncident_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var newIncidentWindow = _serviceProvider.GetRequiredService<NewIncidentWindow>();
                
                // Show as dialog
                if (newIncidentWindow.ShowDialog() == true)
                {
                    // Refresh incidents list after creating new incident
                    await RefreshIncidents();
                    UpdateStatus("New incident created successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open new incident window: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignSelected_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Get selected incident from incidents panel
                // Placeholder for assignment logic
                UpdateStatus("Assignment feature activated");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Assignment failed: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FilterResources_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Show the advanced map filters panel
                ShowMapFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Filter operation failed: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MapFilters_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMapFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open map filters: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowMapFilters()
        {
            // Sync the MapFiltersPanel with current MapPanel filters
            if (MapPanel != null && MapFiltersPanel != null)
            {
                var currentFilters = MapPanel.GetCurrentFilters();
                MapFiltersPanel.SetFilters(currentFilters);
            }
            
            MapFiltersPopup.IsOpen = true;
            UpdateStatus("Map filters opened");
        }

        private void HideMapFilters()
        {
            MapFiltersPopup.IsOpen = false;
            UpdateStatus("Map filters closed");
        }

        private void MapFiltersPanel_FilterChanged(object sender, AegisDispatcher.Controls.MapFilterChangedEventArgs e)
        {
            try
            {
                // Apply the filter change to the MapPanel
                if (MapPanel != null)
                {
                    MapPanel.ApplyExternalFilter(e.FilterType, e.IsEnabled);
                    UpdateStatus($"Map filter changed: {e.FilterType} = {e.IsEnabled}");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Filter change failed: {ex.Message}");
            }
        }

        private void MapFiltersPanel_CloseRequested(object sender, EventArgs e)
        {
            HideMapFilters();
        }

        private async void MapViewToggle_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MapPanel != null)
                {
                    await MapPanel.ToggleMapView();
                    
                    // Update button icon and tooltip based on current view
                    var isSatellite = MapPanel.IsSatelliteView;
                    MapViewToggleButton.Content = isSatellite ? "🗺️" : "🛰️";
                    MapViewToggleButton.ToolTip = isSatellite ? "Switch to Street View" : "Switch to Satellite View";
                    
                    UpdateStatus($"Switched to {(isSatellite ? "satellite" : "street")} view");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to toggle map view: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }





        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Refreshing data...");
            ShowProgress(true);

            try
            {
                await RefreshIncidents();
                
                // Also refresh the map data
                if (MapPanel != null)
                {
                    await MapPanel.RefreshMapDataAsync();
                }
                
                UpdateStatus("Data refreshed successfully");
            }
            catch (Exception ex)
            {
                UpdateStatus($"Refresh failed: {ex.Message}");
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private async void TestFireData_Click(object sender, RoutedEventArgs e)
        {
            UpdateStatus("Testing fire service data loading...");
            ShowProgress(true);

            try
            {
                if (MapPanel != null)
                {
                    await MapPanel.TestFireServiceDataLoading();
                    UpdateStatus("Fire service data test completed");
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Fire service data test failed: {ex.Message}");
            }
            finally
            {
                ShowProgress(false);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to logout?", 
                                       "Logout Confirmation", 
                                       MessageBoxButton.YesNo, 
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _authService.Logout();
                
                // Create and show login window
                var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
                loginWindow.Show();
                
                // Close this window
                this.Close();
            }
        }

        private async void StatusFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string filterValue = selectedItem.Content.ToString() ?? "All";
                
                // Update the incidents panel status filter
                if (IncidentsPanel != null)
                {
                    IncidentStatus? statusFilter = filterValue switch
                    {
                        "Created" => IncidentStatus.Created,
                        "OnGoing" => IncidentStatus.OnGoing,
                        "PartialControl" => IncidentStatus.PartialControl,
                        "Controlled" => IncidentStatus.Controlled,
                        "FullyControlled" => IncidentStatus.FullyControlled,
                        "Closed" => IncidentStatus.Closed,
                        _ => null // "All" or any other value
                    };
                    IncidentsPanel.StatusFilter = statusFilter;
                }
                
                UpdateStatus($"Filter changed to: {filterValue}");
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Search incidents...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search incidents...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private async void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox searchBox)
            {
                string searchText = searchBox.Text;
                
                // Update the incidents panel search filter
                if (IncidentsPanel != null)
                {
                    IncidentsPanel.SearchFilter = searchText;
                }
                
                // Update status
                if (!string.IsNullOrWhiteSpace(searchText) && searchText != "Search incidents...")
                {
                    UpdateStatus($"Searching for: {searchText}");
                }
                else
                {
                    UpdateStatus("Ready");
                }
            }
        }



        #endregion

        #region Timer Events

        private void UpdateClock(object? sender, EventArgs e)
        {
            TimeText.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private async void AutoRefresh(object? sender, EventArgs e)
        {
            // Only auto-refresh if not currently loading
            if (!StatusProgressBar.IsVisible)
            {
                try
                {
                    await RefreshIncidents();
                }
                catch
                {
                    // Silently fail on auto-refresh
                }
            }
        }

        #endregion

        #region UI Helper Methods

        private void UpdateStatus(string message)
        {
            StatusText.Text = message;
        }

        private void ShowProgress(bool show)
        {
            StatusProgressBar.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateConnectionStatus(bool connected)
        {
            ConnectionStatus.Background = connected 
                ? System.Windows.Media.Brushes.LimeGreen 
                : System.Windows.Media.Brushes.Red;
            ConnectionText.Text = connected ? "Connected" : "Disconnected";
        }

        private void UpdateIncidentCount(int count = 0)
        {
            IncidentCountText.Text = $"{count} active incident{(count != 1 ? "s" : "")}";
        }

        #endregion

        #region Window Events

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _clockTimer?.Stop();
            _refreshTimer?.Stop();
            base.OnClosing(e);
        }

        // Event handlers for control panel interactions
        private void OnIncidentSelected(object? sender, Incident incident)
        {
            Console.WriteLine($"MainWindow: OnIncidentSelected called for incident {incident.Id}");
            
            // Update the map to show the selected incident
            if (MapPanel != null)
            {
                Console.WriteLine($"MainWindow: Calling MapPanel.HighlightIncident for incident {incident.Id}");
                MapPanel.HighlightIncident(incident);
            }

            // Update the action resources panel with the selected incident
            if (ActionResourcesPanel != null)
            {
                Console.WriteLine($"MainWindow: Setting ActionResourcesPanel.SelectedIncident to {incident.Id}");
                ActionResourcesPanel.SelectedIncident = incident;
            }

            UpdateStatus($"Selected incident: {incident.MainCategory} - {incident.SubCategory}");
        }

        private async void OnLocationSelected(object sender, Point location)
        {
            try
            {
                // Open NewIncidentWindow with pre-filled location
                var newIncidentWindow = _serviceProvider.GetRequiredService<NewIncidentWindow>();
                
                // Set the location after the window is created
                await Task.Delay(100); // Small delay to ensure window is fully initialized
                newIncidentWindow.SetLocation(location.X, location.Y);
                
                // Show as dialog
                if (newIncidentWindow.ShowDialog() == true)
                {
                    // Refresh incidents list after creating new incident
                    await RefreshIncidents();
                    UpdateStatus("New incident created successfully");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open new incident window: {ex.Message}", 
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"Error opening new incident window");
            }
        }

        private void OnMapIncidentClicked(object? sender, Incident incident)
        {
            // Select the incident in the incidents panel using the new method
            if (IncidentsPanel != null)
            {
                IncidentsPanel.SelectIncidentById(incident.Id);
            }

            // Update the action resources panel
            if (ActionResourcesPanel != null)
            {
                ActionResourcesPanel.SelectedIncident = incident;
            }

            UpdateStatus($"Map incident clicked: {incident.MainCategory} - {incident.SubCategory}");
        }

        private void OnResourceSelected(object? sender, object resource)
        {
            UpdateStatus($"Resource selected: {resource?.GetType().Name}");
        }

        private void OnResourceStatusChanged(object? sender, string status)
        {
            UpdateStatus($"Resource status changed: {status}");
            
            // Refresh incidents and map to reflect any status changes
            _ = Task.Run(async () => 
            {
                await RefreshIncidents();
                
                // Also refresh the map data to update incident assignments
                if (MapPanel != null)
                {
                    await MapPanel.RefreshMapDataAsync();
                }
            });
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle keyboard shortcuts for quick action buttons
            if (e.Key == Key.N && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+N for New Incident
                NewIncident_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.F5)
            {
                // F5 for Refresh
                Refresh_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+L for Logout
                Logout_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }

        #endregion
    }
}