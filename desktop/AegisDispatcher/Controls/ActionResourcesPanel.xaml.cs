using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;

namespace AegisDispatcher.Controls
{
    public class AssignedResourceInfo
    {
        public int IncidentId { get; set; }
        public string IncidentTitle { get; set; } = "";
        public object Resource { get; set; } = null!;
        public ResourceType ResourceType { get; set; }
        public DateTime AssignedAt { get; set; }
        public string Status { get; set; } = "Notified";
        public int AssignmentId { get; set; }
    }

    public partial class ActionResourcesPanel : UserControl, INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;
        private readonly List<Station> _stations = new();
        private readonly Dictionary<int, List<Vehicle>> _stationVehicles = new();
        private readonly Dictionary<int, List<Personnel>> _stationPersonnel = new();
        private readonly Dictionary<int, bool> _expandedStations = new();
        private object? _selectedResource;
        private readonly List<object> _selectedResources = new();
        private Incident? _selectedIncident;
        private string _searchText = "";
        private bool _isSearchFocused = false;
        private bool _showingAssignedResources = false;
        private readonly List<AssignedResourceInfo> _assignedResources = new();
        private readonly Dictionary<(ResourceType, int), AssignedResourceInfo> _currentAssignments = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<object>? ResourceSelected;
        public event EventHandler<string>? StatusChanged;

        public object? SelectedResource
        {
            get => _selectedResource;
            set
            {
                _selectedResource = value;
                OnPropertyChanged(nameof(SelectedResource));
                UpdateSelectedResourceDisplay();
                ResourceSelected?.Invoke(this, value);
            }
        }

        public Incident? SelectedIncident
        {
            get => _selectedIncident;
            set
            {
                Console.WriteLine($"ActionResourcesPanel: SelectedIncident set to {value?.Id} (StationId: {value?.StationId})");
                _selectedIncident = value;
                OnPropertyChanged(nameof(SelectedIncident));
                UpdateAssignmentButtons();

                // Auto-expand assigned station if incident is selected
                if (value != null)
                {
                    ExpandAssignedStation(value);
                }
            }
        }

        public ActionResourcesPanel()
        {
            InitializeComponent();

            _apiService = App.ServiceProvider?.GetService<IApiService>()
                ?? throw new InvalidOperationException("API service not available");
            _authService = App.ServiceProvider?.GetService<IAuthService>()
                ?? throw new InvalidOperationException("Auth service not available");

            InitializePanel();
            StartLoadingAnimation();

            // Defer async loading until after the control is fully loaded
            Loaded += OnControlLoaded;
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnControlLoaded; // Remove the event handler to prevent multiple calls
            await LoadResourcesAsync();
        }

        private void InitializePanel()
        {
            // Initialize search box placeholder
            SearchBox.Text = "Search stations or units...";
            SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF999999"));
        }

        private void StartLoadingAnimation()
        {
            var rotationAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoadingRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }

        public async Task LoadResourcesAsync()
        {
            try
            {
                ShowLoading(true);
                HideError();

                // Load stations
                var stations = await _apiService.GetStationsAsync();
                _stations.Clear();
                _stations.AddRange(stations);

                // Load vehicles for all stations
                var allVehicles = await _apiService.GetVehiclesAsync(null);
                _stationVehicles.Clear();

                foreach (var vehicle in allVehicles)
                {
                    if (!_stationVehicles.ContainsKey(vehicle.StationId))
                        _stationVehicles[vehicle.StationId] = new List<Vehicle>();
                    _stationVehicles[vehicle.StationId].Add(vehicle);
                }

                // Load personnel for all stations (for search functionality)
                var allPersonnel = await _apiService.GetPersonnelAsync(null);
                _stationPersonnel.Clear();

                foreach (var person in allPersonnel)
                {
                    if (!_stationPersonnel.ContainsKey(person.StationId))
                        _stationPersonnel[person.StationId] = new List<Personnel>();
                    _stationPersonnel[person.StationId].Add(person);
                }

                // Load current assignments to check availability
                await LoadCurrentAssignmentsAsync();

                UpdateResourceDisplay();
                StatusChanged?.Invoke(this, "Resources loaded successfully");
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load resources: {ex.Message}");
                StatusChanged?.Invoke(this, $"Error loading resources: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void UpdateResourceDisplay()
        {
            // Clear existing content
            var dynamicElements = ResourcesContainer.Children
                .OfType<FrameworkElement>()
                .Where(e => e.Tag?.ToString() == "Dynamic")
                .ToList();

            foreach (var element in dynamicElements)
            {
                ResourcesContainer.Children.Remove(element);
            }

            var hasResults = false;

            if (string.IsNullOrEmpty(_searchText) || _searchText == "Search stations or units...")
            {
                // Show stations with collapsed view
                foreach (var station in _stations.OrderBy(s => s.Name))
                {
                    if (_stationVehicles.ContainsKey(station.Id) && _stationVehicles[station.Id].Any())
                    {
                        var stationHeader = CreateStationHeader(station);
                        ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, stationHeader);
                        hasResults = true;

                        // Show resources if station is expanded
                        if (_expandedStations.ContainsKey(station.Id) && _expandedStations[station.Id])
                        {
                            var resourcesGrid = CreateResourcesGrid(station.Id);
                            ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, resourcesGrid);
                        }
                    }
                }
            }
            else
            {
                // Show search results
                hasResults = ShowSearchResults();
            }

            // Show/hide no resources message
            NoResourcesMessage.Visibility = hasResults ? Visibility.Collapsed : Visibility.Visible;

            // Restore visual selection state
            RestoreResourceSelections();
        }

        private bool ShowSearchResults()
        {
            var hasResults = false;
            var searchLower = _searchText.ToLower();

            // Search in stations
            foreach (var station in _stations.Where(s => s.Name.ToLower().Contains(searchLower)))
            {
                if (_stationVehicles.ContainsKey(station.Id) && _stationVehicles[station.Id].Any())
                {
                    var stationHeader = CreateStationHeader(station, true); // Always expanded in search
                    ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, stationHeader);

                    var resourcesGrid = CreateResourcesGrid(station.Id);
                    ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, resourcesGrid);
                    hasResults = true;
                }
            }

