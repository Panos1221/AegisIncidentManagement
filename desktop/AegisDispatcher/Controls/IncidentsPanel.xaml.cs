using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;

namespace AegisDispatcher.Controls
{
    public partial class IncidentsPanel : UserControl, INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _loggingService;

        private readonly ObservableCollection<Incident> _incidents = new();
        private readonly ObservableCollection<Incident> _filteredIncidents = new();
        private readonly List<Station> _stations = new();
        private string _searchFilter = string.Empty;
        private IncidentStatus? _statusFilter = null;
        private Incident? _selectedIncident;
        private bool _isRefreshing = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<Incident>? IncidentSelected;

        public ObservableCollection<Incident> Incidents => _incidents;
        public ObservableCollection<Incident> FilteredIncidents => _filteredIncidents;

        public Incident? SelectedIncident
        {
            get => _selectedIncident;
            set
            {
                Console.WriteLine($"IncidentsPanel: SelectedIncident set to {value?.Id} (StationId: {value?.StationId})");
                _selectedIncident = value;
                OnPropertyChanged(nameof(SelectedIncident));
                IncidentSelected?.Invoke(this, value);
            }
        }

        public string SearchFilter
        {
            get => _searchFilter;
            set
            {
                _searchFilter = value;
                OnPropertyChanged(nameof(SearchFilter));
                ApplyFilters();
            }
        }

