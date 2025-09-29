using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;
using AegisDispatcher.Controls;

namespace AegisDispatcher.Views
{
    public partial class NewIncidentWindow : Window
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly IGeocodingService _geocodingService;
        private readonly ILoggingService _loggingService;
        private readonly LocalIncidentDataService _localIncidentDataService;
        private List<IncidentTypeCategory> _incidentCategories = new();
        private List<Station> _stations = new();
        private bool _isLoadingStations = false;

        public Incident? CreatedIncident { get; private set; }

        public NewIncidentWindow(IApiService apiService, IAuthService authService, IGeocodingService geocodingService, ILoggingService loggingService)
        {
            InitializeComponent();
            _apiService = apiService;
            _authService = authService;
            _geocodingService = geocodingService;
            _loggingService = loggingService;
            _localIncidentDataService = new LocalIncidentDataService(loggingService);

            // Initialize the address search control with the geocoding service
            AddressSearchControl.SetGeocodingService(_geocodingService);
            AddressSearchControl.AddressSelected += AddressSearchControl_AddressSelected;

            // Enable left-click location selection and disable context menu for this window
            LocationMapPanel.EnableLeftClickLocationSelection = true;
            LocationMapPanel.DisableContextMenu = true;
            LocationMapPanel.HideOtherMarkers = true;

            LoadInitialData();
        }

