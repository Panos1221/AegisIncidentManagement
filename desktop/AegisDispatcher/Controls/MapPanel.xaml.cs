using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;
using System.Text.Json;
using System.Globalization;

namespace AegisDispatcher.Controls
{
    public partial class MapPanel : UserControl
    {
        private readonly IApiService _apiService;
        private readonly ILoggingService _loggingService;

        private readonly JsonSerializerOptions _jsonOptions;
        private bool _isMapLoaded = false;
        private bool _isSatelliteView = false;
        private bool _isLegendVisible = true;
        private bool _isResourcesVisible = false;
        private bool _isUpdatingFilters = false;
        private MapFilters _currentFilters = new MapFilters();
        private Point _lastRightClickPoint;
        private readonly List<Incident> _incidents = new();
        private readonly List<Station> _stations = new();
        private readonly List<Vehicle> _vehicles = new();
        private readonly List<FireHydrant> _fireHydrants = new();
        private readonly List<FireStationBoundary> _fireStationBoundaries = new();
        private readonly List<Ship> _ships = new();
        private readonly List<CoastGuardStation> _coastGuardStations = new();
        private readonly List<PoliceStation> _policeStations = new();
        private readonly List<Hospital> _hospitals = new();
        private readonly List<PatrolZone> _patrolZones = new();
        

        
        private User? _currentUser;

        public event EventHandler<Point>? LocationSelected;
        public event EventHandler<Incident>? IncidentClicked;

        public bool IsMapLoaded => _isMapLoaded;
        public bool IsSatelliteView => _isSatelliteView;
        
        /// <summary>
        /// When true, left-clicks on the map will trigger LocationSelected event
        /// Used in NewIncidentWindow for direct location selection
        /// </summary>
        public bool EnableLeftClickLocationSelection { get; set; } = false;
        
        /// <summary>
        /// Disables the right-click context menu (used in NewIncidentWindow)
        /// </summary>
        public bool DisableContextMenu { get; set; } = false;
        
        /// <summary>
        /// When true, hides all other markers (incidents, stations, vehicles, etc.) and only shows location pins
        /// Used in NewIncidentWindow to show only the selected location
        /// </summary>
        public bool HideOtherMarkers { get; set; } = false;
        
        public async Task ToggleMapView()
        {
            _isSatelliteView = !_isSatelliteView;
            if (_isMapLoaded)
            {
                var tileLayer = _isSatelliteView 
                    ? "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"
                    : "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png";
                    
                var attribution = _isSatelliteView
                    ? "Tiles © Esri — Source: Esri, Maxar, Earthstar Geographics, and the GIS User Community"
                    : "© OpenStreetMap contributors";
                    
                await MapWebView.CoreWebView2.ExecuteScriptAsync($"changeTileLayer('{tileLayer}', '{attribution}');");
                _loggingService.LogInformation($"MapPanel: Switched to {(_isSatelliteView ? "satellite" : "street")} view");
            }
        }
        


        public MapPanel()
        {
            InitializeComponent();
            
            _apiService = App.ServiceProvider?.GetService<IApiService>() 
                ?? throw new InvalidOperationException("API service not available");
            _loggingService = App.ServiceProvider?.GetService<ILoggingService>() 
                ?? throw new InvalidOperationException("Logging service not available");


            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };



            // Wire up the NavigationCompleted event
            MapWebView.NavigationCompleted += MapWebView_NavigationCompleted;

            InitializeMap();
            StartLoadingAnimation();
        }

        public void SetCurrentUser(User user)
        {
            _currentUser = user;
            UpdateAgencySpecificResources();
        }

        public void ApplyExternalFilter(MapFilterType filterType, bool isEnabled)
        {
            if (_isUpdatingFilters) return; // Prevent recursive updates
            
            _isUpdatingFilters = true;
            
            try
            {
                // Update the internal filters
                switch (filterType)
                {
                    case MapFilterType.Incidents:
                        _currentFilters.Incidents = isEnabled;
                        ShowIncidentsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.Vehicles:
                        _currentFilters.Vehicles = isEnabled;
                        ShowVehiclesToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.FireStations:
                        _currentFilters.FireStations = isEnabled;
                        ShowFireStationsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.FireHydrants:
                        _currentFilters.FireHydrants = isEnabled;
                        ShowFireHydrantsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.FireStationBoundaries:
                        _currentFilters.FireStationBoundaries = isEnabled;
                        ShowFireStationBoundariesToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.PoliceStations:
                        _currentFilters.PoliceStations = isEnabled;
                        ShowPoliceStationsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.PatrolZones:
                        _currentFilters.PatrolZones = isEnabled;
                        ShowPatrolZonesToggle.IsChecked = isEnabled;
                        ShowPatrolZonesCoastGuardToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.CoastGuardStations:
                        _currentFilters.CoastGuardStations = isEnabled;
                        ShowCoastGuardStationsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.Ships:
                        _currentFilters.Ships = isEnabled;
                        ShowShipsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.Hospitals:
                        _currentFilters.Hospitals = isEnabled;
                        ShowHospitalsToggle.IsChecked = isEnabled;
                        break;
                    case MapFilterType.Ambulances:
                        _currentFilters.Ambulances = isEnabled;
                        ShowAmbulancesToggle.IsChecked = isEnabled;
                        break;
                }

                // Apply the filter to the map
                ApplyFilterToMap(filterType, isEnabled);
            }
            finally
            {
                _isUpdatingFilters = false;
            }
        }

        public MapFilters GetCurrentFilters()
        {
            return new MapFilters
            {
                Incidents = _currentFilters.Incidents,
                Vehicles = _currentFilters.Vehicles,
                FireStations = _currentFilters.FireStations,
                FireHydrants = _currentFilters.FireHydrants,
                FireStationBoundaries = _currentFilters.FireStationBoundaries,
                PoliceStations = _currentFilters.PoliceStations,
                PatrolZones = _currentFilters.PatrolZones,
                CoastGuardStations = _currentFilters.CoastGuardStations,
                Ships = _currentFilters.Ships,
                Hospitals = _currentFilters.Hospitals,
                Ambulances = _currentFilters.Ambulances
            };
        }

        /// <summary>
        /// Refreshes all map data including incidents, vehicles, and other resources
        /// </summary>
        public async Task RefreshMapDataAsync()
        {
            try
            {
                Console.WriteLine("MapPanel: Manual refresh requested");
                _loggingService.LogInformation("MapPanel: Manual refresh requested");
                await LoadMapData();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MapPanel: Error during manual refresh: {ex.Message}");
                _loggingService.LogError(ex, "MapPanel: Error during manual refresh");
            }
        }

        private void UpdateAgencySpecificResources()
        {
            if (_currentUser == null) return;

            // Hide all agency-specific resources first
            FireServiceResources.Visibility = Visibility.Collapsed;
            PoliceResources.Visibility = Visibility.Collapsed;
            CoastGuardResources.Visibility = Visibility.Collapsed;
            EKABResources.Visibility = Visibility.Collapsed;

            // Set default filters based on user's agency - only show relevant layers
            _currentFilters = new MapFilters
            {
                Incidents = true,
                Vehicles = true,
                FireStations = false,
                FireHydrants = false,
                FireStationBoundaries = false,
                PoliceStations = false,
                PatrolZones = false,
                CoastGuardStations = false,
                Ships = false,
                Hospitals = false,
                Ambulances = false
            };

            // Show resources based on user's agency
            switch (_currentUser.AgencyName)
            {
                case "Hellenic Fire Service":
                    FireServiceResources.Visibility = Visibility.Visible;
                    _currentFilters.FireStations = true;
                    _currentFilters.FireHydrants = true; // Enable fire hydrants by default for Fire Service
                    break;
                    
                case "Hellenic Police":
                    PoliceResources.Visibility = Visibility.Visible;
                    _currentFilters.PoliceStations = true;
                    _currentFilters.PatrolZones = true;
                    break;
                    
                case "Hellenic Coast Guard":
                    CoastGuardResources.Visibility = Visibility.Visible;
                    _currentFilters.CoastGuardStations = true;
                    _currentFilters.Ships = true;
                    _currentFilters.PatrolZones = true;
                    break;
                    
                case "EKAB":
                    EKABResources.Visibility = Visibility.Visible;
                    _currentFilters.Hospitals = true;
                    _currentFilters.Ambulances = true;
                    break;
            }

            // Update checkbox states to match filters
            UpdateResourceCheckboxes();
        }

        private void UpdateResourceCheckboxes()
        {
            ShowIncidentsToggle.IsChecked = _currentFilters.Incidents;
            ShowVehiclesToggle.IsChecked = _currentFilters.Vehicles;
            ShowFireStationsToggle.IsChecked = _currentFilters.FireStations;
            ShowFireHydrantsToggle.IsChecked = _currentFilters.FireHydrants;
            ShowFireStationBoundariesToggle.IsChecked = _currentFilters.FireStationBoundaries;
            ShowPoliceStationsToggle.IsChecked = _currentFilters.PoliceStations;
            ShowPatrolZonesToggle.IsChecked = _currentFilters.PatrolZones;
            ShowCoastGuardStationsToggle.IsChecked = _currentFilters.CoastGuardStations;
            ShowShipsToggle.IsChecked = _currentFilters.Ships;
            ShowPatrolZonesCoastGuardToggle.IsChecked = _currentFilters.PatrolZones;
            ShowHospitalsToggle.IsChecked = _currentFilters.Hospitals;
            ShowAmbulancesToggle.IsChecked = _currentFilters.Ambulances;
        }