        public IncidentStatus? StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                OnPropertyChanged(nameof(StatusFilter));
                ApplyFilters();
            }
        }

        public IncidentsPanel()
        {
            InitializeComponent();
            
            // Get services from DI container (this would normally be injected)
            _apiService = App.ServiceProvider?.GetService<IApiService>() 
                ?? throw new InvalidOperationException("API service not available");
            _loggingService = App.ServiceProvider?.GetService<ILoggingService>() 
                ?? throw new InvalidOperationException("Logging service not available");


            StartLoadingAnimation();
            
            // Defer async loading until after the control is fully loaded
            Loaded += OnControlLoaded;
        }

        private async void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnControlLoaded; // Remove the event handler to prevent multiple calls
            await LoadIncidentsAsync();
        }

        public async Task LoadIncidentsAsync()
        {
            // Prevent concurrent refresh operations
            if (_isRefreshing)
            {
                _loggingService.LogInformation("LoadIncidentsAsync skipped - refresh already in progress");
                return;
            }

            try
            {
                _isRefreshing = true;
                _loggingService.LogInformation("LoadIncidentsAsync started");
                ShowLoading(true);
                HideError();

                // Load stations first for station name lookup
                _loggingService.LogInformation("Loading stations...");
                var stations = await _apiService.GetStationsAsync();
                _stations.Clear();
                _stations.AddRange(stations);

                _loggingService.LogInformation("Calling API to get incidents...");
                var incidents = await _apiService.GetIncidentsAsync();
                _loggingService.LogInformation("API returned {Count} incidents", incidents?.Count ?? 0);
                
                _incidents.Clear();
                foreach (var incident in incidents)
                {
                    _incidents.Add(incident);
                }

                _loggingService.LogInformation("Added {Count} incidents to collection", _incidents.Count);
                ApplyFilters();
                _loggingService.LogInformation("Applied filters, filtered incidents: {Count}", _filteredIncidents.Count);
                UpdateUI();
                _loggingService.LogInformation("UI updated successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Failed to load incidents");
                ShowError($"Failed to load incidents: {ex.Message}");
            }
            finally
            {
                _isRefreshing = false;
                ShowLoading(false);
                _loggingService.LogInformation("LoadIncidentsAsync completed");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadIncidentsAsync();
        }

        public void SelectIncidentById(int incidentId)
        {
            var incident = _incidents.FirstOrDefault(i => i.Id == incidentId);
            if (incident != null)
            {
                SelectedIncident = incident;
                UpdateIncidentSelection(incident);
            }
        }

        private void UpdateIncidentSelection(Incident selectedIncident)
        {
            // Update visual selection for all incident cards
            var incidentCards = IncidentsContainer.Children.OfType<Border>().Where(b => b.Tag is Incident).ToList();
            
            foreach (var card in incidentCards)
            {
                var incident = (Incident)card.Tag;
                if (incident.Id == selectedIncident.Id)
                {
                    // Highlight selected card with a blue border
                    card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC"));
                    card.BorderThickness = new Thickness(2);
                    card.Background = GetIncidentCardBackgroundBrush(incident.Status);
                    
                    // Scroll the selected card into view
                    card.BringIntoView();
                }
                else
                {
                    // Reset other cards to default appearance with status-based colors
                    card.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFE5E7EB"));
                    card.BorderThickness = new Thickness(1);
                    card.Background = GetIncidentCardBackgroundBrush(incident.Status);
                }
            }
        }

        private void ApplyFilters()
        {
            _filteredIncidents.Clear();

            var filtered = _incidents.AsEnumerable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(_searchFilter))
            {
                filtered = filtered.Where(i => 
                    i.MainCategory.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) ||
                    i.SubCategory.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) ||
                    (i.Address?.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) == true) ||
                    (i.Notes?.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) == true));
            }

            // Apply status filter
            if (_statusFilter.HasValue)
            {
                filtered = filtered.Where(i => i.Status == _statusFilter.Value);
            }
            else
            {
                // If no specific status filter is applied, exclude Closed incidents from main list
                // Closed incidents should only be visible when explicitly filtered
                filtered = filtered.Where(i => i.Status != IncidentStatus.Closed);
            }

            // Custom ordering: OnGoing, PartialControl, Controlled, FullyControlled
            // Within each status group, order by latest first (CreatedAt descending)
            filtered = filtered.OrderBy(i => GetStatusPriority(i.Status)).ThenByDescending(i => i.CreatedAt);

            foreach (var incident in filtered)
            {
                _filteredIncidents.Add(incident);
            }

            UpdateUI();
        }

        /// <summary>
        /// Gets the priority order for incident statuses.
        /// Lower numbers appear first in the list.
        /// </summary>
        private int GetStatusPriority(IncidentStatus status)
        {
            return status switch
            {
                IncidentStatus.OnGoing => 1,
                IncidentStatus.PartialControl => 2,
                IncidentStatus.Controlled => 3,
                IncidentStatus.FullyControlled => 4,
                IncidentStatus.Created => 5,  // Created incidents appear after active ones
                IncidentStatus.Closed => 6,   // Closed incidents appear last (when filtered)
                _ => 7  // Unknown statuses appear last
            };
        }

        private void UpdateUI()
        {
            if (Dispatcher.CheckAccess())
            {
                UpdateUIInternal();
            }
            else
            {
                Dispatcher.Invoke(UpdateUIInternal);
            }
        }

        private void UpdateUIInternal()
        {
            // Clear existing incident cards
            var incidentCards = IncidentsContainer.Children
                .OfType<Border>()
                .Where(b => b.Tag is Incident)
                .ToList();

            foreach (var card in incidentCards)
            {
                IncidentsContainer.Children.Remove(card);
            }

            // Add incident cards for filtered incidents
            foreach (var incident in _filteredIncidents)
            {
                var card = CreateIncidentCard(incident);
                IncidentsContainer.Children.Insert(IncidentsContainer.Children.Count - 3, card); // Insert before loading/error/no-incidents messages
            }

            // Show appropriate message if no incidents
            if (!_filteredIncidents.Any())
            {
                if (_incidents.Any())
                {
                    // Has incidents but filtered out
                    NoIncidentsMessage.Visibility = Visibility.Visible;
                    var messageBlock = NoIncidentsMessage.Child as StackPanel;
                    if (messageBlock?.Children[1] is TextBlock titleBlock)
                    {
                        titleBlock.Text = "No incidents match current filters";
                    }
                    if (messageBlock?.Children[2] is TextBlock subtitleBlock)
                    {
                        subtitleBlock.Text = "Try adjusting your search or filters";
                    }
                }
                else
                {
                    // No incidents at all
                    NoIncidentsMessage.Visibility = Visibility.Visible;
                    var messageBlock = NoIncidentsMessage.Child as StackPanel;
                    if (messageBlock?.Children[1] is TextBlock titleBlock)
                    {
                        titleBlock.Text = "No active incidents";
                    }
                    if (messageBlock?.Children[2] is TextBlock subtitleBlock)
                    {
                        subtitleBlock.Text = "All clear for now";
                    }
                }
            }
            else
            {
                NoIncidentsMessage.Visibility = Visibility.Collapsed;
            }
        }

        private Border CreateIncidentCard(Incident incident)
        {
            var card = new Border
            {
                Style = (Style)Resources["IncidentCard"],
                Tag = incident,
                Background = GetIncidentCardBackgroundBrush(incident.Status)
            };

            var content = new Grid();
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            content.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Header with status and time
            var header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var statusBadge = new Border
            {
                Style = (Style)Resources["StatusBadge"],
                Background = GetStatusBrush(incident.Status),
                Child = new TextBlock
                {
                    Text = GetStatusText(incident.Status),
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.SemiBold
                }
            };

            // Create a container for time and incident ID
            var timeContainer = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var timeText = new TextBlock
            {
                Style = (Style)Resources["IncidentTime"],
                Text = incident.CreatedAt.ToString("HH:mm"),
                Foreground = GetIncidentCardTextBrush(incident.Status),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            var incidentIdText = new TextBlock
            {
                Text = $"#{incident.Id}",
                FontSize = 9,
                Foreground = GetIncidentCardSecondaryTextBrush(incident.Status),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            timeContainer.Children.Add(timeText);
            timeContainer.Children.Add(incidentIdText);

            header.Children.Add(statusBadge);
            Grid.SetColumn(timeContainer, 1);
            header.Children.Add(timeContainer);

            Grid.SetRow(header, 0);
            content.Children.Add(header);

            // Title
            var title = new TextBlock
            {
                Style = (Style)Resources["IncidentTitle"],
                Text = $"{incident.MainCategory} - {incident.SubCategory}",
                Foreground = GetIncidentCardTextBrush(incident.Status),
                Margin = new Thickness(0, 5, 0, 0)
            };

            Grid.SetRow(title, 1);
            content.Children.Add(title);

            // Location and Station
            var details = new StackPanel { Margin = new Thickness(0, 5, 0, 5) };
            
            var locationText = new TextBlock
            {
                Style = (Style)Resources["IncidentDetail"],
                Text = incident.Address ?? $"{incident.City}, {incident.Region}",
                TextTrimming = TextTrimming.CharacterEllipsis,
                Margin = new Thickness(0, 0, 0, 2),
                Foreground = GetIncidentCardSecondaryTextBrush(incident.Status)
            };

            var stationName = GetStationName(incident.StationId);
            var stationText = new TextBlock
            {
                Style = (Style)Resources["IncidentDetail"],
                Text = $"📍 {stationName}",
                Foreground = GetIncidentCardSecondaryTextBrush(incident.Status),
                FontSize = 10
            };

            details.Children.Add(locationText);
            details.Children.Add(stationText);

            Grid.SetRow(details, 2);
            content.Children.Add(details);

            // Assignments info
            if (incident.Assignments.Any())
            {
                var assignmentsText = new TextBlock
                {
                    Style = (Style)Resources["IncidentDetail"],
                    Text = $"👥 {incident.Assignments.Count} resource{(incident.Assignments.Count != 1 ? "s" : "")} assigned",
                    Margin = new Thickness(0, 0, 0, 5),
                    Foreground = GetIncidentCardSecondaryTextBrush(incident.Status)
                };
                Grid.SetRow(assignmentsText, 3);
                content.Children.Add(assignmentsText);
            }

            card.Child = content;

            // Add click event
            card.MouseLeftButtonUp += (s, e) =>
            {
                SelectedIncident = incident;
                UpdateIncidentSelection(incident);
            };

            return card;
        }

        private SolidColorBrush GetStatusBrush(IncidentStatus status)
        {
            return status switch
            {
                IncidentStatus.Created => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D")),
                IncidentStatus.OnGoing => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545")),
                IncidentStatus.PartialControl => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFD7E14")),
                IncidentStatus.Controlled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFC107")),
                IncidentStatus.FullyControlled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                IncidentStatus.Closed => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6F7B8A")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
            };
        }

        private Brush GetIncidentCardBackgroundBrush(IncidentStatus status)
        {
            // Create vibrant gradient backgrounds
            return status switch
            {
                IncidentStatus.Created => CreateGradientBrush("#8B949E", "#6C757D"), // Brighter gray gradient
                IncidentStatus.OnGoing => CreateGradientBrush("#FF4757", "#DC3545"), // Vibrant red gradient
                IncidentStatus.PartialControl => CreateGradientBrush("#FF8C00", "#FD7E14"), // Vibrant orange gradient
                IncidentStatus.Controlled => CreateGradientBrush("#FFD700", "#FFC107"), // Bright yellow gradient
                IncidentStatus.FullyControlled => CreateGradientBrush("#32CD32", "#28A745"), // Vibrant green gradient
                IncidentStatus.Closed => CreateGradientBrush("#8B949E", "#6C757D"), // Brighter gray gradient
                _ => CreateGradientBrush("#8B949E", "#6C757D")
            };
        }

        private LinearGradientBrush CreateGradientBrush(string color1, string color2)
        {
            var brush = new LinearGradientBrush();
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 1);
            
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(color1), 0.0));
            brush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString(color2), 1.0));
            
            return brush;
        }

        private SolidColorBrush GetIncidentCardTextBrush(IncidentStatus status)
        {
            // High contrast text colors for vibrant backgrounds
            return status switch
            {
                IncidentStatus.Controlled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A202C")), // Very dark text for bright yellow background
                IncidentStatus.PartialControl => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#000000")), // Black text for partial control
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF")) // Pure white text for other vibrant backgrounds
            };
        }

        private SolidColorBrush GetIncidentCardSecondaryTextBrush(IncidentStatus status)
        {
            // High contrast secondary text
            return status switch
            {
                IncidentStatus.Controlled => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2D3748")), // Dark gray for yellow background
                IncidentStatus.PartialControl => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A1A")), // Very dark gray for partial control
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F7FAFC")) // Very light text for other backgrounds
            };
        }

        private SolidColorBrush GetPriorityBrush(IncidentPriority priority)
        {
            return priority switch
            {
                IncidentPriority.Critical => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFDC3545")),
                IncidentPriority.High => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFD7E14")),
                IncidentPriority.Normal => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF007ACC")),
                IncidentPriority.Low => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF28A745")),
                _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF6C757D"))
            };
        }

        private string GetStatusText(IncidentStatus status)
        {
            return status switch
            {
                IncidentStatus.Created => "CREATED",
                IncidentStatus.OnGoing => "ONGOING",
                IncidentStatus.PartialControl => "PARTIAL CONTROL",
                IncidentStatus.Controlled => "CONTROLLED",
                IncidentStatus.FullyControlled => "FULLY CONTROLLED",
                IncidentStatus.Closed => "CLOSED",
                _ => "UNKNOWN"
            };
        }

        private string GetPriorityText(IncidentPriority priority)
        {
            return priority switch
            {
                IncidentPriority.Critical => "CRITICAL",
                IncidentPriority.High => "HIGH",
                IncidentPriority.Normal => "NORMAL",
                IncidentPriority.Low => "LOW",
                _ => "NORMAL"
            };
        }

        private string GetStationName(int stationId)
        {
            var station = _stations.FirstOrDefault(s => s.Id == stationId);
            return station?.Name ?? $"Station {stationId}";
        }

        private void StartLoadingAnimation()
        {
            var rotationAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoadingRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }

        private void ShowLoading(bool show)
        {
            if (LoadingIndicator != null)
            {
                if (Dispatcher.CheckAccess())
                {
                    LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    Dispatcher.Invoke(() => LoadingIndicator.Visibility = show ? Visibility.Visible : Visibility.Collapsed);
                }
            }
        }

        private void ShowError(string message)
        {
            if (ErrorText != null && ErrorMessage != null && NoIncidentsMessage != null)
            {
                if (Dispatcher.CheckAccess())
                {
                    ErrorText.Text = message;
                    ErrorMessage.Visibility = Visibility.Visible;
                    NoIncidentsMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        ErrorText.Text = message;
                        ErrorMessage.Visibility = Visibility.Visible;
                        NoIncidentsMessage.Visibility = Visibility.Collapsed;
                    });
                }
            }
        }

        private void HideError()
        {
            if (ErrorMessage != null)
            {
                if (Dispatcher.CheckAccess())
                {
                    ErrorMessage.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Dispatcher.Invoke(() => ErrorMessage.Visibility = Visibility.Collapsed);
                }
            }
        }

        private async void RetryButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadIncidentsAsync();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