        /// <summary>
        /// Sets the location for the incident and performs reverse geocoding to fill address fields
        /// </summary>
        /// <param name="latitude">The latitude coordinate</param>
        /// <param name="longitude">The longitude coordinate</param>
        public async void SetLocation(double latitude, double longitude)
        {
            try
            {
                // Update coordinates
                LatitudeTextBox.Text = latitude.ToString("F6");
                LongitudeTextBox.Text = longitude.ToString("F6");
                
                // Try to reverse geocode the location to get address
                if (_geocodingService != null)
                {
                    var result = await _geocodingService.ReverseGeocodeAsync(latitude, longitude);
                    if (result != null)
                    {
                        AddressTextBox.Text = result.DisplayName;
                        AddressSearchControl.SetSearchText(result.DisplayName);
                        
                        // Update individual address fields if available
                        if (!string.IsNullOrEmpty(result.Street))
                            StreetTextBox.Text = result.Street;
                        
                        if (!string.IsNullOrEmpty(result.HouseNumber))
                            StreetNumberTextBox.Text = result.HouseNumber;
                        
                        if (!string.IsNullOrEmpty(result.City))
                            CityTextBox.Text = result.City;
                        
                        if (!string.IsNullOrEmpty(result.PostalCode))
                            PostalCodeTextBox.Text = result.PostalCode;
                        
                        if (!string.IsNullOrEmpty(result.Region))
                            RegionTextBox.Text = result.Region;
                        
                        if (!string.IsNullOrEmpty(result.Country))
                            CountryTextBox.Text = result.Country;
                    }
                }
                
                UpdateMapPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to set location: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void LoadInitialData()
        {
            try
            {
                ShowStatus("Loading incident types and stations...", true);

                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    MessageBox.Show("User session expired. Please log in again.", "Authentication Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    DialogResult = false;
                    return;
                }

                // Load incident types
                if (!string.IsNullOrEmpty(user.AgencyName))
                {
                    bool incidentTypesLoaded = false;
                    
                    // Try to load from API first
                    try
                    {
                        var incidentTypes = await _apiService.GetIncidentTypesAsync(user.AgencyName);
                        if (incidentTypes?.Categories != null)
                        {
                            _incidentCategories = incidentTypes.Categories;
                            PopulateMainCategories();
                            incidentTypesLoaded = true;
                        }
                    }
                    catch (Exception incidentEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"API failed to load incident categories: {incidentEx.Message}");
                    }

                    // If API failed, try to load from local data
                    if (!incidentTypesLoaded)
                    {
                        try
                        {
                            System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Attempting to load local data for agency: {user.AgencyName}");
                            var localIncidentTypes = await _localIncidentDataService.GetIncidentTypesByAgencyAsync(user.AgencyName);
                            System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Local data result: {(localIncidentTypes != null ? "Not null" : "Null")}");
                            
                            if (localIncidentTypes?.Categories != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Local data categories count: {localIncidentTypes.Categories.Count}");
                                // Use the local incident types directly as they match the expected format
                                _incidentCategories = localIncidentTypes.Categories;
                                
                                PopulateMainCategories();
                                incidentTypesLoaded = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Local data is null or has no categories");
                            }
                        }
                        catch (Exception localEx)
                        {
                            System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Local data service failed: {localEx.Message}");
                            System.Diagnostics.Debug.WriteLine($"[NewIncidentWindow] Local data service stack trace: {localEx.StackTrace}");
                        }
                    }

                    // If both API and local data failed
                    if (!incidentTypesLoaded)
                    {
                        MessageBox.Show($"No incident categories found for agency: {user.AgencyName}. Both online and offline data are unavailable.", "Warning", 
                                      MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }

                // Load stations
                var stations = await _apiService.GetStationsAsync();
                _stations = stations.ToList();
                PopulateStations();

                HideStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load initial data: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
                HideStatus();
            }
        }

        private void PopulateMainCategories()
        {
            MainCategoryCombo.Items.Clear();
            MainCategoryCombo.Items.Add(new ComboBoxItem { Content = "Select Main Category", IsEnabled = false });
            MainCategoryCombo.SelectedIndex = 0;

            if (_incidentCategories != null && _incidentCategories.Any())
            {
                foreach (var category in _incidentCategories)
                {
                    // Prefer English name, fallback to Greek name, then to category key
                    var displayName = !string.IsNullOrWhiteSpace(category.CategoryNameEn) 
                        ? category.CategoryNameEn 
                        : !string.IsNullOrWhiteSpace(category.CategoryNameEl) 
                            ? category.CategoryNameEl 
                            : category.CategoryKey ?? "Unknown Category";
                    
                    var item = new ComboBoxItem
                    {
                        Content = displayName,
                        Tag = category
                    };
                    MainCategoryCombo.Items.Add(item);
                }
            }
        }

        private void PopulateStations()
        {
            StationCombo.Items.Clear();
            StationCombo.Items.Add(new ComboBoxItem { Content = "Select Station", IsEnabled = false });
            StationCombo.SelectedIndex = 0;

            foreach (var station in _stations)
            {
                var item = new ComboBoxItem
                {
                    Content = station.Name,
                    Tag = station
                };
                StationCombo.Items.Add(item);
            }
        }

        private void MainCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SubCategoryCombo.Items.Clear();
            SubCategoryCombo.IsEnabled = false;

            if (MainCategoryCombo.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is IncidentTypeCategory category)
            {
                SubCategoryCombo.Items.Add(new ComboBoxItem { Content = "Select Sub Category", IsEnabled = false });
                SubCategoryCombo.SelectedIndex = 0;

                foreach (var subcategory in category.Subcategories)
                {
                    // Prefer English name, fallback to Greek name
                    var displayName = !string.IsNullOrWhiteSpace(subcategory.SubcategoryNameEn) 
                        ? subcategory.SubcategoryNameEn 
                        : !string.IsNullOrWhiteSpace(subcategory.SubcategoryNameEl) 
                            ? subcategory.SubcategoryNameEl 
                            : "Unknown Subcategory";
                    
                    var item = new ComboBoxItem
                    {
                        Content = displayName,
                        Tag = subcategory
                    };
                    SubCategoryCombo.Items.Add(item);
                }

                SubCategoryCombo.IsEnabled = true;
            }
        }

        private void Address_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TODO: Implement address geocoding
            // For now, we'll just update the preview
            UpdateMapPreview();
        }

        private void Coordinates_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(LatitudeTextBox.Text, out var lat) && 
                double.TryParse(LongitudeTextBox.Text, out var lng))
            {
                MapPreviewCoords.Text = $"Lat: {lat:F6}, Lng: {lng:F6}";
                
                // Auto-assign station based on coordinates
                AutoAssignStation(lat, lng);
            }
            else
            {
                MapPreviewCoords.Text = "Invalid coordinates";
            }
            
