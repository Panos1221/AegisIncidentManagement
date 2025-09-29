using Microsoft.Web.WebView2.WinForms;
using StationConfigurator.Services;
using StationConfigurator.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StationConfigurator.Forms;

public partial class CreateStationForm : Form
{
    private readonly ApiClient _apiClient;
    private TextBox _nameTextBox;
    private ComboBox _agencyComboBox;
    private TextBox _addressTextBox;
    private Button _searchAddressButton;
    private TextBox _latitudeTextBox;
    private TextBox _longitudeTextBox;
    private WebView2 _mapWebView;
    private Button _okButton;
    private Button _cancelButton;
    private List<AgencyDto> _agencies;

    public CreateStationForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _agencies = new List<AgencyDto>();
        InitializeComponent();
        LoadAgencies();
        InitializeMap();
    }

    private void InitializeComponent()
    {
        Text = "Create New Station";
        Size = new Size(900, 700);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(15)
        };
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        // Left panel for form fields
        var leftPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(10)
        };

        var titleLabel = new Label
        {
            Text = "Station Details",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(10, 10),
            Size = new Size(280, 25)
        };

        var nameLabel = new Label
        {
            Text = "Station Name:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 45),
            Size = new Size(100, 20)
        };

        _nameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 70),
            Size = new Size(260, 25)
        };

        var agencyLabel = new Label
        {
            Text = "Agency:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 105),
            Size = new Size(100, 20)
        };

        _agencyComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 130),
            Size = new Size(260, 25),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var addressLabel = new Label
        {
            Text = "Address (Search):",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 165),
            Size = new Size(120, 20)
        };

        _addressTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 190),
            Size = new Size(180, 25)
        };

        _searchAddressButton = new Button
        {
            Text = "Search",
            Font = new Font("Segoe UI", 9),
            Location = new Point(200, 190),
            Size = new Size(70, 25),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        _searchAddressButton.FlatAppearance.BorderSize = 0;
        _searchAddressButton.Click += SearchAddressButton_Click;

        var coordinatesLabel = new Label
        {
            Text = "Coordinates:",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(10, 225),
            Size = new Size(100, 20)
        };

        var latLabel = new Label
        {
            Text = "Latitude:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 250),
            Size = new Size(60, 20)
        };

        _latitudeTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(75, 250),
            Size = new Size(195, 25),
            Text = "37.9838"
        };
        _latitudeTextBox.TextChanged += CoordinateTextBox_TextChanged;

        var lngLabel = new Label
        {
            Text = "Longitude:",
            Font = new Font("Segoe UI", 10),
            Location = new Point(10, 280),
            Size = new Size(70, 20)
        };

        _longitudeTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Location = new Point(85, 280),
            Size = new Size(185, 25),
            Text = "23.7275"
        };
        _longitudeTextBox.TextChanged += CoordinateTextBox_TextChanged;

        var instructionLabel = new Label
        {
            Text = "Click on the map to set coordinates or search for an address.",
            Font = new Font("Segoe UI", 9, FontStyle.Italic),
            Location = new Point(10, 320),
            Size = new Size(280, 40),
            ForeColor = Color.Gray
        };

        leftPanel.Controls.AddRange(new Control[]
        {
            titleLabel, nameLabel, _nameTextBox, agencyLabel, _agencyComboBox,
            addressLabel, _addressTextBox, _searchAddressButton, coordinatesLabel,
            latLabel, _latitudeTextBox, lngLabel, _longitudeTextBox, instructionLabel
        });

        // Right panel for map
        var mapPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };

        var mapLabel = new Label
        {
            Text = "Map - Click to set location",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Location = new Point(5, 5),
            Size = new Size(200, 20)
        };

        _mapWebView = new WebView2
        {
            Location = new Point(5, 30),
            Size = new Size(mapPanel.Width - 15, mapPanel.Height - 40),
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        mapPanel.Controls.AddRange(new Control[] { mapLabel, _mapWebView });

        // Button panel
        var buttonPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 50
        };

        _okButton = new Button
        {
            Text = "Create Station",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(600, 8),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.OK
        };
        _okButton.FlatAppearance.BorderSize = 0;
        _okButton.Click += OkButton_Click;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 35),
            Location = new Point(730, 8),
            BackColor = Color.FromArgb(108, 117, 125),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            DialogResult = DialogResult.Cancel
        };
        _cancelButton.FlatAppearance.BorderSize = 0;

        buttonPanel.Controls.AddRange(new Control[] { _okButton, _cancelButton });

        mainPanel.Controls.Add(leftPanel, 0, 0);
        mainPanel.Controls.Add(mapPanel, 1, 0);
        mainPanel.SetColumnSpan(buttonPanel, 2);
        mainPanel.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(mainPanel);
    }

    private async void LoadAgencies()
    {
        try
        {
            _agencies = await _apiClient.GetAgenciesAsync();

            _agencyComboBox.DataSource = _agencies;
            _agencyComboBox.DisplayMember = "Name";
            _agencyComboBox.ValueMember = "Id";

            if (_agencies.Any())
                _agencyComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading agencies: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void InitializeMap()
    {
        try
        {
            await _mapWebView.EnsureCoreWebView2Async();

            var html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"" />
    <title>Station Location Map</title>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <link rel=""stylesheet"" href=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"" />
    <style>
        body { margin: 0; padding: 0; }
        #map { height: 100vh; width: 100%; }
    </style>
</head>
<body>
    <div id=""map""></div>
    <script src=""https://unpkg.com/leaflet@1.9.4/dist/leaflet.js""></script>
    <script>
        // Initialize map centered on Athens, Greece
        var map = L.map('map').setView([37.9838, 23.7275], 10);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '© OpenStreetMap contributors'
        }).addTo(map);

        var marker = null;

        // Handle map clicks
        map.on('click', function(e) {
            var lat = e.latlng.lat;
            var lng = e.latlng.lng;

            // Remove existing marker
            if (marker) {
                map.removeLayer(marker);
            }

            // Add new marker
            marker = L.marker([lat, lng]).addTo(map);

            // Send coordinates to C#
            window.chrome.webview.postMessage({
                type: 'coordinates',
                latitude: lat,
                longitude: lng
            });
        });

        // Function to set marker from C#
        function setMarker(lat, lng) {
            if (marker) {
                map.removeLayer(marker);
            }
            marker = L.marker([lat, lng]).addTo(map);
            map.setView([lat, lng], 15);
        }

        // Function to search for address
        function searchAddress(address) {
            fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(address)}&limit=1`)
                .then(response => response.json())
                .then(data => {
                    if (data && data.length > 0) {
                        var lat = parseFloat(data[0].lat);
                        var lng = parseFloat(data[0].lon);
                        setMarker(lat, lng);
                        window.chrome.webview.postMessage({
                            type: 'coordinates',
                            latitude: lat,
                            longitude: lng
                        });
                    } else {
                        window.chrome.webview.postMessage({
                            type: 'error',
                            message: 'Address not found'
                        });
                    }
                })
                .catch(error => {
                    window.chrome.webview.postMessage({
                        type: 'error',
                        message: 'Error searching address: ' + error.message
                    });
                });
        }
    </script>
</body>
</html>";

            _mapWebView.NavigateToString(html);
            _mapWebView.WebMessageReceived += MapWebView_WebMessageReceived;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize map: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MapWebView_WebMessageReceived(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            string messageJson;

            // Try different methods to get the message
            try
            {
                messageJson = e.TryGetWebMessageAsString();
            }
            catch (ArgumentException)
            {
                // If TryGetWebMessageAsString fails, try WebMessageAsJson
                try
                {
                    var jsonObject = e.WebMessageAsJson;
                    messageJson = jsonObject;
                }
                catch (Exception)
                {
                    // If both fail, we can't process this message
                    return;
                }
            }
            catch (Exception)
            {
                // For any other exception, we can't process this message
                return;
            }

            if (string.IsNullOrEmpty(messageJson))
            {
                return;
            }

            // Parse the JSON message
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var message = JsonSerializer.Deserialize<MapMessage>(messageJson, options);

                if (message?.Type == "coordinates")
                {
                    // Update UI on main thread
                    if (InvokeRequired)
                    {
                        Invoke(() =>
                        {
                            _latitudeTextBox.Text = message.Latitude.ToString("F6");
                            _longitudeTextBox.Text = message.Longitude.ToString("F6");
                        });
                    }
                    else
                    {
                        _latitudeTextBox.Text = message.Latitude.ToString("F6");
                        _longitudeTextBox.Text = message.Longitude.ToString("F6");
                    }
                }
                else if (message?.Type == "error")
                {
                    MessageBox.Show(message.Message, "Map Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (JsonException)
            {
                // If JSON parsing fails, try manual parsing as fallback
                if (messageJson.Contains("\"type\":\"coordinates\""))
                {
                    try
                    {
                        var latMatch = System.Text.RegularExpressions.Regex.Match(messageJson, @"""latitude"":\s*([+-]?\d+\.?\d*)");
                        var lngMatch = System.Text.RegularExpressions.Regex.Match(messageJson, @"""longitude"":\s*([+-]?\d+\.?\d*)");

                        if (latMatch.Success && lngMatch.Success)
                        {
                            var lat = double.Parse(latMatch.Groups[1].Value);
                            var lng = double.Parse(lngMatch.Groups[1].Value);

                            // Update UI on main thread
                            if (InvokeRequired)
                            {
                                Invoke(() =>
                                {
                                    _latitudeTextBox.Text = lat.ToString("F6");
                                    _longitudeTextBox.Text = lng.ToString("F6");
                                });
                            }
                            else
                            {
                                _latitudeTextBox.Text = lat.ToString("F6");
                                _longitudeTextBox.Text = lng.ToString("F6");
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // If manual parsing also fails, silently ignore
                    }
                }
            }
        }
        catch (Exception)
        {
            // Silently ignore any other errors to prevent the application from crashing
        }
    }

    private async void SearchAddressButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_addressTextBox.Text))
        {
            MessageBox.Show("Please enter an address to search.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            // Check if WebView is ready before executing script
            if (_mapWebView?.CoreWebView2 != null)
            {
                var escapedAddress = _addressTextBox.Text.Replace("'", "\\'").Replace("\"", "\\\"");
                await _mapWebView.ExecuteScriptAsync($"searchAddress('{escapedAddress}');");
            }
            else
            {
                MessageBox.Show("Map is not ready yet. Please wait a moment and try again.", "Map Loading",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error searching address: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void CoordinateTextBox_TextChanged(object? sender, EventArgs e)
    {
        if (double.TryParse(_latitudeTextBox.Text, out var lat) &&
            double.TryParse(_longitudeTextBox.Text, out var lng) &&
            lat >= -90 && lat <= 90 && lng >= -180 && lng <= 180)
        {
            try
            {
                // Check if WebView is ready before executing script
                if (_mapWebView?.CoreWebView2 != null)
                {
                    await _mapWebView.ExecuteScriptAsync($"setMarker({lat.ToString("F6")}, {lng.ToString("F6")});");
                }
            }
            catch
            {
                // Ignore errors when manually typing coordinates or WebView isn't ready
            }
        }
    }

    private async void OkButton_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput())
        {
            return;
        }

        try
        {
            var createDto = new CreateStationDto
            {
                Name = _nameTextBox.Text.Trim(),
                AgencyId = ((AgencyDto)_agencyComboBox.SelectedItem).Id,
                Latitude = double.Parse(_latitudeTextBox.Text),
                Longitude = double.Parse(_longitudeTextBox.Text)
            };

            await _apiClient.CreateStationAsync(createDto);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create station: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            MessageBox.Show("Please enter a station name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameTextBox.Focus();
            return false;
        }

        if (_agencyComboBox.SelectedItem == null)
        {
            MessageBox.Show("Please select an agency.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _agencyComboBox.Focus();
            return false;
        }

        if (!double.TryParse(_latitudeTextBox.Text, out var lat) || lat < -90 || lat > 90)
        {
            MessageBox.Show("Please enter a valid latitude (-90 to 90).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _latitudeTextBox.Focus();
            return false;
        }

        if (!double.TryParse(_longitudeTextBox.Text, out var lng) || lng < -180 || lng > 180)
        {
            MessageBox.Show("Please enter a valid longitude (-180 to 180).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _longitudeTextBox.Focus();
            return false;
        }

        return true;
    }

    private class MapMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "";

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";
    }
}