            // Search in vehicles
            foreach (var stationVehicles in _stationVehicles)
            {
                var matchingVehicles = stationVehicles.Value
                    .Where(v => v.Callsign.ToLower().Contains(searchLower) ||
                               v.Type.ToLower().Contains(searchLower))
                    .ToList();

                if (matchingVehicles.Any())
                {
                    var station = _stations.FirstOrDefault(s => s.Id == stationVehicles.Key);
                    if (station != null)
                    {
                        var stationHeader = CreateStationHeader(station, true);
                        ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, stationHeader);

                        var resourcesGrid = CreateResourcesGrid(stationVehicles.Key, matchingVehicles);
                        ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, resourcesGrid);
                        hasResults = true;
                    }
                }
            }

            // Search in personnel (only show in search results)
            foreach (var stationPersonnel in _stationPersonnel)
            {
                var matchingPersonnel = stationPersonnel.Value
                    .Where(p => p.Name.ToLower().Contains(searchLower) ||
                               p.Rank.ToLower().Contains(searchLower) ||
                               (p.BadgeNumber?.ToLower().Contains(searchLower) ?? false))
                    .ToList();

                if (matchingPersonnel.Any())
                {
                    var station = _stations.FirstOrDefault(s => s.Id == stationPersonnel.Key);
                    if (station != null)
                    {
                        var stationHeader = CreateStationHeader(station, true);
                        ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, stationHeader);

                        var personnelGrid = CreatePersonnelGrid(matchingPersonnel);
                        ResourcesContainer.Children.Insert(ResourcesContainer.Children.Count - 3, personnelGrid);
                        hasResults = true;
                    }
                }
            }

            return hasResults;
        }

        private Border CreateStationHeader(Station station, bool forceExpanded = false)
        {
            var header = new Border
            {
                Style = (Style)Resources["StationHeader"],
                Tag = "Dynamic"
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Expand/Collapse button (only if not in search mode)
            if (!forceExpanded)
            {
                var isExpanded = _expandedStations.ContainsKey(station.Id) && _expandedStations[station.Id];
                var expandButton = new Button
                {
                    Content = isExpanded ? "▼" : "▶",
                    Style = (Style)Resources["ExpandButton"],
                    Tag = station.Id
                };
                expandButton.Click += ExpandButton_Click;
                grid.Children.Add(expandButton);
            }

            var nameBlock = new TextBlock
            {
                Text = station.Name,
                Style = (Style)Resources["StationName"],
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(nameBlock, 1);
            grid.Children.Add(nameBlock);

            var vehicleCount = _stationVehicles.ContainsKey(station.Id) ? _stationVehicles[station.Id].Count : 0;
            var countBlock = new TextBlock
            {
                Text = $"{vehicleCount} unit{(vehicleCount != 1 ? "s" : "")}",
                Style = (Style)Resources["ResourceCount"],
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(countBlock, 2);
            grid.Children.Add(countBlock);

            header.Child = grid;

            // Click handler for the entire header (only if not in search mode)
            if (!forceExpanded)
            {
                header.MouseLeftButtonUp += (s, e) => ToggleStationExpansion(station.Id);
            }

            return header;
        }

        private Grid CreateResourcesGrid(int stationId, List<Vehicle>? specificVehicles = null)
        {
            var grid = new Grid
            {
                Tag = "Dynamic",
                Margin = new Thickness(0, 5, 0, 10)
            };

            var vehicles = specificVehicles ?? (_stationVehicles.ContainsKey(stationId) ? _stationVehicles[stationId] : new List<Vehicle>());

            if (!vehicles.Any()) return grid;

            // Calculate columns based on available width (assume 2-3 per row)
            var columnsPerRow = Math.Max(2, Math.Min(3, (int)(ActualWidth / 120))); // Adjust based on card width
            if (columnsPerRow == 0) columnsPerRow = 2; // Fallback

            var rows = (int)Math.Ceiling((double)vehicles.Count / columnsPerRow);

            // Setup grid structure
            for (int i = 0; i < columnsPerRow; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            // Add vehicle cards
            for (int i = 0; i < vehicles.Count; i++)
            {
                var vehicle = vehicles[i];
                var card = CreateVehicleCard(vehicle);

                var row = i / columnsPerRow;
                var col = i % columnsPerRow;

                Grid.SetRow(card, row);
                Grid.SetColumn(card, col);
                grid.Children.Add(card);
            }

            return grid;
        }

        private Grid CreatePersonnelGrid(List<Personnel> personnel)
        {
            var grid = new Grid
            {
                Tag = "Dynamic",
                Margin = new Thickness(0, 5, 0, 10)
            };

            if (!personnel.Any()) return grid;

            var columnsPerRow = Math.Max(2, Math.Min(3, (int)(ActualWidth / 120)));
            if (columnsPerRow == 0) columnsPerRow = 2;

            var rows = (int)Math.Ceiling((double)personnel.Count / columnsPerRow);

            for (int i = 0; i < columnsPerRow; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            }
            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }

            for (int i = 0; i < personnel.Count; i++)
            {
                var person = personnel[i];
                var card = CreatePersonnelCard(person);

                var row = i / columnsPerRow;
                var col = i % columnsPerRow;

                Grid.SetRow(card, row);
                Grid.SetColumn(card, col);
                grid.Children.Add(card);
            }

            return grid;
        }

        private Border CreateVehicleCard(Vehicle vehicle)
        {
            // Check if vehicle is currently assigned
            var assignmentKey = (ResourceType.Vehicle, vehicle.Id);
            var isAssigned = _currentAssignments.ContainsKey(assignmentKey);
            var assignment = isAssigned ? _currentAssignments[assignmentKey] : null;

            var card = new Border
            {
                Style = (Style)Resources["ResourceCard"],
                Tag = "Dynamic"
            };

            // Set card appearance based on assignment status
            if (isAssigned)
            {
                card.Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)); // Light yellow background
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Orange border
                card.BorderThickness = new Thickness(2);
                card.Opacity = 0.8;
            }

            var content = new StackPanel();

            // Status and title
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 2)
            };

            titlePanel.Children.Add(new Ellipse
            {
                Style = (Style)Resources["StatusIndicator"],
                Fill = isAssigned ? new SolidColorBrush(Color.FromRgb(245, 158, 11)) : GetVehicleStatusBrush(vehicle.Status)
            });

            var callsignTextBlock = new TextBlock
            {
                Text = vehicle.Callsign,
                Style = (Style)Resources["ResourceTitle"]
            };

            // Set black foreground if vehicle is assigned
            if (isAssigned)
            {
                callsignTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            }

            titlePanel.Children.Add(callsignTextBlock);

            content.Children.Add(titlePanel);

            // Vehicle type
            var vehicleTypeTextBlock = new TextBlock
            {
                Text = vehicle.Type,
                Style = (Style)Resources["ResourceDetail"],
                Margin = new Thickness(0, 0, 0, 1)
            };

            // Set black foreground if vehicle is assigned
            if (isAssigned)
            {
                vehicleTypeTextBlock.Foreground = new SolidColorBrush(Colors.Black);
            }

            content.Children.Add(vehicleTypeTextBlock);

            // Status - show assignment or vehicle status
            if (isAssigned)
            {
                content.Children.Add(new TextBlock
                {
                    Text = $"Assigned to Incident #{assignment!.IncidentId}",
                    Style = (Style)Resources["ResourceDetail"],
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 83, 9)), // Dark orange
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 1)
                });

                content.Children.Add(new TextBlock
                {
                    Text = $"Status: {assignment.Status}",
                    Style = (Style)Resources["ResourceDetail"],
                    Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15)) // Darker orange
                });
            }
            else
            {
                content.Children.Add(new TextBlock
                {
                    Text = GetVehicleStatusText(vehicle.Status),
                    Style = (Style)Resources["ResourceDetail"]
                });
            }

            card.Child = content;

            // Click handler for selection - only allow if not assigned
            if (!isAssigned)
            {
                card.MouseLeftButtonUp += (s, e) =>
                {
                    SelectResource(vehicle, card);
                };
                card.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                card.Cursor = System.Windows.Input.Cursors.No;
                card.ToolTip = $"This vehicle is assigned to Incident #{assignment!.IncidentId} and cannot be reassigned";
            }

            return card;
        }

        private Border CreatePersonnelCard(Personnel person)
        {
            // Check if personnel is currently assigned
            var assignmentKey = (ResourceType.Personnel, person.Id);
            var isAssigned = _currentAssignments.ContainsKey(assignmentKey);
            var assignment = isAssigned ? _currentAssignments[assignmentKey] : null;

            var card = new Border
            {
                Style = (Style)Resources["ResourceCard"],
                Tag = "Dynamic"
            };

            // Set card appearance based on assignment status
            if (isAssigned)
            {
                card.Background = new SolidColorBrush(Color.FromRgb(254, 243, 199)); // Light yellow background
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Orange border
                card.BorderThickness = new Thickness(2);
                card.Opacity = 0.8;
            }

            var content = new StackPanel();

            // Status and title
            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 2)
            };

            titlePanel.Children.Add(new Ellipse
            {
                Style = (Style)Resources["StatusIndicator"],
                Fill = isAssigned
                    ? new SolidColorBrush(Color.FromRgb(245, 158, 11))
                    : (person.IsActive ? Brushes.LimeGreen : Brushes.Gray)
            });

            titlePanel.Children.Add(new TextBlock
            {
                Text = person.Name,
                Style = (Style)Resources["ResourceTitle"]
            });

            content.Children.Add(titlePanel);

            content.Children.Add(new TextBlock
            {
                Text = person.Rank,
                Style = (Style)Resources["ResourceDetail"],
                Margin = new Thickness(0, 0, 0, 1)
            });

            // Show assignment status or badge
            if (isAssigned)
            {
                content.Children.Add(new TextBlock
                {
                    Text = $"Assigned to Incident #{assignment!.IncidentId}",
                    Style = (Style)Resources["ResourceDetail"],
                    Foreground = new SolidColorBrush(Color.FromRgb(180, 83, 9)), // Dark orange
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, 0, 0, 1)
                });

                content.Children.Add(new TextBlock
                {
                    Text = $"Status: {assignment.Status}",
                    Style = (Style)Resources["ResourceDetail"],
                    Foreground = new SolidColorBrush(Color.FromRgb(120, 53, 15)) // Darker orange
                });
            }
            else if (!string.IsNullOrEmpty(person.BadgeNumber))
            {
                content.Children.Add(new TextBlock
                {
                    Text = $"Badge: {person.BadgeNumber}",
                    Style = (Style)Resources["ResourceDetail"]
                });
            }

            card.Child = content;

            // Click handler for selection - only allow if not assigned
            if (!isAssigned)
            {
                card.MouseLeftButtonUp += (s, e) =>
                {
                    SelectResource(person, card);
                };
                card.Cursor = System.Windows.Input.Cursors.Hand;
            }
            else
            {
                card.Cursor = System.Windows.Input.Cursors.No;
                card.ToolTip = $"This person is assigned to Incident #{assignment!.IncidentId} and cannot be reassigned";
            }

            return card;
        }

        private void SelectResource(object resource, Border selectedCard)
        {
            // Check if resource is already selected
            var isAlreadySelected = _selectedResources.Contains(resource);

            if (isAlreadySelected)
            {
                // Deselect the resource
                _selectedResources.Remove(resource);

                // Reset card appearance
                selectedCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF626262"));
                selectedCard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C3C"));
            }
            else
            {
                // Select the resource
                _selectedResources.Add(resource);

                // Highlight selected card
                selectedCard.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
                selectedCard.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F"));
            }

            // Update the single selected resource for backward compatibility
            SelectedResource = _selectedResources.LastOrDefault();

            UpdateSelectedResourceDisplay();
        }

        private void ClearResourceSelections()
        {
            // Clear the selected resources list
            _selectedResources.Clear();
            SelectedResource = null;

            // Find all resource cards and reset their appearance
            foreach (var element in ResourcesContainer.Children.OfType<FrameworkElement>())
            {
                if (element is Border border && border.Tag?.ToString() == "Dynamic")
                {
                    border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF626262"));
                    border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C3C"));
                }
                else if (element is Grid grid)
                {
                    foreach (var child in grid.Children.OfType<Border>())
                    {
                        child.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF626262"));
                        child.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C3C"));
                    }
                }
            }

            UpdateSelectedResourceDisplay();
        }

        private void RestoreResourceSelections()
        {
            if (!_selectedResources.Any()) return;

            // Find and highlight selected resource cards
            foreach (var element in ResourcesContainer.Children.OfType<FrameworkElement>())
            {
                if (element is Border border && border.Tag?.ToString() == "Dynamic")
                {
                    // Check if this card represents a selected resource
                    var cardContent = border.Child as StackPanel;
                    if (cardContent != null && IsResourceSelected(border))
                    {
                        border.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
                        border.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F"));
                    }
                }
                else if (element is Grid grid)
                {
                    foreach (var child in grid.Children.OfType<Border>())
                    {
                        if (IsResourceSelected(child))
                        {
                            child.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
                            child.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF1E3A5F"));
                        }
                    }
                }
            }
        }

        private bool IsResourceSelected(Border card)
        {
            // This is a simplified check - in a real implementation you might need
            // to store resource IDs and match them more precisely
            var cardContent = card.Child as StackPanel;
            if (cardContent?.Children.Count > 0)
            {
                var titlePanel = cardContent.Children[0] as StackPanel;
                if (titlePanel?.Children.Count > 1)
                {
                    var titleBlock = titlePanel.Children[1] as TextBlock;
                    if (titleBlock != null)
                    {
                        var cardTitle = titleBlock.Text;
                        return _selectedResources.Any(r => GetResourceName(r) == cardTitle);
                    }
                }
            }
            return false;
        }

        private void ToggleStationExpansion(int stationId)
        {
            _expandedStations[stationId] = !(_expandedStations.ContainsKey(stationId) && _expandedStations[stationId]);
            UpdateResourceDisplay();
        }

        private void ExpandAssignedStation(Incident incident)
        {
            Console.WriteLine($"ActionResourcesPanel: ExpandAssignedStation called for incident {incident.Id}, StationId: {incident.StationId}");
            Console.WriteLine($"ActionResourcesPanel: Available stations: {string.Join(", ", _stations.Select(s => $"{s.Id}:{s.Name}"))}");
            
            // Expand the station responsible for this incident
            if (incident.StationId > 0 && _stations.Any(s => s.Id == incident.StationId))
            {
                _expandedStations[incident.StationId] = true;
                Console.WriteLine($"ActionResourcesPanel: Expanded station {incident.StationId} for incident #{incident.Id}");
                StatusChanged?.Invoke(this, $"Expanded station for incident #{incident.Id}");
            }
            else
            {
                Console.WriteLine($"ActionResourcesPanel: Could not find station {incident.StationId} for incident #{incident.Id}");
            }

            // Also expand stations that have resources assigned to this incident
            foreach (var assignment in incident.Assignments)
            {
                // Find the station for the assigned resource
                var assignedStationId = 0;

                if (assignment.ResourceType == ResourceType.Vehicle)
                {
                    // Find vehicle's station
                    foreach (var stationVehicles in _stationVehicles)
                    {
                        if (stationVehicles.Value.Any(v => v.Id == assignment.ResourceId))
                        {
                            assignedStationId = stationVehicles.Key;
                            break;
                        }
                    }
                }
                else if (assignment.ResourceType == ResourceType.Personnel)
                {
                    // Find personnel's station
                    foreach (var stationPersonnel in _stationPersonnel)
                    {
                        if (stationPersonnel.Value.Any(p => p.Id == assignment.ResourceId))
                        {
                            assignedStationId = stationPersonnel.Key;
                            break;
                        }
                    }
                }

                if (assignedStationId > 0 && _stations.Any(s => s.Id == assignedStationId))
                {
                    _expandedStations[assignedStationId] = true;
                    Console.WriteLine($"ActionResourcesPanel: Expanded assigned station {assignedStationId} for incident #{incident.Id}");
                }
            }

            Console.WriteLine($"ActionResourcesPanel: Expanded stations: {string.Join(", ", _expandedStations.Where(kvp => kvp.Value).Select(kvp => kvp.Key))}");
            UpdateResourceDisplay();
        }

        private void UpdateSelectedResourceDisplay()
        {
            if (_selectedResources.Any())
            {
                SelectedResourcePanel.Visibility = Visibility.Visible;

                var resourceCount = _selectedResources.Count;
                if (resourceCount == 1)
                {
                    var resource = _selectedResources.First();
                    if (resource is Vehicle vehicle)
                    {
                        SelectedResourceText.Text = $"🚒 {vehicle.Callsign}";
                        SelectedResourceDetail.Text = $"{vehicle.Type} • {GetVehicleStatusText(vehicle.Status)}";
                    }
                    else if (resource is Personnel person)
                    {
                        SelectedResourceText.Text = $"👤 {person.Name}";
                        SelectedResourceDetail.Text = $"{person.Rank} • {person.AgencyName}";
                    }
                }
                else
                {
                    // Multiple resources selected
                    var vehicleCount = _selectedResources.OfType<Vehicle>().Count();
                    var personnelCount = _selectedResources.OfType<Personnel>().Count();

                    var parts = new List<string>();
                    if (vehicleCount > 0) parts.Add($"{vehicleCount} vehicle{(vehicleCount != 1 ? "s" : "")}");
                    if (personnelCount > 0) parts.Add($"{personnelCount} personnel");

                    SelectedResourceText.Text = $"📋 {resourceCount} Resources Selected";
                    SelectedResourceDetail.Text = string.Join(", ", parts);
                }

                // Update button text for multiple assignments
                AssignToIncidentBtn.Content = resourceCount == 1 ? "Assign" : $"Assign All ({resourceCount})";
                UpdateAssignmentButtons();
            }
            else
            {
                SelectedResourcePanel.Visibility = Visibility.Collapsed;
                AssignToIncidentBtn.Content = "Assign";
                UpdateAssignmentButtons();
            }
        }

        private void UpdateAssignmentButtons()
        {
            var hasResources = _selectedResources.Any();
            var hasIncident = _selectedIncident != null;

            AssignToIncidentBtn.IsEnabled = hasResources && hasIncident;
        }

        private void ExpandButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int stationId)
            {
                ToggleStationExpansion(stationId);
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!_isSearchFocused && SearchBox.Text == "Search stations or units...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = Brushes.White;
                _isSearchFocused = true;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Search stations or units...";
                SearchBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF999999"));
                _isSearchFocused = false;
                _searchText = "";
                UpdateResourceDisplay();
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isSearchFocused)
            {
                _searchText = SearchBox.Text;
                UpdateResourceDisplay();
            }
        }

        private string GetResourceName(object resource)
        {
            return resource switch
            {
                Vehicle v => v.Callsign,
                Personnel p => p.Name,
                _ => "Unknown"
            };
        }

        private SolidColorBrush GetVehicleStatusBrush(VehicleStatus status)
        {
            return status switch
            {
                VehicleStatus.Available => Brushes.LimeGreen,
                VehicleStatus.Notified => Brushes.Yellow,
                VehicleStatus.EnRoute => Brushes.Orange,
                VehicleStatus.OnScene => Brushes.Red,
                VehicleStatus.Busy => Brushes.Purple,
                VehicleStatus.Maintenance => Brushes.Gray,
                VehicleStatus.Offline => Brushes.DarkRed,
                _ => Brushes.Gray
            };
        }

        private string GetVehicleStatusText(VehicleStatus status)
        {
            return status switch
            {
                VehicleStatus.Available => "Available",
                VehicleStatus.Notified => "Notified",
                VehicleStatus.EnRoute => "En Route",
                VehicleStatus.OnScene => "On Scene",
                VehicleStatus.Busy => "Busy",
                VehicleStatus.Maintenance => "Maintenance",
                VehicleStatus.Offline => "Offline",
                _ => "Unknown"
            };
        }

        private void ShowLoading(bool show)
        {
            if (LoadingIndicator != null)
            {
                LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void ShowError(string message)
        {
            if (ErrorText != null && ErrorMessage != null && NoResourcesMessage != null)
            {
                ErrorText.Text = message;
                ErrorMessage.Visibility = Visibility.Visible;
                NoResourcesMessage.Visibility = Visibility.Collapsed;
            }
        }

        private void HideError()
        {
            if (ErrorMessage != null)
            {
                ErrorMessage.Visibility = Visibility.Collapsed;
            }
        }

        #region Event Handlers

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadResourcesAsync();
        }

        private void AvailableResources_Click(object sender, RoutedEventArgs e)
        {
            SwitchToAvailableResources();
        }

        private void AssignedResources_Click(object sender, RoutedEventArgs e)
        {
            SwitchToAssignedResources();
        }

        private void ClearSelection_Click(object sender, RoutedEventArgs e)
        {
            SelectedResource = null;
            ClearResourceSelections();
            StatusChanged?.Invoke(this, "Resource selection cleared");
        }

        private async void AssignToIncident_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedResources.Any() || _selectedIncident == null) return;

            try
            {
                var currentUser = _authService.GetCurrentUser();
                if (currentUser == null)
                {
                    StatusChanged?.Invoke(this, "Error: No authenticated user found");
                    return;
                }

                // Disable the assign button during operation
                AssignToIncidentBtn.IsEnabled = false;

                var resourceCount = _selectedResources.Count;
                StatusChanged?.Invoke(this, $"Assigning {resourceCount} resource{(resourceCount != 1 ? "s" : "")} to incident #{_selectedIncident.Id}...");

                var successCount = 0;
                var failureCount = 0;
                var errorMessages = new List<string>();

                // Assign each selected resource
                foreach (var selectedResource in _selectedResources.ToList()) // ToList to avoid modification during iteration
                {
                    try
                    {
                        var assignment = new CreateAssignment
                        {
                            IncidentId = _selectedIncident.Id,
                            ResourceType = selectedResource is Vehicle ? ResourceType.Vehicle : ResourceType.Personnel,
                            ResourceId = selectedResource is Vehicle v ? v.Id : ((Personnel)selectedResource).Id,
                            AssignedByUserId = currentUser.Id
                        };

                        var success = await _apiService.AssignResourceAsync(assignment);

                        if (success)
                        {
                            successCount++;
                        }
                        else
                        {
                            failureCount++;
                            var resourceName = GetResourceName(selectedResource);
                            errorMessages.Add($"Failed to assign {resourceName}");
                        }
                    }
                    catch (ApiException ex)
                    {
                        failureCount++;
                        var resourceName = GetResourceName(selectedResource);
                        errorMessages.Add($"Assignment failed for {resourceName}: {ex.Message}");
                        Console.WriteLine($"ActionResourcesPanel: API Exception during assignment of {resourceName}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        failureCount++;
                        var resourceName = GetResourceName(selectedResource);
                        errorMessages.Add($"Unexpected error for {resourceName}: {ex.Message}");
                        Console.WriteLine($"ActionResourcesPanel: Unexpected exception during assignment of {resourceName}: {ex}");
                    }
                }

                // Report results
                if (successCount > 0 && failureCount == 0)
                {
                    StatusChanged?.Invoke(this, $"Successfully assigned {successCount} resource{(successCount != 1 ? "s" : "")} to incident #{_selectedIncident.Id}");
                }
                else if (successCount > 0 && failureCount > 0)
                {
                    StatusChanged?.Invoke(this, $"Assigned {successCount} resource{(successCount != 1 ? "s" : "")}, {failureCount} failed. Check console for details.");
                    foreach (var error in errorMessages)
                    {
                        Console.WriteLine($"ActionResourcesPanel: {error}");
                    }
                }
                else
                {
                    StatusChanged?.Invoke(this, $"Failed to assign any resources. Check console for details.");
                    foreach (var error in errorMessages)
                    {
                        Console.WriteLine($"ActionResourcesPanel: {error}");
                    }
                }

                // Clear selection and refresh
                ClearResourceSelections();

                // Reload current assignments to reflect new assignment
                await LoadCurrentAssignmentsAsync();

                // Refresh display to show updated availability
                UpdateResourceDisplay();
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Assignment process failed: {ex.Message}");
                Console.WriteLine($"ActionResourcesPanel: Unexpected exception during assignment process: {ex}");
            }
            finally
            {
                // Re-enable the assign button
                AssignToIncidentBtn.IsEnabled = true;
                UpdateAssignmentButtons();
            }
        }

        #endregion

        #region Tab Switching

        private void SwitchToAvailableResources()
        {
            _showingAssignedResources = false;

            // Update button appearance
            AvailableResourcesBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
            AvailableResourcesBtn.Foreground = Brushes.White;
            AssignedResourcesBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C3C"));
            AssignedResourcesBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC"));

            // Show/hide panels
            AvailableResourcesContainer.Visibility = Visibility.Visible;
            AssignedResourcesContainer.Visibility = Visibility.Collapsed;
            SearchBarPanel.Visibility = Visibility.Visible;

            StatusChanged?.Invoke(this, "Showing available resources");
        }

        private async void SwitchToAssignedResources()
        {
            _showingAssignedResources = true;

            // Update button appearance
            AssignedResourcesBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
            AssignedResourcesBtn.Foreground = Brushes.White;
            AvailableResourcesBtn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3C3C3C"));
            AvailableResourcesBtn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCCCCCC"));

            // Show/hide panels
            AvailableResourcesContainer.Visibility = Visibility.Collapsed;
            AssignedResourcesContainer.Visibility = Visibility.Visible;
            SearchBarPanel.Visibility = Visibility.Collapsed; // No search for assigned resources view

            // Load assigned resources
            await LoadAssignedResourcesAsync();

            StatusChanged?.Invoke(this, "Showing assigned resources");
        }

        private async Task LoadCurrentAssignmentsAsync()
        {
            try
            {
                _currentAssignments.Clear();

                // Load all incidents to get assignments
                var incidents = await _apiService.GetIncidentsAsync();

                foreach (var incident in incidents.Where(i => i.Status != IncidentStatus.Closed && i.Assignments.Any()))
                {
                    foreach (var assignment in incident.Assignments)
                    {
                        // Only track active assignments (not finished/completed)
                        if (assignment.Status != "Finished" && assignment.Status != "Completed")
                        {
                            var assignedResource = new AssignedResourceInfo
                            {
                                IncidentId = incident.Id,
                                IncidentTitle = $"{incident.MainCategory} - {incident.SubCategory}",
                                ResourceType = assignment.ResourceType,
                                AssignedAt = assignment.CreatedAt,
                                Status = assignment.Status ?? "Notified",
                                AssignmentId = assignment.Id
                            };

                            var key = (assignment.ResourceType, assignment.ResourceId);
                            _currentAssignments[key] = assignedResource;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Don't fail the whole resource loading if assignment loading fails
                Console.WriteLine($"Failed to load current assignments: {ex.Message}");
            }
        }

        private async Task LoadAssignedResourcesAsync()
        {
            try
            {
                ShowAssignedLoading(true);

                // Load all incidents to get assignments
                var incidents = await _apiService.GetIncidentsAsync();
                _assignedResources.Clear();

                foreach (var incident in incidents.Where(i => i.Status != IncidentStatus.Closed && i.Assignments.Any()))
                {
                    foreach (var assignment in incident.Assignments)
                    {
                        // Skip finished or completed assignments
                        var assignmentStatus = assignment.Status ?? "Notified";
                        if (assignmentStatus == "Finished" || assignmentStatus == "Completed")
                        {
                            continue;
                        }

                        object? resource = null;

                        if (assignment.ResourceType == ResourceType.Vehicle)
                        {
                            // Find the vehicle in our loaded data
                            foreach (var stationVehicles in _stationVehicles.Values)
                            {
                                resource = stationVehicles.FirstOrDefault(v => v.Id == assignment.ResourceId);
                                if (resource != null) break;
                            }
                        }
                        else if (assignment.ResourceType == ResourceType.Personnel)
                        {
                            // Find the personnel in our loaded data
                            foreach (var stationPersonnel in _stationPersonnel.Values)
                            {
                                resource = stationPersonnel.FirstOrDefault(p => p.Id == assignment.ResourceId);
                                if (resource != null) break;
                            }
                        }

                        if (resource != null)
                        {
                            var assignedResource = new AssignedResourceInfo
                            {
                                IncidentId = incident.Id,
                                IncidentTitle = $"{incident.MainCategory} - {incident.SubCategory}",
                                Resource = resource,
                                ResourceType = assignment.ResourceType,
                                AssignedAt = assignment.CreatedAt,
                                Status = assignmentStatus,
                                AssignmentId = assignment.Id
                            };

                            _assignedResources.Add(assignedResource);
                        }
                    }
                }

                UpdateAssignedResourcesDisplay();
                StatusChanged?.Invoke(this, $"Loaded {_assignedResources.Count} assigned resources");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"Failed to load assigned resources: {ex.Message}");
            }
            finally
            {
                ShowAssignedLoading(false);
            }
        }

        private void UpdateAssignedResourcesDisplay()
        {
            // Clear existing assigned resource cards
            var assignedCards = AssignedResourcesPanel.Children
                .OfType<Border>()
                .Where(b => b.Tag?.ToString() == "AssignedResource")
                .ToList();

            foreach (var card in assignedCards)
            {
                AssignedResourcesPanel.Children.Remove(card);
            }

            if (_assignedResources.Any())
            {
                NoAssignedResourcesMessage.Visibility = Visibility.Collapsed;

                // Group by incident
                var groupedByIncident = _assignedResources.GroupBy(r => r.IncidentId);

                foreach (var incidentGroup in groupedByIncident.OrderBy(g => g.Key))
                {
                    // Create incident header
                    var incidentHeader = CreateIncidentHeader(incidentGroup.First());
                    AssignedResourcesPanel.Children.Insert(AssignedResourcesPanel.Children.Count - 2, incidentHeader);

                    // Create resource cards for this incident
                    foreach (var assignedResource in incidentGroup.OrderBy(r => r.AssignedAt))
                    {
                        var resourceCard = CreateAssignedResourceCard(assignedResource);
                        AssignedResourcesPanel.Children.Insert(AssignedResourcesPanel.Children.Count - 2, resourceCard);
                    }
                }
            }
            else
            {
                NoAssignedResourcesMessage.Visibility = Visibility.Visible;
            }
        }

        private Border CreateIncidentHeader(AssignedResourceInfo assignedResource)
        {
            var header = new Border
            {
                Style = (Style)Resources["StationHeader"],
                Tag = "AssignedResource",
                Margin = new Thickness(0, 10, 0, 5)
            };

            var content = new StackPanel();

            var titleBlock = new TextBlock
            {
                Text = $"📋 Incident #{assignedResource.IncidentId}: {assignedResource.IncidentTitle}",
                Style = (Style)Resources["StationName"],
                Margin = new Thickness(0, 0, 0, 2)
            };

            content.Children.Add(titleBlock);
            header.Child = content;

            return header;
        }

        private Border CreateAssignedResourceCard(AssignedResourceInfo assignedResource)
        {
            var card = new Border
            {
                Style = (Style)Resources["ResourceCard"],
                Tag = "AssignedResource",
                Margin = new Thickness(10, 2, 0, 2)
            };

            var content = new Grid();
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Resource info
            var resourceInfo = new StackPanel();

            var titlePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 2)
            };

            titlePanel.Children.Add(new Ellipse
            {
                Style = (Style)Resources["StatusIndicator"],
                Fill = GetCurrentStatusBrush(assignedResource.Status)
            });

            var resourceName = assignedResource.ResourceType == ResourceType.Vehicle
                ? $"🚒 {((Vehicle)assignedResource.Resource).Callsign}"
                : $"👤 {((Personnel)assignedResource.Resource).Name}";

            titlePanel.Children.Add(new TextBlock
            {
                Text = resourceName,
                Style = (Style)Resources["ResourceTitle"]
            });

            resourceInfo.Children.Add(titlePanel);

            var statusText = new TextBlock
            {
                Text = assignedResource.Status,
                Style = (Style)Resources["ResourceDetail"],
                Margin = new Thickness(0, 0, 0, 2)
            };

            var assignedAtText = new TextBlock
            {
                Text = $"Assigned: {assignedResource.AssignedAt:HH:mm}",
                Style = (Style)Resources["ResourceDetail"]
            };

            resourceInfo.Children.Add(statusText);
            resourceInfo.Children.Add(assignedAtText);

            Grid.SetColumn(resourceInfo, 0);
            content.Children.Add(resourceInfo);

            // Status change buttons
            var statusPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            // Use string-based statuses to match web app exactly
            var statusOptions = assignedResource.ResourceType == ResourceType.Vehicle
                ? new[] { "Notified", "On Scene", "Finished" }
                : new[] { "Notified", "On Scene", "Unavailable" };

            foreach (var status in statusOptions)
            {
                if (status != assignedResource.Status)
                {
                    // Implement assignment logic restrictions
                    bool isAllowed = true;
                    
                    // Only allow notified resources to be set to on scene
                    if (status == "On Scene" && assignedResource.Status != "Notified")
                    {
                        isAllowed = false;
                    }
                    
                    // Prevent on scene resources from being set back to notified
                    if (status == "Notified" && assignedResource.Status == "On Scene")
                    {
                        isAllowed = false;
                    }
                    
                    if (isAllowed)
                    {
                        var statusBtn = new Button
                        {
                            Content = status,
                            Background = GetStatusBrush(status),
                            Foreground = GetStatusForegroundBrush(status),
                            BorderThickness = new Thickness(1),
                            BorderBrush = GetStatusBorderBrush(status),
                            Padding = new Thickness(8, 4, 8, 4),
                            Margin = new Thickness(0, 1, 0, 1),
                            FontSize = 9,
                            FontWeight = FontWeights.SemiBold,
                            Cursor = System.Windows.Input.Cursors.Hand,
                            Tag = $"{assignedResource.IncidentId}:{assignedResource.ResourceType}:{(assignedResource.Resource is Vehicle v ? v.Id : ((Personnel)assignedResource.Resource).Id)}:{status}"
                        };
                        statusBtn.Click += UpdateResourceStatus_Click;
                        statusPanel.Children.Add(statusBtn);
                    }
                }
            }

            Grid.SetColumn(statusPanel, 1);
            content.Children.Add(statusPanel);

            card.Child = content;
            return card;
        }

        private async void UpdateResourceStatus_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagString)
            {
                try
                {
                    var parts = tagString.Split(':');
                    if (parts.Length == 4)
                    {
                        var incidentId = int.Parse(parts[0]);
                        var resourceType = (ResourceType)Enum.Parse(typeof(ResourceType), parts[1]);
                        var resourceId = int.Parse(parts[2]);
                        var newStatus = parts[3];

                        // Find the assigned resource
                        var assignedResource = _assignedResources.FirstOrDefault(ar =>
                            ar.IncidentId == incidentId &&
                            ar.ResourceType == resourceType &&
                            (ar.Resource is Vehicle v ? v.Id : ((Personnel)ar.Resource).Id) == resourceId);

                        if (assignedResource != null)
                        {
                            // Show confirmation dialog for "Finished" status
                            if (newStatus == "Finished")
                            {
                                var resourceName = assignedResource.ResourceType == ResourceType.Vehicle
                                    ? ((Vehicle)assignedResource.Resource).Callsign
                                    : ((Personnel)assignedResource.Resource).Name;

                                var result = MessageBox.Show(
                                    $"Are you sure you want to mark {resourceName} as finished?\n\nThis will remove the resource from the incident and make it available for other assignments.",
                                    "Confirm Finish Resource",
                                    MessageBoxButton.YesNo,
                                    MessageBoxImage.Question,
                                    MessageBoxResult.No);

                                if (result != MessageBoxResult.Yes)
                                {
                                    return; // User cancelled, don't update status
                                }
                            }
                            // Update assignment status via API
                            await UpdateAssignmentStatusAsync(incidentId, assignedResource, newStatus);

                            // Update locally
                            assignedResource.Status = newStatus;

                            // Update current assignments if status changed
                            var key = (assignedResource.ResourceType,
                                assignedResource.ResourceType == ResourceType.Vehicle
                                    ? ((Vehicle)assignedResource.Resource).Id
                                    : ((Personnel)assignedResource.Resource).Id);

                            if (_currentAssignments.ContainsKey(key))
                            {
                                _currentAssignments[key].Status = newStatus;

                                // If status is Finished or Completed, remove from current assignments
                                if (newStatus == "Finished" || newStatus == "Completed")
                                {
                                    _currentAssignments.Remove(key);
                                    // Refresh available resources to show resource as available again
                                    UpdateResourceDisplay();
                                }
                            }

                            // Refresh the assigned resources display with fresh data from API
                            if (_showingAssignedResources)
                            {
                                await LoadAssignedResourcesAsync();
                            }
                            else
                            {
                                UpdateAssignedResourcesDisplay();
                            }

                            var updatedResourceName = assignedResource.ResourceType == ResourceType.Vehicle
                                ? ((Vehicle)assignedResource.Resource).Callsign
                                : ((Personnel)assignedResource.Resource).Name;

                            StatusChanged?.Invoke(this, $"Updated {updatedResourceName} status to {newStatus}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusChanged?.Invoke(this, $"Failed to update status: {ex.Message}");
                }
            }
        }

        private void ShowAssignedLoading(bool show)
        {
            if (AssignedLoadingIndicator != null)
            {
                AssignedLoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;

                if (show)
                {
                    var rotationAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
                    {
                        RepeatBehavior = RepeatBehavior.Forever
                    };
                    AssignedLoadingRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
                }
            }
        }

        #endregion

        #region Status Colors - Better Visibility

        private Brush GetStatusBrush(string status)
        {
            return status switch
            {
                "Notified" => new SolidColorBrush(Color.FromRgb(255, 237, 213)), // Light orange background
                "On Scene" => new SolidColorBrush(Color.FromRgb(219, 234, 254)), // Light blue background
                "Finished" => new SolidColorBrush(Color.FromRgb(220, 252, 231)), // Light green background
                "Unavailable" => new SolidColorBrush(Color.FromRgb(254, 226, 226)), // Light red background
                _ => new SolidColorBrush(Color.FromRgb(243, 244, 246)) // Light gray
            };
        }

        private Brush GetStatusForegroundBrush(string status)
        {
            return status switch
            {
                "Notified" => new SolidColorBrush(Color.FromRgb(154, 52, 18)), // Dark orange text
                "On Scene" => new SolidColorBrush(Color.FromRgb(30, 64, 175)), // Dark blue text
                "Finished" => new SolidColorBrush(Color.FromRgb(20, 83, 45)), // Dark green text
                "Unavailable" => new SolidColorBrush(Color.FromRgb(153, 27, 27)), // Dark red text
                _ => new SolidColorBrush(Color.FromRgb(55, 65, 81)) // Dark gray
            };
        }

        private Brush GetStatusBorderBrush(string status)
        {
            return status switch
            {
                "Notified" => new SolidColorBrush(Color.FromRgb(251, 146, 60)), // Orange border
                "On Scene" => new SolidColorBrush(Color.FromRgb(59, 130, 246)), // Blue border
                "Finished" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // Green border
                "Unavailable" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red border
                _ => new SolidColorBrush(Color.FromRgb(156, 163, 175)) // Gray border
            };
        }

        private Brush GetCurrentStatusBrush(string status)
        {
            return status switch
            {
                "Notified" => new SolidColorBrush(Color.FromRgb(251, 146, 60)), // Orange
                "On Scene" => new SolidColorBrush(Color.FromRgb(59, 130, 246)), // Blue
                "Finished" => new SolidColorBrush(Color.FromRgb(34, 197, 94)), // Green
                "Unavailable" => new SolidColorBrush(Color.FromRgb(239, 68, 68)), // Red
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128)) // Gray
            };
        }

        private async Task UpdateAssignmentStatusAsync(int incidentId, AssignedResourceInfo assignedResource, string newStatus)
        {
            try
            {
                await _apiService.UpdateAssignmentStatusAsync(incidentId, assignedResource.AssignmentId, newStatus);

                // Send SignalR notification to web clients
                await SendAssignmentStatusChangedNotificationAsync(incidentId, assignedResource.AssignmentId,
                    assignedResource.Status, newStatus);
            }
            catch (Exception ex)
            {
                throw; // Let the caller handle the error
            }
        }

        private async Task SendAssignmentStatusChangedNotificationAsync(int incidentId, int assignmentId,
            string oldStatus, string newStatus)
        {
            try
            {
                // Send SignalR notification - this would typically be handled by the backend
                // For now, we'll just log it
                Console.WriteLine($"SignalR: Assignment status changed - Incident: {incidentId}, Assignment: {assignmentId}, {oldStatus} -> {newStatus}");
            }
            catch (Exception ex)
            {
                // Don't fail the status update if SignalR fails
                Console.WriteLine($"Failed to send SignalR notification: {ex.Message}");
            }
        }

        #endregion

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