            UpdateMapPreview();
        }

        private async void AutoAssignStation(double latitude, double longitude)
        {
            if (_isLoadingStations) return;
            
            _loggingService.LogInformation("AutoAssignStation: Starting automatic station assignment for coordinates {Latitude}, {Longitude}", latitude, longitude);
            
            try
            {
                _isLoadingStations = true;
                
                var user = _authService.GetCurrentUser();
                if (user?.AgencyName == null) return;

                var agencyType = GetAgencyTypeString(user.AgencyName);
                var request = new StationAssignmentRequest
                {
                    Latitude = latitude,
                    Longitude = longitude,
                    AgencyType = agencyType
                };

                var stationAssignment = await _apiService.FindStationByLocationAsync(request);
                
                if (stationAssignment != null)
                {
                    // Find and select the station in the combo box
                    foreach (ComboBoxItem item in StationCombo.Items)
                    {
                        if (item.Tag is Station station && station.Id == stationAssignment.StationId)
                        {
                            StationCombo.SelectedItem = item;
                            StationAssignmentStatus.Text = $"✓ Auto-assigned to {stationAssignment.StationName} " +
                                                         $"({stationAssignment.AssignmentMethod}, {stationAssignment.Distance:F0}m)";
                            StationAssignmentStatus.Visibility = Visibility.Visible;
                            break;
                        }
                    }
                }
                else
                {
                    StationAssignmentStatus.Text = "⚠ No station found for this location";
                    StationAssignmentStatus.Foreground = System.Windows.Media.Brushes.Orange;
                    StationAssignmentStatus.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                StationAssignmentStatus.Text = $"⚠ Station assignment failed: {ex.Message}";
                StationAssignmentStatus.Foreground = System.Windows.Media.Brushes.Red;
                StationAssignmentStatus.Visibility = Visibility.Visible;
            }
            finally
            {
                _isLoadingStations = false;
            }
        }

        private string GetAgencyTypeString(string agencyName)
        {
            var lowerAgency = agencyName.ToLower();
            
            if (lowerAgency.Contains("fire"))
                return "fire";
            else if (lowerAgency.Contains("coast"))
                return "coastguard";
            else if (lowerAgency.Contains("police"))
                return "police";
            else if (lowerAgency.Contains("ekab"))
                return "hospital";
            
            return "fire"; // default
        }

        private void UpdateMapPreview()
        {
            // TODO: Update the actual map preview
            // For now, just update the coordinates display
        }

        private void AddressSearchControl_AddressSelected(object sender, AddressSelectedEventArgs e)
        {
            try
            {
                var result = e.SelectedAddress;
                if (result == null) return;

                // Update all address fields
                AddressTextBox.Text = result.DisplayName;
                
                // Parse and update individual address components
                if (!string.IsNullOrEmpty(result.Street))
                    StreetTextBox.Text = result.Street;
                
                if (!string.IsNullOrEmpty(result.HouseNumber))
                    StreetNumberTextBox.Text = result.HouseNumber;
                
                if (!string.IsNullOrEmpty(result.City))
                    CityTextBox.Text = result.City;
                
                if (!string.IsNullOrEmpty(result.PostalCode))
                    PostalCodeTextBox.Text = result.PostalCode;
                
                if (!string.IsNullOrEmpty(result.Region))
                    RegionTextBox.Text = result.Region;
                
                if (!string.IsNullOrEmpty(result.Country))
                    CountryTextBox.Text = result.Country;

                // Update coordinates
                LatitudeTextBox.Text = result.Latitude.ToString("F6");
                LongitudeTextBox.Text = result.Longitude.ToString("F6");

                UpdateMapPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating address fields: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetLocationOnMap_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Focus on the map and show instructions
                MessageBox.Show("Click on the map below to select a location for this incident.", 
                    "Select Location", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to activate map selector: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void LocationMapPanel_LocationSelected(object sender, Point location)
        {
            try
            {
                // Update coordinates
                LatitudeTextBox.Text = location.X.ToString("F6");
                LongitudeTextBox.Text = location.Y.ToString("F6");
                
                // Try to reverse geocode the location to get address
                if (_geocodingService != null)
                {
                    var result = await _geocodingService.ReverseGeocodeAsync(location.X, location.Y);
                    if (result != null)
                    {
                        AddressTextBox.Text = result.DisplayName;
                        AddressSearchControl.SetSearchText(result.DisplayName);
                        
                        // Update individual address fields if available
                        if (!string.IsNullOrEmpty(result.Street))
                            StreetTextBox.Text = result.Street;
                        
                        if (!string.IsNullOrEmpty(result.HouseNumber))
                            StreetNumberTextBox.Text = result.HouseNumber;
                        
                        if (!string.IsNullOrEmpty(result.City))
                            CityTextBox.Text = result.City;
                        
                        if (!string.IsNullOrEmpty(result.PostalCode))
                            PostalCodeTextBox.Text = result.PostalCode;
                        
                        if (!string.IsNullOrEmpty(result.Region))
                            RegionTextBox.Text = result.Region;
                        
                        if (!string.IsNullOrEmpty(result.Country))
                            CountryTextBox.Text = result.Country;
                    }
                }
                
                UpdateMapPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to process selected location: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;

            try
            {
                ShowStatus("Creating incident...", true);
                CreateButton.IsEnabled = false;

                var user = _authService.GetCurrentUser();
                if (user == null)
                {
                    MessageBox.Show("User session expired. Please log in again.", "Authentication Error", 
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var incident = CreateIncidentFromForm(user);
                CreatedIncident = await _apiService.CreateIncidentAsync(incident);

                MessageBox.Show("Incident created successfully!", "Success", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
                
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create incident: {ex.Message}", "Error", 
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                HideStatus();
                CreateButton.IsEnabled = true;
            }
        }

        private bool ValidateForm()
        {
            var errors = new List<string>();

            // Validate main category
            if (MainCategoryCombo.SelectedIndex <= 0)
                errors.Add("Please select a main category");

            // Validate sub category
            if (SubCategoryCombo.SelectedIndex <= 0)
                errors.Add("Please select a sub category");

            // Validate coordinates
            if (!double.TryParse(LatitudeTextBox.Text, out var lat) || lat < -90 || lat > 90)
                errors.Add("Please enter a valid latitude (-90 to 90)");

            if (!double.TryParse(LongitudeTextBox.Text, out var lng) || lng < -180 || lng > 180)
                errors.Add("Please enter a valid longitude (-180 to 180)");

            // Validate station
            if (StationCombo.SelectedIndex <= 0)
                errors.Add("Please select a station");

            if (errors.Any())
            {
                var message = "Please fix the following issues:\n\n" + string.Join("\n", errors);
                MessageBox.Show(message, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private CreateIncident CreateIncidentFromForm(User user)
        {
            var mainCategory = (MainCategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            var subCategory = (SubCategoryCombo.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";
            var priority = (IncidentPriority)(int.Parse((PriorityCombo.SelectedItem as ComboBoxItem)?.Tag.ToString() ?? "3"));
            var station = (StationCombo.SelectedItem as ComboBoxItem)?.Tag as Station;
            
            double.TryParse(LatitudeTextBox.Text, out var latitude);
            double.TryParse(LongitudeTextBox.Text, out var longitude);

            var incident = new CreateIncident
            {
                StationId = station?.Id ?? 0,
                MainCategory = mainCategory,
                SubCategory = subCategory,
                Address = AddressTextBox.Text,
                Street = StreetTextBox.Text,
                StreetNumber = StreetNumberTextBox.Text,
                City = CityTextBox.Text,
                Region = RegionTextBox.Text,
                PostalCode = PostalCodeTextBox.Text,
                Country = CountryTextBox.Text,
                Latitude = latitude,
                Longitude = longitude,
                Priority = priority,
                Notes = NotesTextBox.Text,
                CreatedByUserId = user.Id
            };

            // Add caller information if provided
            if (!string.IsNullOrWhiteSpace(CallerNameTextBox.Text) || 
                !string.IsNullOrWhiteSpace(CallerPhoneTextBox.Text))
            {
                incident.Callers = new List<CallerDto>
                {
                    new CallerDto
                    {
                        Name = CallerNameTextBox.Text,
                        PhoneNumber = CallerPhoneTextBox.Text,
                        CalledAt = DateTime.UtcNow,
                        Notes = CallerNotesTextBox.Text
                    }
                };
            }

            return incident;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void ShowStatus(string message, bool isLoading)
        {
            StatusText.Text = message;
            StatusPanel.Visibility = Visibility.Visible;
            
            if (isLoading)
            {
                var rotationAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
                {
                    RepeatBehavior = RepeatBehavior.Forever
                };
                StatusRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
            }
        }

        private void HideStatus()
        {
            StatusPanel.Visibility = Visibility.Collapsed;
            StatusRotation.BeginAnimation(RotateTransform.AngleProperty, null);
        }
    }
}