        private async void InitializeMap()
        {
            try
            {
                _loggingService.LogInformation("MapPanel: Initializing WebView2...");
                
                // Initialize WebView2 with map HTML
                await MapWebView.EnsureCoreWebView2Async();
                
                _loggingService.LogInformation("MapPanel: WebView2 core initialized successfully");
                
                // Set up virtual host mapping for icons
                var iconsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Icons");
                if (System.IO.Directory.Exists(iconsPath))
                {
                    _loggingService.LogInformation($"MapPanel: Setting up virtual host for icons at: {iconsPath}");
                    MapWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "aegis-icons.local",
                        iconsPath,
                        CoreWebView2HostResourceAccessKind.Allow);
                }
                else
                {
                    _loggingService.LogWarning($"MapPanel: Icons directory not found at: {iconsPath}");
                }
                
                // Set up virtual host mapping for Resources (including JS files)
                var resourcesPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
                if (System.IO.Directory.Exists(resourcesPath))
                {
                    _loggingService.LogInformation($"MapPanel: Setting up virtual host for resources at: {resourcesPath}");
                    MapWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                        "aegis-resources.local",
                        resourcesPath,
                        CoreWebView2HostResourceAccessKind.Allow);
                }
                else
                {
                    _loggingService.LogWarning($"MapPanel: Resources directory not found at: {resourcesPath}");
                }
                
