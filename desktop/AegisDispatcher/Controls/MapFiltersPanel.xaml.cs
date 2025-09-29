using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AegisDispatcher.Models;

namespace AegisDispatcher.Controls
{
    public partial class MapFiltersPanel : UserControl
    {
        public event EventHandler<MapFilterChangedEventArgs>? FilterChanged;
        public event EventHandler? CloseRequested;

        private User? _currentUser;

        public MapFiltersPanel()
        {
            InitializeComponent();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            UpdateAgencyFiltersVisibility();
        }

        private void UpdateAgencyFiltersVisibility()
        {
            if (_currentUser == null) return;

            // Hide all agency-specific filters first
            FireServiceFilters.Visibility = Visibility.Collapsed;
            PoliceFilters.Visibility = Visibility.Collapsed;
            CoastGuardFilters.Visibility = Visibility.Collapsed;
            EKABFilters.Visibility = Visibility.Collapsed;

            // Show filters based on user's agency
            switch (_currentUser.AgencyName)
            {
                case "Hellenic Fire Service":
                    FireServiceFilters.Visibility = Visibility.Visible;
                    break;
                    
                case "Hellenic Police":
                    PoliceFilters.Visibility = Visibility.Visible;
                    break;
                    
                case "Hellenic Coast Guard":
                    CoastGuardFilters.Visibility = Visibility.Visible;
                    break;
                    
                case "EKAB":
                    EKABFilters.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void FilterToggle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                var filterType = GetFilterTypeFromToggleButton(toggleButton);
                var isEnabled = toggleButton.IsChecked ?? false;
                
                FilterChanged?.Invoke(this, new MapFilterChangedEventArgs(filterType, isEnabled));
            }
        }

        private MapFilterType GetFilterTypeFromToggleButton(ToggleButton toggleButton)
        {
            return toggleButton.Name switch
            {
                nameof(IncidentsToggle) => MapFilterType.Incidents,
                nameof(VehiclesToggle) => MapFilterType.Vehicles,
                nameof(FireStationsToggle) => MapFilterType.FireStations,
                nameof(FireHydrantsToggle) => MapFilterType.FireHydrants,
                nameof(FireStationBoundariesToggle) => MapFilterType.FireStationBoundaries,
                nameof(PoliceStationsToggle) => MapFilterType.PoliceStations,
                nameof(PatrolZonesToggle) => MapFilterType.PatrolZones,
                nameof(CoastGuardStationsToggle) => MapFilterType.CoastGuardStations,
                nameof(ShipsToggle) => MapFilterType.Ships,
                nameof(PatrolZonesCoastGuardToggle) => MapFilterType.PatrolZones,
                nameof(HospitalsToggle) => MapFilterType.Hospitals,
                nameof(AmbulancesToggle) => MapFilterType.Ambulances,
                _ => MapFilterType.Incidents
            };
        }

        private void ShowAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetAllFilters(true);
        }

        private void HideAllButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide all filters including incidents
            SetAllFilters(false);
        }

        private void SetAllFilters(bool isEnabled)
        {
            var toggleButtons = new[]
            {
                IncidentsToggle, VehiclesToggle, FireStationsToggle, FireHydrantsToggle, FireStationBoundariesToggle,
                PoliceStationsToggle, PatrolZonesToggle, CoastGuardStationsToggle,
                ShipsToggle, PatrolZonesCoastGuardToggle, HospitalsToggle, AmbulancesToggle
            };

            foreach (var toggle in toggleButtons)
            {
                if (toggle.Visibility == Visibility.Visible)
                {
                    toggle.IsChecked = isEnabled;
                    var filterType = GetFilterTypeFromToggleButton(toggle);
                    FilterChanged?.Invoke(this, new MapFilterChangedEventArgs(filterType, isEnabled));
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public MapFilters GetCurrentFilters()
        {
            return new MapFilters
            {
                Incidents = IncidentsToggle.IsChecked ?? false,
                Vehicles = VehiclesToggle.IsChecked ?? false,
                FireStations = FireStationsToggle.IsChecked ?? false,
                FireHydrants = FireHydrantsToggle.IsChecked ?? false,
                FireStationBoundaries = FireStationBoundariesToggle.IsChecked ?? false,
                PoliceStations = PoliceStationsToggle.IsChecked ?? false,
                PatrolZones = (PatrolZonesToggle.IsChecked ?? false) || (PatrolZonesCoastGuardToggle.IsChecked ?? false),
                CoastGuardStations = CoastGuardStationsToggle.IsChecked ?? false,
                Ships = ShipsToggle.IsChecked ?? false,
                Hospitals = HospitalsToggle.IsChecked ?? false,
                Ambulances = AmbulancesToggle.IsChecked ?? false
            };
        }

        public void SetFilters(MapFilters filters)
        {
            IncidentsToggle.IsChecked = filters.Incidents;
            VehiclesToggle.IsChecked = filters.Vehicles;
            FireStationsToggle.IsChecked = filters.FireStations;
            FireHydrantsToggle.IsChecked = filters.FireHydrants;
            FireStationBoundariesToggle.IsChecked = filters.FireStationBoundaries;
            PoliceStationsToggle.IsChecked = filters.PoliceStations;
            PatrolZonesToggle.IsChecked = filters.PatrolZones;
            CoastGuardStationsToggle.IsChecked = filters.CoastGuardStations;
            ShipsToggle.IsChecked = filters.Ships;
            PatrolZonesCoastGuardToggle.IsChecked = filters.PatrolZones;
            HospitalsToggle.IsChecked = filters.Hospitals;
            AmbulancesToggle.IsChecked = filters.Ambulances;
        }
    }

    public class MapFilterChangedEventArgs : EventArgs
    {
        public MapFilterType FilterType { get; }
        public bool IsEnabled { get; }

        public MapFilterChangedEventArgs(MapFilterType filterType, bool isEnabled)
        {
            FilterType = filterType;
            IsEnabled = isEnabled;
        }
    }

    public enum MapFilterType
    {
        Incidents,
        Vehicles,
        FireStations,
        FireHydrants,
        FireStationBoundaries,
        PoliceStations,
        PatrolZones,
        CoastGuardStations,
        Ships,
        Hospitals,
        Ambulances
    }

    public class MapFilters
    {
        public bool Incidents { get; set; } = true;
        public bool Vehicles { get; set; } = true;
        public bool FireStations { get; set; } = true;
        public bool FireHydrants { get; set; } = false;
        public bool FireStationBoundaries { get; set; } = false;
        public bool PoliceStations { get; set; } = true;
        public bool PatrolZones { get; set; } = true;
        public bool CoastGuardStations { get; set; } = true;
        public bool Ships { get; set; } = true;
        public bool Hospitals { get; set; } = true;
        public bool Ambulances { get; set; } = true;
    }
}