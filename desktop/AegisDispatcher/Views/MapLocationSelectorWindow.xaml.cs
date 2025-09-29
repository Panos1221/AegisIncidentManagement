using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using AegisDispatcher.Services;

namespace AegisDispatcher.Views
{
    public partial class MapLocationSelectorWindow : Window
    {
        private readonly IGeocodingService _geocodingService;
        private bool _isMapReady = false;
        
        public double? SelectedLatitude { get; private set; }
        public double? SelectedLongitude { get; private set; }
        public string SelectedAddress { get; private set; } = string.Empty;

        public MapLocationSelectorWindow(IGeocodingService geocodingService)
        {
            InitializeComponent();
            _geocodingService = geocodingService;
            InitializeMapAsync();
        }

        private async void InitializeMapAsync()
        {
            try
            {
                await MapWebView.EnsureCoreWebView2Async();
                
                // Set up message handling for map clicks
                MapWebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                
                // Load the map HTML
                var mapHtml = GetMapHtml();
                MapWebView.NavigateToString(mapHtml);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize map: {ex.Message}", "Map Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                DialogResult = false;
                Close();
            }
        }

        private async void MapWebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                _isMapReady = true;
                LoadingOverlay.Visibility = Visibility.Collapsed;
                StatusText.Text = "Map loaded. Click on the map to select a location.";
            }
            else
            {
                StatusText.Text = "Failed to load map. Please try again.";
            }
        }

        private async void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var message = e.TryGetWebMessageAsString();
                if (string.IsNullOrEmpty(message)) return;

                var parts = message.Split(',');
                if (parts.Length >= 2 && 
                    double.TryParse(parts[0], out double lat) && 
                    double.TryParse(parts[1], out double lng))
                {
                    await HandleLocationSelected(lat, lng);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error processing map click: {ex.Message}";
            }
        }

        private async Task HandleLocationSelected(double latitude, double longitude)
        {
            try
            {
                StatusText.Text = "Getting address information...";
                CoordinatesText.Text = $"{latitude:F6}, {longitude:F6}";
                CoordinatesPanel.Visibility = Visibility.Visible;

                // Store the coordinates
                SelectedLatitude = latitude;
                SelectedLongitude = longitude;

                // Try to get address information via reverse geocoding
                try
                {
                    var result = await _geocodingService.ReverseGeocodeAsync(latitude, longitude);
                    if (result != null && !string.IsNullOrEmpty(result.DisplayName))
                    {
                        SelectedAddress = result.DisplayName;
                        StatusText.Text = $"Location selected: {result.DisplayName}";
                    }
                    else
                    {
                        SelectedAddress = $"Location at {latitude:F6}, {longitude:F6}";
                        StatusText.Text = "Location selected (address lookup failed)";
                    }
                }
                catch
                {
                    SelectedAddress = $"Location at {latitude:F6}, {longitude:F6}";
                    StatusText.Text = "Location selected (address lookup unavailable)";
                }

                // Enable the confirm button
                ConfirmButton.IsEnabled = true;

                // Update the map marker
                if (_isMapReady)
                {
                    await MapWebView.CoreWebView2.ExecuteScriptAsync(
                        $"updateMarker({latitude}, {longitude});");
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error handling location selection: {ex.Message}";
            }
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedLatitude.HasValue && SelectedLongitude.HasValue)
            {
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private string GetMapHtml()
        {
            return @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>Location Selector</title>
    <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />
    <style>
        body { margin: 0; padding: 0; font-family: Arial, sans-serif; }
        #map { height: 100vh; width: 100%; cursor: crosshair; }
        .location-marker { 
            background-color: #ff4444; 
            border: 3px solid white; 
            border-radius: 50%; 
            box-shadow: 0 2px 6px rgba(0,0,0,0.3);
        }
    </style>
</head>
<body>
    <div id='map'></div>
    
    <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>
    <script>
        // Initialize map centered on Greece
        var map = L.map('map').setView([39.0742, 21.8243], 7);
        
        // Add OpenStreetMap tiles
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors',
            maxZoom: 19
        }).addTo(map);
        
        var selectedMarker = null;
        
        // Handle map clicks
        map.on('click', function(e) {
            var lat = e.latlng.lat;
            var lng = e.latlng.lng;
            
            // Send coordinates to C# application
            window.chrome.webview.postMessage(lat + ',' + lng);
        });
        
        // Function to update marker position (called from C#)
        function updateMarker(lat, lng) {
            if (selectedMarker) {
                map.removeLayer(selectedMarker);
            }
            
            selectedMarker = L.circleMarker([lat, lng], {
                radius: 10,
                className: 'location-marker',
                fillColor: '#ff4444',
                color: 'white',
                weight: 3,
                opacity: 1,
                fillOpacity: 1
            }).addTo(map);
            
            // Center map on selected location
            map.setView([lat, lng], Math.max(map.getZoom(), 15));
        }
        
        // Add some visual feedback
        map.on('mousemove', function(e) {
            document.getElementById('map').style.cursor = 'crosshair';
        });
    </script>
</body>
</html>";
        }
    }
}