                _loggingService.LogInformation("MapPanel: Creating HTML content...");
                var mapHtml = CreateMapHTML();
                _loggingService.LogInformation("MapPanel: HTML created, navigating to map...");
                MapWebView.NavigateToString(mapHtml);
                _loggingService.LogInformation("MapPanel: Navigation started successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"MapPanel: Failed to initialize map: {ex.Message}");
                _loggingService.LogError($"MapPanel: Stack trace: {ex.StackTrace}");
                ShowFallbackMap($"Failed to initialize map: {ex.Message}");
            }
        }

        private string CreateMapHTML()
        {
            return """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title>AegisDispatcher Map</title>
    <meta name="viewport" content="initial-scale=1,maximum-scale=1,user-scalable=no" />
    <link rel="stylesheet" href="https://aegis-resources.local/js/leaflet.css" />
    <style>
        body { margin: 0; padding: 0; background: #1e1e1e; }
        #map { height: 100vh; width: 100vw; }
        .leaflet-container { background: #1e1e1e; }
        .leaflet-control-container .leaflet-control { background: #2d2d30; border: 1px solid #626262; }
        .leaflet-control-container .leaflet-control a { background: #2d2d30; color: white; }
        .leaflet-control-container .leaflet-control a:hover { background: #007acc; }
        .leaflet-control-attribution { display: none !important; }
        .leaflet-popup-content-wrapper { background: white; color: #333; border-radius: 8px; }
        .leaflet-popup-tip { background: white; }
        .leaflet-popup-content { margin: 12px 16px; }
        .leaflet-popup-content h3 { margin: 0 0 8px 0; color: #111827; font-size: 14px; font-weight: 600; }
        .leaflet-popup-content p { margin: 4px 0; font-size: 12px; color: #374151; }
        .leaflet-popup-content strong { color: #111827; }
        .status-badge { padding: 2px 6px; border-radius: 4px; font-size: 10px; font-weight: bold; }
        .status-created { background: #dc3545; color: white; }
        .status-ongoing { background: #fd7e14; color: white; }
        .status-controlled { background: #28a745; color: white; }
        .status-closed { background: #6c757d; color: white; }
        .priority-critical { color: #dc3545; font-weight: bold; }
        .priority-high { color: #fd7e14; font-weight: bold; }
        .priority-normal { color: #28a745; }
        .priority-low { color: #6c757d; }
    </style>
</head>
<body>
    <div id="map"></div>
    <script>
        // Check if Leaflet is available, if not, load it
        function loadLeaflet() {
            return new Promise((resolve, reject) => {
                if (typeof L !== 'undefined') {
                    console.log('Leaflet already loaded');
                    resolve();
                    return;
                }
                
                console.log('Loading Leaflet library...');
                
                // Load CSS first
                const link = document.createElement('link');
                link.rel = 'stylesheet';
                link.href = 'https://aegis-resources.local/js/leaflet.css';
                document.head.appendChild(link);
                
                // Load JavaScript
                const script = document.createElement('script');
                script.src = 'https://aegis-resources.local/js/leaflet.js';
                script.onload = () => {
                    console.log('Leaflet loaded successfully');
                    resolve();
                };
                script.onerror = (error) => {
                    console.error('Failed to load Leaflet:', error);
                    reject(error);
                };
                document.head.appendChild(script);
            });
        }
        
        var map;
        var markers = {};
        var currentTileLayer;
        
        function debug(msg) {
            console.log(msg);
        }
        
        async function initializeMap() {
            console.log('Initializing map...');
            
            // Guard against multiple initializations
            if (map) {
                console.log('Map already initialized, skipping...');
                return;
            }
            
            try {
                // Ensure Leaflet is loaded
                await loadLeaflet();
                
                // Wait a bit for Leaflet to be fully ready
                await new Promise(resolve => setTimeout(resolve, 100));
                
                if (typeof L === 'undefined') {
                    throw new Error('Leaflet library not available');
                }
                
                console.log('Creating map with Leaflet version:', L.version);
                
                // Initialize map centered on Greece (Athens)
                map = L.map('map').setView([37.9755, 23.7348], 8);
                console.log('Map created successfully');
                
                // Add OpenStreetMap tiles with error handling
                currentTileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                    attribution: '',
                    maxZoom: 19,
                    errorTileUrl: 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjU2IiBoZWlnaHQ9IjI1NiIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48cmVjdCB3aWR0aD0iMjU2IiBoZWlnaHQ9IjI1NiIgZmlsbD0iI2Y4ZjlmYSIvPjx0ZXh0IHg9IjUwJSIgeT0iNTAlIiBmb250LWZhbWlseT0iQXJpYWwiIGZvbnQtc2l6ZT0iMTQiIGZpbGw9IiM2Yzc1N2QiIHRleHQtYW5jaG9yPSJtaWRkbGUiIGR5PSIuM2VtIj5NYXAgVGlsZSBOb3QgQXZhaWxhYmxlPC90ZXh0Pjwvc3ZnPg=='
                }).addTo(map);
                console.log('Tile layer added');
            
                // Add basic event listeners
                map.on('click', function(e) {
                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            type: 'click',
                            lat: e.latlng.lat,
                            lng: e.latlng.lng
                        });
                    }
                });
                
                map.on('contextmenu', function(e) {
                    if (window.chrome && window.chrome.webview) {
                        // Get the pixel coordinates of the right-click
                        var containerPoint = map.latLngToContainerPoint(e.latlng);
                        window.chrome.webview.postMessage({
                            type: 'contextmenu',
                            lat: e.latlng.lat,
                            lng: e.latlng.lng,
                            x: containerPoint.x,
                            y: containerPoint.y
                        });
                    }
                });
                
                map.on('mousemove', function(e) {
                    if (window.chrome && window.chrome.webview) {
                        window.chrome.webview.postMessage({
                            type: 'coordinates',
                            lat: e.latlng.lat.toFixed(6),
                            lng: e.latlng.lng.toFixed(6)
                        });
                    }
                });
                
                // Add zoom event handler for dynamic icon sizing
                map.on('zoomend', function() {
                    updateAllMarkerSizes();
                });
                
                console.log('Map initialization complete, sending ready message...');
                // Signal that map is ready
                if (window.chrome && window.chrome.webview) {
                    console.log('WebView2 available, posting ready message');
                    window.chrome.webview.postMessage({ type: 'ready' });
                } else {
                    console.log('WebView2 not available');
                }
            } catch (error) {
                console.error('Error initializing map:', error);
                console.error('Error stack:', error.stack);
                
                // Send error to C#
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.postMessage({
                        type: 'map-error',
                        message: error.message,
                        stack: error.stack
                    });
                }
            }
        }
        
        function getIconSize(baseSize, zoom) {
            // Calculate dynamic size based on zoom level
            // Zoom levels typically range from 1-20, with 10-13 being common city/region levels
            var minZoom = 6;   // Very zoomed out (country level)
            var maxZoom = 18;  // Very zoomed in (street level)
            var minScale = 0.4; // Minimum scale factor
            var maxScale = 1.2; // Maximum scale factor
            
            // Normalize zoom to 0-1 range
            var normalizedZoom = Math.max(0, Math.min(1, (zoom - minZoom) / (maxZoom - minZoom)));
            
            // Calculate scale factor
            var scale = minScale + (maxScale - minScale) * normalizedZoom;
            
            return Math.round(baseSize * scale);
        }
        
        function addMarker(id, lat, lng, type, title, content, iconData, iconPath) {
            var icon;
            var currentZoom = map.getZoom();
            
            // Create custom icons based on type and agency using actual PNG files with dynamic sizing
            switch(type) {
                case 'incident':
                    if (iconPath) {
                        // Use the provided icon path from C# IncidentIconService
                        var incidentSize = getIconSize(42, currentZoom);
                        icon = L.icon({
                            iconUrl: iconPath,
                            iconSize: [incidentSize, incidentSize],
                            iconAnchor: [incidentSize/2, incidentSize],
                            popupAnchor: [0, -incidentSize]
                        });
                    } else {
                        // Fallback to old logic if no icon path provided
                        icon = getIncidentIcon(iconData, currentZoom);
                    }
                    break;
                case 'fire-station':
                    var stationSize = getIconSize(32, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/fireStation.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                    break;
                case 'police-station':
                    var stationSize = getIconSize(32, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/policeStation.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                    break;
                case 'coast-guard-station':
                    var stationSize = getIconSize(32, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/port-station.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                    break;
                case 'hospital':
                    var stationSize = getIconSize(32, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/hospital.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                    break;
                case 'vehicle':
                    var vehicleType = iconData?.type || 'police';
                    var vehicleIcon = 'policeCar.png';
                    if (vehicleType.toLowerCase().includes('fire')) {
                        vehicleIcon = 'fireEngine.png';
                    } else if (vehicleType.toLowerCase().includes('ambulance') || vehicleType.toLowerCase().includes('ekab')) {
                        vehicleIcon = 'ambulance.png';
                    }
                    
                    var vehicleSize = getIconSize(24, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Vehicles/' + vehicleIcon,
                        iconSize: [vehicleSize, vehicleSize],
                        iconAnchor: [vehicleSize/2, vehicleSize],
                        popupAnchor: [0, -vehicleSize]
                    });
                    break;
                case 'ship':
                    var vehicleSize = getIconSize(24, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/Vehicles/coastGuardShip.png',
                        iconSize: [vehicleSize, vehicleSize],
                        iconAnchor: [vehicleSize/2, vehicleSize],
                        popupAnchor: [0, -vehicleSize]
                    });
                    break;
                case 'fire-hydrant':
                    var hydrantSize = getIconSize(20, currentZoom);
                    icon = L.icon({
                        iconUrl: 'https://aegis-icons.local/General/fire-hydrant-icon.png',
                        iconSize: [hydrantSize, hydrantSize],
                        iconAnchor: [hydrantSize/2, hydrantSize],
                        popupAnchor: [0, -hydrantSize]
                    });
                    break;
                case 'location-pin':
                    var pinSize = getIconSize(32, currentZoom);
                    icon = L.icon({
                        iconUrl: 'data:image/svg+xml;base64,' + btoa('<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="#e74c3c"><path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/></svg>'),
                        iconSize: [pinSize, pinSize],
                        iconAnchor: [pinSize/2, pinSize],
                        popupAnchor: [0, -pinSize]
                    });
                    break;
                default:
                    icon = L.marker();
            }
            
            // Remove existing marker with same ID if it exists
            if (markers[id]) {
                map.removeLayer(markers[id]);
                delete markers[id];
            }
            
            var marker = L.marker([lat, lng], { icon: icon }).addTo(map);
            if (content) {
                marker.bindPopup(content);
            }
            
            // Add click handler for incidents
            if (type === 'incident') {
                marker.on('click', function() {
                    console.log('Incident marker clicked, id:', id);
                    var extractedId = id.replace('incident_', '');
                    console.log('Extracted incident ID:', extractedId);
                    if (window.chrome && window.chrome.webview) {
                        try {
                            window.chrome.webview.postMessage({
                                type: 'incident-click',
                                id: extractedId
                            });
                            console.log('Sent incident-click message to C#');
                        } catch (error) {
                            console.error('Error sending message to C#:', error);
                        }
                    } else {
                        console.log('WebView2 not available');
                    }
                });
            }
            
            // Store marker metadata for zoom updates
            marker._markerType = type;
            marker._markerIconData = iconData;
            marker._markerContent = content;
            marker._markerTitle = title;
            marker._markerIconPath = iconPath;
            
            markers[id] = marker;
            return marker;
        }
        

        
        function getIncidentIcon(iconData, zoom) {
            if (!iconData) {
                return getDefaultIncidentIcon(zoom);
            }
            
            var agency = iconData.userAgency || 'default';
            var mainCategory = iconData.mainCategory || '';
            var status = iconData.status || 'Created';
            
            // Get icon based on agency and incident type
            if (agency === 'Hellenic Fire Service') {
                return getFireDepartmentIncidentIcon(mainCategory, status, zoom);
            } else if (agency === 'EKAB') {
                return getEKABIncidentIcon(status, zoom);
            } else {
                return getDefaultIncidentIcon(status, zoom);
            }
        }
        
        function getFireDepartmentIncidentIcon(mainCategory, status, zoom) {
            var iconPath = getFireDepartmentIconPath(mainCategory, status);
            var incidentSize = getIconSize(42, zoom);
            return L.icon({
                iconUrl: iconPath,
                iconSize: [incidentSize, incidentSize],
                iconAnchor: [incidentSize/2, incidentSize],
                popupAnchor: [0, -incidentSize]
            });
        }
        
        function getEKABIncidentIcon(status, zoom) {
            var iconPath = getEKABIconPath(status);
            var incidentSize = getIconSize(42, zoom);
            return L.icon({
                iconUrl: iconPath,
                iconSize: [incidentSize, incidentSize],
                iconAnchor: [incidentSize/2, incidentSize],
                popupAnchor: [0, -incidentSize]
            });
        }
        
        function getDefaultIncidentIcon(status, zoom) {
            var statusColor = getIncidentStatusColor(status || 'Created');
            var incidentSize = getIconSize(32, zoom || map.getZoom());
            var fontSize = Math.max(8, Math.round(incidentSize * 0.5));
            
            return L.divIcon({
                className: 'incident-marker',
                html: '<div style="background: ' + statusColor + '; color: white; border-radius: 50%; width: ' + incidentSize + 'px; height: ' + incidentSize + 'px; display: flex; align-items: center; justify-content: center; font-size: ' + fontSize + 'px; border: 2px solid white; box-shadow: 0 2px 4px rgba(0,0,0,0.3);">🚨</div>',
                iconSize: [incidentSize, incidentSize],
                iconAnchor: [incidentSize/2, incidentSize]
            });
        }
        
        function getFireDepartmentIconPath(mainCategory, status) {
            var statusKey = getStatusKey(status);
            var baseUrl = 'https://aegis-icons.local/Incidents/FireDept/';
            
            // Map categories to icon types
            if (mainCategory.includes('ΔΑΣΙΚΕΣ') || mainCategory.includes('Forest')) {
                return baseUrl + 'wildfire-' + statusKey + '.png';
            } else if (mainCategory.includes('ΑΣΤΙΚΕΣ') || mainCategory.includes('Urban')) {
                return baseUrl + 'fire-location-' + statusKey + '.png';
            } else {
                return baseUrl + 'fire-location-' + statusKey + '.png';
            }
        }
        
        function getEKABIconPath(status) {
            var statusKey = getStatusKey(status);
            return 'https://aegis-icons.local/Incidents/Universal/help-location-' + statusKey + '.png';
        }
        
        function getStatusKey(status) {
            switch(status) {
                case 'OnGoing': return 'ongoing';
                case 'PartialControl': return 'partial';
                case 'Controlled': return 'controlled';
                case 'FullyControlled':
                case 'Closed': return 'ended';
                default: return 'ongoing';
            }
        }
        
        function getIncidentStatusColor(status) {
            switch(status) {
                case 'Created': return '#dc3545';
                case 'OnGoing': return '#fd7e14';
                case 'PartialControl': return '#ffc107';
                case 'FullyControlled': return '#28a745';
                case 'Closed': return '#6c757d';
                default: return '#dc3545';
            }
        }
        
        function getIncidentStatusBadgeColor(status) {
            switch(status) {
                case 'Created': return 'bg-red-100 text-red-800';
                case 'OnGoing': return 'bg-orange-100 text-orange-800';
                case 'PartialControl': return 'bg-yellow-100 text-yellow-800';
                case 'FullyControlled': return 'bg-green-100 text-green-800';
                case 'Closed': return 'bg-gray-100 text-gray-800';
                default: return 'bg-red-100 text-red-800';
            }
        }
        
        function getIncidentStatusTranslation(status) {
            switch(status) {
                case 'Created': return 'Created';
                case 'OnGoing': return 'Ongoing';
                case 'PartialControl': return 'Partial Control';
                case 'FullyControlled': return 'Fully Controlled';
                case 'Closed': return 'Closed';
                default: return 'Unknown';
            }
        }
        
        function getVehicleStatusColor(status) {
            switch(status) {
                case 'Available': return '#28a745';
                case 'Notified': return '#ffc107';
                case 'EnRoute': return '#fd7e14';
                case 'OnScene': return '#dc3545';
                case 'Busy': return '#6f42c1';
                case 'Maintenance': return '#6c757d';
                case 'Offline': return '#343a40';
                default: return '#28a745';
            }
        }
        
        function removeMarker(id) {
            if (markers[id]) {
                map.removeLayer(markers[id]);
                delete markers[id];
            }
        }
        
        function clearMarkers() {
            for (var id in markers) {
                map.removeLayer(markers[id]);
            }
            markers = {};
        }
        
        function clearMarkersByType(type) {
            console.log('Clearing markers of type:', type);
            for (var id in markers) {
                if (id.startsWith(type + '_')) {
                    if (map.hasLayer(markers[id])) {
                        map.removeLayer(markers[id]);
                    }
                    delete markers[id];
                }
            }
        }
        
        function changeTileLayer(tileUrl, attribution) {
            console.log('Changing tile layer to:', tileUrl);
            if (currentTileLayer) {
                map.removeLayer(currentTileLayer);
            }
            currentTileLayer = L.tileLayer(tileUrl, {
                attribution: attribution || '',
                maxZoom: 19
            }).addTo(map);
        }
        
        function changeTileLayer(tileUrl, attribution) {
            if (currentTileLayer) {
                map.removeLayer(currentTileLayer);
            }
            currentTileLayer = L.tileLayer(tileUrl, {
                attribution: attribution || '© OpenStreetMap contributors',
                maxZoom: 19
            }).addTo(map);
        }
        
        function setView(lat, lng, zoom) {
            map.setView([lat, lng], zoom || 13);
        }
        
        function fitBounds(bounds) {
            if (bounds && bounds.length === 4) {
                map.fitBounds([[bounds[0], bounds[1]], [bounds[2], bounds[3]]]);
            }
        }
        
        function updateAllMarkerSizes() {
            var currentZoom = map.getZoom();
            console.log('Updating marker sizes for zoom level:', currentZoom);
            
            // Update all markers with new sizes
            for (var id in markers) {
                var marker = markers[id];
                if (marker._markerType) {
                    var newIcon = createIconForType(
                        marker._markerType, 
                        marker._markerIconData, 
                        currentZoom,
                        marker._markerIconPath
                    );
                    marker.setIcon(newIcon);
                }
            }
        }
        
        function createIconForType(type, iconData, zoom, iconPath) {
            switch(type) {
                case 'incident':
                    // Use iconPath from C# IncidentIconService if available
                    if (iconPath) {
                        var incidentSize = getIconSize(32, zoom);
                        return L.icon({
                            iconUrl: iconPath,
                            iconSize: [incidentSize, incidentSize],
                            iconAnchor: [incidentSize/2, incidentSize],
                            popupAnchor: [0, -incidentSize]
                        });
                    }
                    return getIncidentIcon(iconData, zoom);
                case 'fire-station':
                    var stationSize = getIconSize(32, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/fireStation.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                case 'police-station':
                    var stationSize = getIconSize(32, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/policeStation.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                case 'coast-guard-station':
                    var stationSize = getIconSize(32, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/port-station.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                case 'hospital':
                    var stationSize = getIconSize(32, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Stations/hospital.png',
                        iconSize: [stationSize, stationSize],
                        iconAnchor: [stationSize/2, stationSize],
                        popupAnchor: [0, -stationSize]
                    });
                case 'vehicle':
                    var vehicleType = iconData?.type || 'police';
                    var vehicleIcon = 'policeCar.png';
                    if (vehicleType.toLowerCase().includes('fire')) {
                        vehicleIcon = 'fireEngine.png';
                    } else if (vehicleType.toLowerCase().includes('ambulance') || vehicleType.toLowerCase().includes('ekab')) {
                        vehicleIcon = 'ambulance.png';
                    }
                    
                    var vehicleSize = getIconSize(24, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Vehicles/' + vehicleIcon,
                        iconSize: [vehicleSize, vehicleSize],
                        iconAnchor: [vehicleSize/2, vehicleSize],
                        popupAnchor: [0, -vehicleSize]
                    });
                case 'ship':
                    var vehicleSize = getIconSize(24, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/Vehicles/coastGuardShip.png',
                        iconSize: [vehicleSize, vehicleSize],
                        iconAnchor: [vehicleSize/2, vehicleSize],
                        popupAnchor: [0, -vehicleSize]
                    });
                case 'fire-hydrant':
                    var hydrantSize = getIconSize(20, zoom);
                    return L.icon({
                        iconUrl: 'https://aegis-icons.local/General/fire-hydrant-icon.png',
                        iconSize: [hydrantSize, hydrantSize],
                        iconAnchor: [hydrantSize/2, hydrantSize],
                        popupAnchor: [0, -hydrantSize]
                    });
                default:
                    return L.marker();
            }
        }
        
        // Function to handle viewing incident details
        function viewIncidentDetails(incidentId) {
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.postMessage({
                    type: 'view-incident-details',
                    incidentId: incidentId
                });
            }
        }
        
        // Store polygons separately from markers
        var polygons = {};
        
        // Function to add polygon boundaries
        function addPolygon(id, coordinates, type, title, content, style) {
            try {
                // Remove existing polygon with same ID if it exists
                if (polygons[id]) {
                    map.removeLayer(polygons[id]);
                    delete polygons[id];
                }
                
                // Create polygon from coordinates
                var polygon = L.polygon(coordinates, {
                    color: style?.color || '#FF6B35',
                    weight: style?.weight || 2,
                    opacity: style?.opacity || 0.8,
                    fillColor: style?.fillColor || '#FF6B35',
                    fillOpacity: style?.fillOpacity || 0.2
                }).addTo(map);
                
                if (content) {
                    polygon.bindPopup(content);
                }
                
                polygons[id] = polygon;
                return polygon;
            } catch (error) {
                console.error('Error adding polygon:', error);
                return null;
            }
        }
        
        // Function to toggle polygon visibility
        function togglePolygonVisibility(layerType, isVisible) {
            console.log('Toggling polygon visibility:', layerType, isVisible);
            
            for (var id in polygons) {
                if (id.startsWith(layerType + '_')) {
                    var polygon = polygons[id];
                    if (isVisible) {
                        if (!map.hasLayer(polygon)) {
                            polygon.addTo(map);
                        }
                    } else {
                        if (map.hasLayer(polygon)) {
                            map.removeLayer(polygon);
                        }
                    }
                }
            }
        }
        
        // Function to toggle layer visibility
        function toggleLayerVisibility(layerType, isVisible) {
            console.log('Toggling layer visibility:', layerType, isVisible);
            
            // Handle markers
            for (var id in markers) {
                if (id.startsWith(layerType + '_')) {
                    var marker = markers[id];
                    if (isVisible) {
                        // Only add to map if not already added
                        if (!map.hasLayer(marker)) {
                            marker.addTo(map);
                        }
                    } else {
                        // Remove from map if it exists
                        if (map.hasLayer(marker)) {
                            map.removeLayer(marker);
                        }
                    }
                }
            }
            
            // Handle polygons
            togglePolygonVisibility(layerType, isVisible);
        }

        // Initialize map when page loads
        document.addEventListener('DOMContentLoaded', function() {
            console.log('DOM loaded, initializing map...');
            try {
                initializeMap();
            } catch (error) {
                console.error('Error during map initialization:', error);
                console.error('Error stack:', error.stack);
            }
        });
        
        // Additional error handling
        window.onerror = function(msg, url, lineNo, columnNo, error) {
            console.error('JavaScript Error:', {
                message: msg,
                source: url,
                line: lineNo,
                column: columnNo,
                error: error
            });
            return false;
        };
        
        window.addEventListener('unhandledrejection', function(event) {
            console.error('Unhandled promise rejection:', event.reason);
        });
    </script>
</body>
</html>
""";
        }

        private void StartLoadingAnimation()
        {
            var rotationAnimation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            MapLoadingRotation.BeginAnimation(RotateTransform.AngleProperty, rotationAnimation);
        }



        private async void MapWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Set up message handling
                MapWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                
                // Wait a bit for the event handler to be fully registered
                await Task.Delay(100);
                await MapWebView.CoreWebView2.ExecuteScriptAsync(@"
                    console.log('Testing WebView2 communication...');
                    if (window.chrome && window.chrome.webview) {
                        console.log('WebView2 available, sending test message');
                        window.chrome.webview.postMessage({
                            type: 'test-message',
                            message: 'Communication test from JavaScript'
                        });
                        console.log('Test message sent');
                    } else {
                        console.log('WebView2 not available for test message');
                    }
                ");

                
                // Set up JavaScript error logging using available events
                MapWebView.CoreWebView2.DOMContentLoaded += async (s, args) => {
                    _loggingService.LogInformation("MapPanel: DOM Content Loaded");
                    
                    // Add comprehensive error handling to the page
                    await MapWebView.CoreWebView2.ExecuteScriptAsync(@"
                        // Global error handler
                        window.addEventListener('error', function(e) {
                            const errorMsg = 'JS Error: ' + (e.error?.message || e.message) + ' at ' + e.filename + ':' + e.lineno + ':' + e.colno;
                            console.error(errorMsg);
                            console.error('Stack:', e.error?.stack);
                            
                            // Send error to C#
                            if (window.chrome && window.chrome.webview) {
                                window.chrome.webview.postMessage({
                                    type: 'javascript-error',
                                    message: errorMsg,
                                    stack: e.error?.stack || 'No stack trace available',
                                    filename: e.filename,
                                    lineno: e.lineno,
                                    colno: e.colno
                                });
                            }
                        });
                        
                        // Unhandled promise rejection handler
                        window.addEventListener('unhandledrejection', function(e) {
                            const errorMsg = 'Unhandled Promise Rejection: ' + e.reason;
                            console.error(errorMsg);
                            
                            if (window.chrome && window.chrome.webview) {
                                window.chrome.webview.postMessage({
                                    type: 'javascript-error',
                                    message: errorMsg,
                                    stack: e.reason?.stack || 'No stack trace available'
                                });
                            }
                        });
                        
                        // Override console methods to capture all logs
                        const originalLog = console.log;
                        const originalError = console.error;
                        const originalWarn = console.warn;
                        
                        console.log = function(...args) {
                            originalLog.apply(console, args);
                            if (window.chrome && window.chrome.webview) {
                                window.chrome.webview.postMessage({
                                    type: 'console-log',
                                    level: 'log',
                                    message: args.join(' ')
                                });
                            }
                        };
                        
                        console.error = function(...args) {
                            originalError.apply(console, args);
                            if (window.chrome && window.chrome.webview) {
                                window.chrome.webview.postMessage({
                                    type: 'console-log',
                                    level: 'error',
                                    message: args.join(' ')
                                });
                            }
                        };
                        
                        console.warn = function(...args) {
                            originalWarn.apply(console, args);
                            if (window.chrome && window.chrome.webview) {
                                window.chrome.webview.postMessage({
                                    type: 'console-log',
                                    level: 'warn',
                                    message: args.join(' ')
                                });
                            }
                        };
                        
                        console.log('JavaScript error handlers installed');
                    ");
                };
                
                _loggingService.LogInformation("MapPanel: Navigation successful, waiting for ready message from map...");
            }
            else
            {
                _loggingService.LogError("MapPanel: Navigation failed");
                ShowFallbackMap("Failed to load map");
            }
        }




        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // Use WebMessageAsJson instead of TryGetWebMessageAsString to avoid WebView2 issues
                var messageString = e.WebMessageAsJson;
                
                //_loggingService.LogInformation("MapPanel: Received WebView2 message: {Message}", messageString);
                
                if (string.IsNullOrEmpty(messageString))
                {
                    return;
                }
                
                var message = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(messageString);
                
                switch (message?["type"]?.ToString())
                {
                    case "ready":
                        _loggingService.LogInformation("MapPanel: Received ready message from map");
                        _isMapLoaded = true;
                        MapLoadingOverlay.Visibility = Visibility.Collapsed;
                        await LoadMapData();
                        break;
                        
                    case "coordinates":
                        var latElement = (System.Text.Json.JsonElement)message["lat"];
                        var lngElement = (System.Text.Json.JsonElement)message["lng"];
                        var lat = latElement.GetString();
                        var lng = lngElement.GetString();
                        CoordinatesText.Text = $"Lat: {lat}, Lng: {lng}";
                        break;
                        
                    case "contextmenu":
                        // Only show context menu if not disabled
                        if (!DisableContextMenu)
                        {
                            var latCtxElement = (System.Text.Json.JsonElement)message["lat"];
                            var lngCtxElement = (System.Text.Json.JsonElement)message["lng"];
                            var xCtxElement = (System.Text.Json.JsonElement)message["x"];
                            var yCtxElement = (System.Text.Json.JsonElement)message["y"];
                            ShowContextMenu(
                                latCtxElement.GetDouble(), 
                                lngCtxElement.GetDouble(),
                                xCtxElement.GetDouble(), 
                                yCtxElement.GetDouble());
                        }
                        break;
                        
                    case "click":
                        HideContextMenu(null, null);
                        
                        // If left-click location selection is enabled, trigger LocationSelected event
                        if (EnableLeftClickLocationSelection && message.ContainsKey("lat") && message.ContainsKey("lng"))
                        {
                            var latClickElement = (System.Text.Json.JsonElement)message["lat"];
                            var lngClickElement = (System.Text.Json.JsonElement)message["lng"];
                            var clickLat = latClickElement.GetDouble();
                            var clickLng = lngClickElement.GetDouble();
                            
                            // Add a visual pin marker at the clicked location
                            await AddLocationPin(clickLat, clickLng);
                            
                            var clickPoint = new Point(clickLat, clickLng);
                            LocationSelected?.Invoke(this, clickPoint);
                        }
                        break;
                        
                    case "incident-click":
                        var incidentIdElement = (System.Text.Json.JsonElement)message["id"];
                        var incidentIdString = incidentIdElement.GetString();
                        var incidentId = int.Parse(incidentIdString);
                        var incident = _incidents.FirstOrDefault(i => i.Id == incidentId);
                        if (incident != null)
                        {
                            IncidentClicked?.Invoke(this, incident);
                        }
                        break;
                        
                    case "view-incident-details":
                        var detailsIncidentIdElement = (System.Text.Json.JsonElement)message["incidentId"];
                        var detailsIncidentIdString = detailsIncidentIdElement.GetString();
                        var detailsIncidentId = int.Parse(detailsIncidentIdString);
                        var detailsIncident = _incidents.FirstOrDefault(i => i.Id == detailsIncidentId);
                        if (detailsIncident != null)
                        {
                            IncidentClicked?.Invoke(this, detailsIncident);
                        }
                        break;
                        
                    case "test-message":
                        // Test message - no action needed
                        break;
                        
                    case "javascript-error":
                        var errorMessageElement = (System.Text.Json.JsonElement)message["message"];
                        var stackElement = (System.Text.Json.JsonElement)message["stack"];
                        var filenameElement = (System.Text.Json.JsonElement)message["filename"];
                        var linenoElement = (System.Text.Json.JsonElement)message["lineno"];
                        var colnoElement = (System.Text.Json.JsonElement)message["colno"];
                        
                        var errorMessage = errorMessageElement.GetString();
                        var stack = stackElement.GetString();
                        var filename = filenameElement.GetString();
                        var lineno = linenoElement.GetString();
                        var colno = colnoElement.GetString();
                        
                        // Log JavaScript errors
                        _loggingService.LogError($"JavaScript Error: {errorMessage} at {filename}:{lineno}:{colno}");
                        break;
                        
                    case "console-log":
                        var levelElement = (System.Text.Json.JsonElement)message["level"];
                        var logMessageElement = (System.Text.Json.JsonElement)message["message"];
                        var level = levelElement.GetString();
                        var logMessage = logMessageElement.GetString();
                        
                        // Log JavaScript console messages based on level
                        switch (level?.ToLower())
                        {
                            case "error":
                                _loggingService.LogError($"JavaScript: {logMessage}");
                                break;
                            case "warn":
                                _loggingService.LogWarning($"JavaScript: {logMessage}");
                                break;
                            default:
                                _loggingService.LogDebug($"JavaScript: {logMessage}");
                                break;
                        }
                        break;
                        
                    case "map-error":
                        var mapErrorElement = (System.Text.Json.JsonElement)message["error"];
                        var mapError = mapErrorElement.GetString();
                        _loggingService.LogError($"Map initialization error: {mapError}");
                        
                        // Show fallback map on error
                        ShowFallbackMap($"Map failed to load: {mapError}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, $"MapPanel: Error handling map message: {ex.Message}");
            }
        }

        private async Task LoadMapData()
        {
            if (!_isMapLoaded) 
            {
                Console.WriteLine("MapPanel: Map not loaded yet, skipping data load");
                await MapWebView.CoreWebView2.ExecuteScriptAsync("debug('Map not loaded yet, skipping data load');");
                return;
            }

            try
            {
                Console.WriteLine("MapPanel: Starting to load map data...");
                _loggingService.LogInformation("MapPanel: LoadMapData started");
                await MapWebView.CoreWebView2.ExecuteScriptAsync("debug('Starting to load map data...');");
                
                // Load incidents (only active ones for map display)
                Console.WriteLine("MapPanel: Loading incidents...");
                await MapWebView.CoreWebView2.ExecuteScriptAsync("debug('Loading incidents...');");
                var incidents = await _apiService.GetIncidentsAsync();
                _incidents.Clear();
                _incidents.AddRange(incidents.Where(i => 
                    i.Status != IncidentStatus.FullyControlled && 
                    i.Status != IncidentStatus.Closed));
                Console.WriteLine($"MapPanel: Loaded {_incidents.Count} active incidents");
                await MapWebView.CoreWebView2.ExecuteScriptAsync($"debug('Loaded {_incidents.Count} active incidents');");

                // Load stations (filtered by agency if needed)
                Console.WriteLine("MapPanel: Loading stations...");
                var stations = await _apiService.GetStationsAsync();
                _stations.Clear();
                _stations.AddRange(stations);
                Console.WriteLine($"MapPanel: Loaded {_stations.Count} stations");

                // Load vehicles (with location data)
                Console.WriteLine("MapPanel: Loading vehicles...");
                var vehicles = await _apiService.GetVehiclesAsync();
                _vehicles.Clear();
                _vehicles.AddRange(vehicles.Where(v => v.Latitude.HasValue && v.Longitude.HasValue));
                Console.WriteLine($"MapPanel: Loaded {_vehicles.Count} vehicles with GPS");

                // Load agency-specific data based on user's agency
                if (_currentUser != null)
                {
                    Console.WriteLine($"MapPanel: Loading agency-specific data for {_currentUser.AgencyName}");
                    _loggingService.LogInformation("MapPanel: Loading agency-specific data for {AgencyName}", _currentUser.AgencyName);
                    switch (_currentUser.AgencyName)
                    {
                        case "Hellenic Fire Service":
                            _loggingService.LogInformation("MapPanel: Calling LoadFireServiceData");
                            await LoadFireServiceData();
                            break;
                            
                        case "Hellenic Police":
                            await LoadPoliceData();
                            break;
                            
                        case "Hellenic Coast Guard":
                            await LoadCoastGuardData();
                            break;
                            
                        case "EKAB":
                            await LoadEKABData();
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("MapPanel: No current user set");
                }

                // Update map markers
                Console.WriteLine("MapPanel: Updating map markers...");
                await MapWebView.CoreWebView2.ExecuteScriptAsync("debug('Updating map markers...');");
                await UpdateMapMarkers();
                Console.WriteLine("MapPanel: Map data loading completed");
                await MapWebView.CoreWebView2.ExecuteScriptAsync("debug('Map data loading completed');");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MapPanel: Error loading map data: {ex.Message}");
                Console.WriteLine($"MapPanel: Stack trace: {ex.StackTrace}");
            }
        }

        private async Task LoadFireServiceData()
        {
            try
            {
                Console.WriteLine("MapPanel: Loading Fire Service specific data...");
                _loggingService.LogInformation("MapPanel: LoadFireServiceData started");
                
                // Only load fire hydrants if user is from Fire Service and filter is enabled
                _loggingService.LogInformation("MapPanel: Checking fire hydrants conditions - User: {User}, Agency: {Agency}, Filter: {Filter}", 
                    _currentUser?.Name, _currentUser?.AgencyName, _currentFilters.FireHydrants);
                
                if (_currentUser?.AgencyName == "Hellenic Fire Service" && _currentFilters.FireHydrants)
                {
                    Console.WriteLine("MapPanel: Loading fire hydrants...");
                    _loggingService.LogInformation("MapPanel: Starting fire hydrants API call");
                    try
                    {
                        var fireHydrants = await _apiService.GetFireHydrantsAsync();
                        _fireHydrants.Clear();
                        _fireHydrants.AddRange(fireHydrants);
                        Console.WriteLine($"MapPanel: Loaded {_fireHydrants.Count} fire hydrants");
                        _loggingService.LogInformation("MapPanel: Successfully loaded {Count} fire hydrants", _fireHydrants.Count);
                    }
                    catch (Exception hydrantEx)
                    {
                        Console.WriteLine($"MapPanel: Failed to load fire hydrants: {hydrantEx.Message}");
                        _loggingService.LogError(hydrantEx, "Failed to load fire hydrants: {Message}", hydrantEx.Message);
                        // Clear fire hydrants if loading fails
                        _fireHydrants.Clear();
                    }
                }
                else
                {
                    Console.WriteLine("MapPanel: Fire hydrants not enabled for display or user not authorized");
                    _loggingService.LogInformation("MapPanel: Fire hydrants skipped - conditions not met");
                    _fireHydrants.Clear();
                }

                // Load fire station boundaries if user is from Fire Service and filter is enabled
                _loggingService.LogInformation("MapPanel: Checking boundaries conditions - User: {User}, Agency: {Agency}, Filter: {Filter}", 
                    _currentUser?.Name, _currentUser?.AgencyName, _currentFilters.FireStationBoundaries);
                
                if (_currentUser?.AgencyName == "Hellenic Fire Service" && _currentFilters.FireStationBoundaries)
                {
                    Console.WriteLine("MapPanel: Loading fire station boundaries...");
                    _loggingService.LogInformation("MapPanel: Starting fire station boundaries API call");
                    try
                    {
                        var boundaries = await _apiService.GetFireStationBoundariesAsync();
                        _fireStationBoundaries.Clear();
                        _fireStationBoundaries.AddRange(boundaries);
                        Console.WriteLine($"MapPanel: Loaded {_fireStationBoundaries.Count} fire station boundaries");
                        _loggingService.LogInformation("MapPanel: Successfully loaded {Count} fire station boundaries", _fireStationBoundaries.Count);
                    }
                    catch (Exception boundaryEx)
                    {
                        Console.WriteLine($"MapPanel: Failed to load fire station boundaries: {boundaryEx.Message}");
                        _loggingService.LogError(boundaryEx, "Failed to load fire station boundaries: {Message}", boundaryEx.Message);
                        // Clear boundaries if loading fails
                        _fireStationBoundaries.Clear();
                    }
                }
                else
                {
                    Console.WriteLine("MapPanel: Fire station boundaries not enabled for display or user not authorized");
                    _loggingService.LogInformation("MapPanel: Fire station boundaries skipped - conditions not met");
                    _fireStationBoundaries.Clear();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MapPanel: Error loading fire service data: {ex.Message}");
                _loggingService.LogError(ex, "Error loading fire service data");
                // Don't fail the entire map load if fire hydrants fail
            }
        }

        private async Task LoadPoliceData()
        {
            try
            {
                if (_currentFilters.PoliceStations)
                {
                    var policeStations = await _apiService.GetPoliceStationsAsync();
                    _policeStations.Clear();
                    _policeStations.AddRange(policeStations);
                }

                if (_currentFilters.PatrolZones && _currentUser != null)
                {
                    var patrolZones = await _apiService.GetPatrolZonesAsync(_currentUser.AgencyId);
                    _patrolZones.Clear();
                    _patrolZones.AddRange(patrolZones);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading police data: {ex.Message}");
            }
        }

        private async Task LoadCoastGuardData()
        {
            try
            {
                if (_currentFilters.CoastGuardStations)
                {
                    var coastGuardStations = await _apiService.GetCoastGuardStationsAsync();
                    _coastGuardStations.Clear();
                    _coastGuardStations.AddRange(coastGuardStations);
                }

                if (_currentFilters.Ships)
                {
                    var ships = await _apiService.GetShipsAsync();
                    _ships.Clear();
                    _ships.AddRange(ships);
                }

                if (_currentFilters.PatrolZones && _currentUser != null)
                {
                    var patrolZones = await _apiService.GetPatrolZonesAsync(_currentUser.AgencyId);
                    _patrolZones.Clear();
                    _patrolZones.AddRange(patrolZones);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading coast guard data: {ex.Message}");
            }
        }

        private async Task LoadEKABData()
        {
            try
            {
                if (_currentFilters.Hospitals)
                {
                    var hospitals = await _apiService.GetHospitalsAsync();
                    _hospitals.Clear();
                    _hospitals.AddRange(hospitals);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading EKAB data: {ex.Message}");
            }
        }

        private async Task UpdateMapMarkers()
        {
            if (!_isMapLoaded) 
            {
                Console.WriteLine("MapPanel: Map not loaded, skipping marker update");
                return;
            }

            try
            {
                Console.WriteLine("MapPanel: Updating map markers...");
                // Clear existing markers
                await MapWebView.CoreWebView2.ExecuteScriptAsync("clearMarkers();");
                Console.WriteLine("MapPanel: Cleared existing markers");

                // Skip adding other markers if HideOtherMarkers is enabled (used in NewIncidentWindow)
                if (HideOtherMarkers)
                {
                    Console.WriteLine("MapPanel: HideOtherMarkers is enabled, skipping all markers except location pins");
                    return;
                }

                // Add incident markers if layer is visible
                if (_currentFilters.Incidents)
                {
                    Console.WriteLine($"MapPanel: Adding {_incidents.Count} incident markers");
                    foreach (var incident in _incidents)
                    {
                        if (incident.Latitude != 0 && incident.Longitude != 0)
                        {
                            Console.WriteLine($"MapPanel: Adding incident {incident.Id} at {incident.Latitude}, {incident.Longitude}");
                            var statusBadge = GetStatusBadgeClass(incident.Status.ToString());
                            var priorityClass = GetPriorityClass(incident.Priority.ToString());
                            
                            // Debug: Log all assignments first
                            if (incident.Assignments != null)
                            {
                                Console.WriteLine($"MapPanel: Incident {incident.Id} - All assignments:");
                                foreach (var assignment in incident.Assignments)
                                {
                                    Console.WriteLine($"  Assignment ID: {assignment.Id}, ResourceType: {assignment.ResourceType}, ResourceId: {assignment.ResourceId}, Status: '{assignment.Status}'");
                                }
                            }
                            
                            // Filter out finished vehicles from assigned resources
                            var assignedVehicles = incident.Assignments?.Where(a => 
                                a.ResourceType == ResourceType.Vehicle && 
                                a.Status != "Finished" && 
                                a.Status != "Completed").ToList() ?? new List<Assignment>();
                            
                            Console.WriteLine($"MapPanel: Incident {incident.Id} has {incident.Assignments?.Count ?? 0} total assignments, {assignedVehicles.Count} active vehicle assignments");
                            
                            // Debug: Log filtered assignments
                            Console.WriteLine($"MapPanel: Incident {incident.Id} - Filtered vehicle assignments:");
                            foreach (var assignment in assignedVehicles)
                            {
                                Console.WriteLine($"  Filtered Assignment ID: {assignment.Id}, ResourceId: {assignment.ResourceId}, Status: '{assignment.Status}'");
                            }
                            
                            var assignedVehiclesHtml = "";
                            if (assignedVehicles.Any())
                            {
                                assignedVehiclesHtml = "<div style='margin: 8px 0;'><div style='display: flex; align-items: center; margin-bottom: 4px;'><span style='font-size: 12px; font-weight: 600; color: #374151;'>Assigned Resources:</span></div>";
                                foreach (var assignment in assignedVehicles)
                                {
                                    var assignedVehicle = _vehicles.FirstOrDefault(v => v.Id == assignment.ResourceId);
                                    if (assignedVehicle != null)
                                    {
                                        Console.WriteLine($"MapPanel: Adding assigned vehicle {assignedVehicle.Callsign} (Status: {assignment.Status}) to incident {incident.Id}");
                                        assignedVehiclesHtml += $"<div style='font-size: 11px; color: #4B5563; background: #EBF8FF; padding: 4px 8px; border-radius: 4px; margin: 2px 0;'>{EscapeHtml(assignedVehicle.Callsign)} - {EscapeHtml(assignedVehicle.Type)} ({EscapeHtml(assignment.Status)})</div>";
                                    }
                                    else
                                    {
                                        Console.WriteLine($"MapPanel: Could not find vehicle with ID {assignment.ResourceId} for incident {incident.Id}");
                                    }
                                }
                                assignedVehiclesHtml += "</div>";
                            }
                            else
                            {
                                Console.WriteLine($"MapPanel: No active vehicle assignments found for incident {incident.Id}");
                            }

                            var popupContent = $@"
                                <div style='padding: 8px; background: white; color: #111827; min-width: 200px;'>
                                    <h3 style='font-weight: 600; color: #111827; margin: 0 0 8px 0; font-size: 14px;'>{EscapeHtml(incident.MainCategory)} - {EscapeHtml(incident.SubCategory)}</h3>
                                    <p style='font-size: 12px; color: #6B7280; margin: 4px 0 8px 0;'>{(string.IsNullOrEmpty(incident.Notes) ? "No notes" : EscapeHtml(incident.Notes))}</p>
                                    {assignedVehiclesHtml}
                                    <div style='display: flex; align-items: center; justify-content: space-between; margin-top: 8px;'>
                                        <span style='padding: 2px 8px; font-size: 10px; font-weight: 600; border-radius: 12px; {GetStatusBadgeStyle(incident.Status.ToString())}'>{GetIncidentStatusTranslation(incident.Status.ToString())}</span>
                                        <span style='font-size: 10px; color: #9CA3AF;'>ID: {incident.Id}</span>
                                    </div>
                                </div>";
                            
                            // Get the icon path using the IncidentIconService
                            var iconPath = IncidentIconService.GetIncidentIconPath(
                                incident.MainCategory, 
                                incident.Status,
                                _currentUser?.AgencyName
                            );
                            
                            var iconData = $"{{status: '{incident.Status}', priority: '{incident.Priority}', mainCategory: '{EscapeHtml(incident.MainCategory)}', userAgency: '{EscapeHtml(_currentUser?.AgencyName ?? "")}'}}";
                            var script = $"addMarker('incident_{incident.Id}', {incident.Latitude.ToString(CultureInfo.InvariantCulture)}, {incident.Longitude.ToString(CultureInfo.InvariantCulture)}, 'incident', '{EscapeHtml(incident.MainCategory)}', `{popupContent}`, {iconData}, '{iconPath}');";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                        else
                        {
                            Console.WriteLine($"MapPanel: Skipping incident {incident.Id} - no coordinates ({incident.Latitude}, {incident.Longitude})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("MapPanel: Incidents layer is disabled");
                }

                // Add fire station markers if layer is visible and user is from fire service
                if (_currentFilters.FireStations && _currentUser?.AgencyName == "Hellenic Fire Service")
                {
                    foreach (var station in _stations.Where(s => s.AgencyType == AgencyType.FireDepartment))
                    {
                        if (station.Latitude != 0 && station.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>{EscapeHtml(station.Name)}</h3>
                                <p><strong>Type:</strong> Fire Station</p>
                                <p><strong>Location:</strong> {station.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {station.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('fire-station_{station.Id}', {station.Latitude.ToString(CultureInfo.InvariantCulture)}, {station.Longitude.ToString(CultureInfo.InvariantCulture)}, 'fire-station', '{EscapeHtml(station.Name)}', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add police station markers
                if (_currentFilters.PoliceStations)
                {
                    foreach (var station in _policeStations)
                    {
                        if (station.Latitude != 0 && station.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>{EscapeHtml(station.Name)}</h3>
                                <p><strong>Type:</strong> Police Station</p>
                                {(string.IsNullOrEmpty(station.Address) ? "" : $"<p><strong>Address:</strong> {EscapeHtml(station.Address)}</p>")}
                                {(string.IsNullOrEmpty(station.Sinoikia) ? "" : $"<p><strong>District:</strong> {EscapeHtml(station.Sinoikia)}</p>")}
                                <p><strong>Location:</strong> {station.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {station.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('police-station_{station.Id}', {station.Latitude.ToString(CultureInfo.InvariantCulture)}, {station.Longitude.ToString(CultureInfo.InvariantCulture)}, 'police-station', '{EscapeHtml(station.Name)}', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add coast guard station markers
                if (_currentFilters.CoastGuardStations)
                {
                    foreach (var station in _coastGuardStations)
                    {
                        if (station.Latitude != 0 && station.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>{EscapeHtml(station.Name)}</h3>
                                <p><strong>Type:</strong> Coast Guard Station</p>
                                {(string.IsNullOrEmpty(station.Address) ? "" : $"<p><strong>Address:</strong> {EscapeHtml(station.Address)}</p>")}
                                {(string.IsNullOrEmpty(station.Area) ? "" : $"<p><strong>Area:</strong> {EscapeHtml(station.Area)}</p>")}
                                <p><strong>Location:</strong> {station.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {station.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('coast-guard-station_{station.Id}', {station.Latitude.ToString(CultureInfo.InvariantCulture)}, {station.Longitude.ToString(CultureInfo.InvariantCulture)}, 'coast-guard-station', '{EscapeHtml(station.Name)}', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add hospital markers
                if (_currentFilters.Hospitals)
                {
                    foreach (var hospital in _hospitals)
                    {
                        if (hospital.Latitude != 0 && hospital.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>{EscapeHtml(hospital.Name)}</h3>
                                <p><strong>Type:</strong> Hospital</p>
                                {(string.IsNullOrEmpty(hospital.Address) ? "" : $"<p><strong>Address:</strong> {EscapeHtml(hospital.Address)}</p>")}
                                {(string.IsNullOrEmpty(hospital.City) ? "" : $"<p><strong>City:</strong> {EscapeHtml(hospital.City)}</p>")}
                                <p><strong>Location:</strong> {hospital.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {hospital.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('hospital_{hospital.Id}', {hospital.Latitude.ToString(CultureInfo.InvariantCulture)}, {hospital.Longitude.ToString(CultureInfo.InvariantCulture)}, 'hospital', '{EscapeHtml(hospital.Name)}', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add vehicle markers if layer is visible
                if (_currentFilters.Vehicles)
                {
                    foreach (var vehicle in _vehicles)
                    {
                        if (vehicle.Latitude.HasValue && vehicle.Longitude.HasValue)
                        {
                            var statusBadge = GetVehicleStatusBadgeClass(vehicle.Status.ToString());
                            var popupContent = $@"
                                <h3>{EscapeHtml(vehicle.Callsign)}</h3>
                                <p><strong>Type:</strong> {EscapeHtml(vehicle.Type)}</p>
                                <p><strong>Status:</strong> <span class='{statusBadge}'>{vehicle.Status}</span></p>
                                {(vehicle.FuelLevelPercent.HasValue ? $"<p><strong>Fuel:</strong> {vehicle.FuelLevelPercent}%</p>" : "")}
                                <p><strong>Location:</strong> {vehicle.Latitude.Value.ToString("F4", CultureInfo.InvariantCulture)}, {vehicle.Longitude.Value.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var iconData = $"{{status: '{vehicle.Status}', agency: '{EscapeHtml(_currentUser?.AgencyName ?? "")}'}}";
                            var script = $"addMarker('vehicle_{vehicle.Id}', {vehicle.Latitude.Value.ToString(CultureInfo.InvariantCulture)}, {vehicle.Longitude.Value.ToString(CultureInfo.InvariantCulture)}, 'vehicle', '{EscapeHtml(vehicle.Callsign)}', `{popupContent}`, {iconData});";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add ship markers
                if (_currentFilters.Ships)
                {
                    foreach (var ship in _ships)
                    {
                        if (ship.Latitude != 0 && ship.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>{EscapeHtml(ship.Name ?? $"MMSI: {ship.Mmsi}")}</h3>
                                <p><strong>MMSI:</strong> {ship.Mmsi}</p>
                                {(ship.Speed.HasValue ? $"<p><strong>Speed:</strong> {ship.Speed:F1} knots</p>" : "")}
                                <p><strong>Last Update:</strong> {ship.LastUpdate:HH:mm:ss}</p>
                                <p><strong>Location:</strong> {ship.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {ship.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('ship_{ship.Mmsi}', {ship.Latitude.ToString(CultureInfo.InvariantCulture)}, {ship.Longitude.ToString(CultureInfo.InvariantCulture)}, 'ship', '{EscapeHtml(ship.Name ?? ship.Mmsi.ToString())}', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add fire hydrant markers
                if (_currentFilters.FireHydrants)
                {
                    foreach (var hydrant in _fireHydrants)
                    {
                        if (hydrant.Latitude != 0 && hydrant.Longitude != 0)
                        {
                            var popupContent = $@"
                                <h3>Fire Hydrant</h3>
                                {(string.IsNullOrEmpty(hydrant.Address) ? "" : $"<p><strong>Address:</strong> {EscapeHtml(hydrant.Address)}</p>")}
                                {(hydrant.Pressure.HasValue ? $"<p><strong>Pressure:</strong> {hydrant.Pressure} PSI</p>" : "")}
                                <p><strong>Location:</strong> {hydrant.Latitude.ToString("F4", CultureInfo.InvariantCulture)}, {hydrant.Longitude.ToString("F4", CultureInfo.InvariantCulture)}</p>";
                            
                            var script = $"addMarker('fire-hydrant_{hydrant.Id}', {hydrant.Latitude.ToString(CultureInfo.InvariantCulture)}, {hydrant.Longitude.ToString(CultureInfo.InvariantCulture)}, 'fire-hydrant', 'Fire Hydrant', `{popupContent}`);";
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }

                // Add fire station boundary polygons
                if (_currentFilters.FireStationBoundaries)
                {
                    foreach (var boundary in _fireStationBoundaries)
                    {
                        if (boundary.Coordinates != null && boundary.Coordinates.Any())
                        {
                            var popupContent = $@"
                                <h3>Fire Station District</h3>
                                <p><strong>Name:</strong> {EscapeHtml(boundary.Name)}</p>
                                <p><strong>Region:</strong> {EscapeHtml(boundary.Region)}</p>
                                <p><strong>Area:</strong> {boundary.Area:F2} km²</p>";
                            
                            // Convert coordinates to JavaScript array format
                            var coordinatesJson = JsonSerializer.Serialize(boundary.Coordinates, _jsonOptions);
                            
                            var script = $@"
                                addPolygon('fire-station-boundary_{boundary.Id}', {coordinatesJson}, 'fire-station-boundary', 'Fire Station District', `{popupContent}`, {{
                                    color: '#FF6B35',
                                    weight: 2,
                                    opacity: 0.8,
                                    fillColor: '#FF6B35',
                                    fillOpacity: 0.2
                                }});";
                            
                            await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating map markers: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a visual pin marker at the specified location for location selection feedback
        /// </summary>
        private async Task AddLocationPin(double lat, double lng)
        {
            try
            {
                if (_isMapLoaded)
                {
                    // Remove any existing location pin first
                    await MapWebView.CoreWebView2.ExecuteScriptAsync("removeMarker('location-pin');");
                    
                    // Add a new location pin marker
                    var popupContent = $@"
                        <div style='text-align: center;'>
                            <h3>📍 Selected Location</h3>
                            <p><strong>Coordinates:</strong><br>{lat.ToString("F6", CultureInfo.InvariantCulture)}, {lng.ToString("F6", CultureInfo.InvariantCulture)}</p>
                        </div>";
                    
                    var script = $"addMarker('location-pin', {lat.ToString(CultureInfo.InvariantCulture)}, {lng.ToString(CultureInfo.InvariantCulture)}, 'location-pin', 'Selected Location', `{popupContent}`);";
                    await MapWebView.CoreWebView2.ExecuteScriptAsync(script);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error adding location pin: {ex.Message}");
            }
        }

        private string EscapeHtml(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            return input.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&#39;")
                       .Replace("`", "&#96;");
        }

        private string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "Created" => "status-created",
                "OnGoing" => "status-ongoing",
                "PartialControl" => "status-ongoing",
                "FullyControlled" => "status-controlled",
                "Closed" => "status-closed",
                _ => "status-created"
            };
        }

        private string GetStatusBadgeStyle(string status)
        {
            return status switch
            {
                "Created" => "background: #FEE2E2; color: #991B1B;",
                "OnGoing" => "background: #FED7AA; color: #9A3412;",
                "PartialControl" => "background: #FEF3C7; color: #92400E;",
                "FullyControlled" => "background: #D1FAE5; color: #065F46;",
                "Closed" => "background: #F3F4F6; color: #374151;",
                _ => "background: #FEE2E2; color: #991B1B;"
            };
        }

        private string GetIncidentStatusTranslation(string status)
        {
            return status switch
            {
                "Created" => "Created",
                "OnGoing" => "Ongoing",
                "PartialControl" => "Partial Control",
                "Controlled" => "Controlled",
                "FullyControlled" => "Fully Controlled",
                "Closed" => "Closed",
                _ => "Unknown"
            };
        }

        private string GetPriorityClass(string priority)
        {
            return priority switch
            {
                "Critical" => "priority-critical",
                "High" => "priority-high",
                "Normal" => "priority-normal",
                "Low" => "priority-low",
                _ => "priority-normal"
            };
        }

        private string GetVehicleStatusBadgeClass(string status)
        {
            return status switch
            {
                "Available" => "status-controlled",
                "Notified" => "status-ongoing",
                "EnRoute" => "status-ongoing",
                "OnScene" => "status-created",
                "Busy" => "status-created",
                "Maintenance" => "status-closed",
                "Offline" => "status-closed",
                _ => "status-controlled"
            };
        }

        public async Task RefreshMapData()
        {
            await LoadMapData();
        }

        // Test method to directly load fire service data
        public async Task TestFireServiceDataLoading()
        {
            try
            {
                _loggingService.LogInformation("MapPanel: TestFireServiceDataLoading started");
                
                // Set a test user
                _currentUser = new User 
                { 
                    AgencyName = "Hellenic Fire Service",
                    Name = "Test Fire User"
                };
                
                // Enable fire hydrants filter
                _currentFilters.FireHydrants = true;
                _currentFilters.FireStationBoundaries = true;
                
                _loggingService.LogInformation("MapPanel: Test user set, calling LoadFireServiceData");
                await LoadFireServiceData();
                _loggingService.LogInformation("MapPanel: TestFireServiceDataLoading completed");
                
                // Also test direct API call
                _loggingService.LogInformation("MapPanel: Testing direct API call to fire hydrants");
                try
                {
                    var directResult = await _apiService.GetFireHydrantsAsync();
                    _loggingService.LogInformation("MapPanel: Direct API call returned {Count} fire hydrants", directResult.Count);
                }
                catch (Exception apiEx)
                {
                    _loggingService.LogError(apiEx, "MapPanel: Direct API call failed: {Message}", apiEx.Message);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "MapPanel: TestFireServiceDataLoading failed: {Message}", ex.Message);
            }
        }

        public async Task FocusOnIncident(Incident incident)
        {
            if (!_isMapLoaded || incident == null) return;

            try
            {
                // Focus the map on the incident location
                if (incident.Latitude != 0 && incident.Longitude != 0)
                {
                    await MapWebView.CoreWebView2.ExecuteScriptAsync(
                        $"setView({incident.Latitude.ToString(CultureInfo.InvariantCulture)}, {incident.Longitude.ToString(CultureInfo.InvariantCulture)}, 15);");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MapPanel: Error focusing on incident {incident.Id}: {ex.Message}");
            }
        }

        public async Task HighlightIncident(Incident incident)
        {
            if (!_isMapLoaded || incident == null) return;

            try
            {
                // First focus on the incident
                await FocusOnIncident(incident);
                
                // Then highlight the incident marker by opening its popup
                await MapWebView.CoreWebView2.ExecuteScriptAsync(
                    $"if (markers['incident_{incident.Id}']) {{ markers['incident_{incident.Id}'].openPopup(); }}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MapPanel: Error highlighting incident {incident.Id}: {ex.Message}");
            }
        }



        private void ShowFallbackMap(string errorMessage)
        {
            MapLoadingOverlay.Visibility = Visibility.Collapsed;
            FallbackMap.Visibility = Visibility.Visible;
            
            if (FallbackMap.Children[0] is StackPanel panel && 
                panel.Children[2] is TextBlock errorBlock)
            {
                errorBlock.Text = errorMessage;
            }
        }

        private void ShowContextMenu(double lat, double lng, double x, double y)
        {
            _lastRightClickPoint = new Point(lat, lng);
            
            ContextCoordinates.Text = $"Lat: {lat:F3}, Lng: {lng:F3}";
            
            // Position popup at the click coordinates
            // Convert relative coordinates to screen coordinates
            var mapPosition = MapWebView.PointToScreen(new Point(x, y));
            ContextMenuPopup.HorizontalOffset = mapPosition.X;
            ContextMenuPopup.VerticalOffset = mapPosition.Y;
            
            // Show the popup
            ContextMenuPopup.IsOpen = true;
        }

        private void HideContextMenu(object? sender, MouseButtonEventArgs? e)
        {
            ContextMenuPopup.IsOpen = false;
        }

        #region Button Event Handlers

        private async void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (_isMapLoaded)
            {
                await MapWebView.CoreWebView2.ExecuteScriptAsync("map.zoomIn();");
            }
        }

        private async void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (_isMapLoaded)
            {
                await MapWebView.CoreWebView2.ExecuteScriptAsync("map.zoomOut();");
            }
        }



        private async void CenterOnIncidents_Click(object sender, RoutedEventArgs e)
        {
            if (_isMapLoaded && _incidents.Any())
            {
                var incidentsWithLocation = _incidents.Where(i => i.Latitude != 0 && i.Longitude != 0).ToList();
                if (incidentsWithLocation.Any())
                {
                    var avgLat = incidentsWithLocation.Average(i => i.Latitude);
                    var avgLng = incidentsWithLocation.Average(i => i.Longitude);
                    await MapWebView.CoreWebView2.ExecuteScriptAsync($"setView({avgLat.ToString(CultureInfo.InvariantCulture)}, {avgLng.ToString(CultureInfo.InvariantCulture)}, 12);");
                }
            }
        }

        private async void ShowMyLocation_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for showing user's location
            // This would typically use geolocation APIs
        }

        private async void RetryMap_Click(object sender, RoutedEventArgs e)
        {
            FallbackMap.Visibility = Visibility.Collapsed;
            MapLoadingOverlay.Visibility = Visibility.Visible;
            InitializeMap();
        }

        private void ToggleLegend_Click(object sender, RoutedEventArgs e)
        {
            _isLegendVisible = !_isLegendVisible;
            MapLegend.Visibility = _isLegendVisible ? Visibility.Visible : Visibility.Collapsed;
            ToggleLegendButton.Content = _isLegendVisible ? "Hide Legend" : "Show Legend";
        }

        private void AddIncidentHere_Click(object sender, RoutedEventArgs e)
        {
            LocationSelected?.Invoke(this, _lastRightClickPoint);
            HideContextMenu(null, null);
        }

        private void AssignNearestUnit_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for assigning nearest unit
            HideContextMenu(null, null);
        }

        private void ToggleResources_Click(object sender, RoutedEventArgs e)
        {
            _isResourcesVisible = !_isResourcesVisible;
            ResourcesPanel.Visibility = _isResourcesVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void ResourceToggle_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;
            if (_isUpdatingFilters) return; // Prevent recursive updates

            var isChecked = checkBox.IsChecked ?? false;
            
            // Update the current filters based on which checkbox was changed
            switch (checkBox.Name)
            {
                case nameof(ShowIncidentsToggle):
                    _currentFilters.Incidents = isChecked;
                    ApplyFilterToMap(MapFilterType.Incidents, isChecked);
                    break;
                case nameof(ShowVehiclesToggle):
                    _currentFilters.Vehicles = isChecked;
                    ApplyFilterToMap(MapFilterType.Vehicles, isChecked);
                    break;
                case nameof(ShowFireStationsToggle):
                    _currentFilters.FireStations = isChecked;
                    ApplyFilterToMap(MapFilterType.FireStations, isChecked);
                    break;
                case nameof(ShowFireHydrantsToggle):
                    _currentFilters.FireHydrants = isChecked;
                    ApplyFilterToMap(MapFilterType.FireHydrants, isChecked);
                    break;
                case nameof(ShowFireStationBoundariesToggle):
                    _currentFilters.FireStationBoundaries = isChecked;
                    ApplyFilterToMap(MapFilterType.FireStationBoundaries, isChecked);
                    break;
                case nameof(ShowPoliceStationsToggle):
                    _currentFilters.PoliceStations = isChecked;
                    ApplyFilterToMap(MapFilterType.PoliceStations, isChecked);
                    break;
                case nameof(ShowPatrolZonesToggle):
                case nameof(ShowPatrolZonesCoastGuardToggle):
                    _currentFilters.PatrolZones = isChecked;
                    ApplyFilterToMap(MapFilterType.PatrolZones, isChecked);
                    break;
                case nameof(ShowCoastGuardStationsToggle):
                    _currentFilters.CoastGuardStations = isChecked;
                    ApplyFilterToMap(MapFilterType.CoastGuardStations, isChecked);
                    break;
                case nameof(ShowShipsToggle):
                    _currentFilters.Ships = isChecked;
                    ApplyFilterToMap(MapFilterType.Ships, isChecked);
                    break;
                case nameof(ShowHospitalsToggle):
                    _currentFilters.Hospitals = isChecked;
                    ApplyFilterToMap(MapFilterType.Hospitals, isChecked);
                    break;
                case nameof(ShowAmbulancesToggle):
                    _currentFilters.Ambulances = isChecked;
                    ApplyFilterToMap(MapFilterType.Ambulances, isChecked);
                    break;
            }
        }

        private void ApplyFilterToMap(MapFilterType filterType, bool isEnabled)
        {
            if (!_isMapLoaded) return;

            try
            {
                var script = filterType switch
                {
                    MapFilterType.Incidents => $"toggleLayerVisibility('incident', {isEnabled.ToString().ToLower()});",
                    MapFilterType.Vehicles => $"toggleLayerVisibility('vehicle', {isEnabled.ToString().ToLower()});",
                    MapFilterType.FireStations => $"toggleLayerVisibility('fire-station', {isEnabled.ToString().ToLower()});",
                    MapFilterType.FireHydrants => $"toggleLayerVisibility('fire-hydrant', {isEnabled.ToString().ToLower()});",
                    MapFilterType.FireStationBoundaries => $"toggleLayerVisibility('fire-station-boundary', {isEnabled.ToString().ToLower()});",
                    MapFilterType.PoliceStations => $"toggleLayerVisibility('police-station', {isEnabled.ToString().ToLower()});",
                    MapFilterType.PatrolZones => $"toggleLayerVisibility('patrol-zone', {isEnabled.ToString().ToLower()});",
                    MapFilterType.CoastGuardStations => $"toggleLayerVisibility('coast-guard-station', {isEnabled.ToString().ToLower()});",
                    MapFilterType.Ships => $"toggleLayerVisibility('ship', {isEnabled.ToString().ToLower()});",
                    MapFilterType.Hospitals => $"toggleLayerVisibility('hospital', {isEnabled.ToString().ToLower()});",
                    MapFilterType.Ambulances => $"toggleLayerVisibility('ambulance', {isEnabled.ToString().ToLower()});",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(script))
                {
                    MapWebView.ExecuteScriptAsync(script);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error applying filter {filterType}: {ex.Message}");
            }
        }






        #endregion
    }
}
