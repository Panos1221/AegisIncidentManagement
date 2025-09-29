using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class VehicleManagementControl : UserControl
{
    private readonly ApiClient _apiClient;
    private DataGridView _vehiclesDataGridView;
    private Button _refreshButton;
    private Button _createVehicleButton;
    private ComboBox _stationFilterComboBox;
    private ComboBox _statusFilterComboBox;
    private List<StationDto> _stations;

    public VehicleManagementControl(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _stations = new List<StationDto>();
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        // Main panel
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(10)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 70
        };

        var titleLabel = new Label
        {
            Text = "Vehicle Management",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(0, 5),
            Size = new Size(200, 25)
        };

        var stationLabel = new Label
        {
            Text = "Filter by Station:",
            Font = new Font("Segoe UI", 9),
            Location = new Point(0, 30),
            Size = new Size(100, 20)
        };

        _stationFilterComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(150, 25),
            Location = new Point(0, 50),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _stationFilterComboBox.SelectedIndexChanged += StationFilter_SelectedIndexChanged;

        var statusLabel = new Label
        {
            Text = "Filter by Status:",
            Font = new Font("Segoe UI", 9),
            Location = new Point(160, 30),
            Size = new Size(100, 20)
        };

        _statusFilterComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(160, 50),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _statusFilterComboBox.Items.AddRange(new object[] { "All Status", "Available", "Notified", "EnRoute", "OnScene", "Busy", "Completed", "Maintenance", "Offline" });
        _statusFilterComboBox.SelectedIndex = 0;
        _statusFilterComboBox.SelectedIndexChanged += StatusFilter_SelectedIndexChanged;

        _refreshButton = new Button
        {
            Text = "Refresh",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 35),
            Location = new Point(300, 35),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _refreshButton.FlatAppearance.BorderSize = 0;
        _refreshButton.Click += RefreshButton_Click;

        _createVehicleButton = new Button
        {
            Text = "Create Vehicle",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(390, 35),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createVehicleButton.FlatAppearance.BorderSize = 0;
        _createVehicleButton.Click += CreateVehicleButton_Click;

        toolbarPanel.Controls.AddRange(new Control[]
        {
            titleLabel, stationLabel, _stationFilterComboBox, statusLabel, _statusFilterComboBox,
            _refreshButton, _createVehicleButton
        });

        // DataGridView
        _vehiclesDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            RowHeadersVisible = false,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            Font = new Font("Segoe UI", 10)
        };

        SetupDataGridViewColumns();

        mainPanel.Controls.Add(toolbarPanel, 0, 0);
        mainPanel.Controls.Add(_vehiclesDataGridView, 0, 1);
        Controls.Add(mainPanel);
    }

    private void SetupDataGridViewColumns()
    {
        _vehiclesDataGridView.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Width = 50,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Callsign",
                HeaderText = "Callsign",
                DataPropertyName = "Callsign",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Type",
                HeaderText = "Vehicle Type",
                DataPropertyName = "Type",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "PlateNumber",
                HeaderText = "Plate #",
                DataPropertyName = "PlateNumber",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                DataPropertyName = "Status",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "StationName",
                HeaderText = "Station",
                DataPropertyName = "Station.Name",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "WaterCapacity",
                HeaderText = "Water Cap. (L)",
                DataPropertyName = "WaterCapacityLiters",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N0" }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "FuelLevel",
                HeaderText = "Fuel %",
                DataPropertyName = "FuelLevelPercent",
                Width = 70,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            }
        });

        // Style the header
        _vehiclesDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _vehiclesDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        _vehiclesDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _vehiclesDataGridView.ColumnHeadersHeight = 35;
        _vehiclesDataGridView.EnableHeadersVisualStyles = false;

        // Alternate row colors
        _vehiclesDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _vehiclesDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
        _vehiclesDataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 102, 204);
        _vehiclesDataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;

        // Add status color coding
        _vehiclesDataGridView.CellFormatting += VehiclesDataGridView_CellFormatting;
    }

    private void VehiclesDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_vehiclesDataGridView.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
        {
            var status = e.Value.ToString();
            switch (status)
            {
                case "Available":
                    e.CellStyle.BackColor = Color.LightGreen;
                    e.CellStyle.ForeColor = Color.DarkGreen;
                    break;
                case "Notified":
                    e.CellStyle.BackColor = Color.Yellow;
                    e.CellStyle.ForeColor = Color.Black;
                    break;
                case "EnRoute":
                    e.CellStyle.BackColor = Color.Orange;
                    e.CellStyle.ForeColor = Color.DarkRed;
                    break;
                case "OnScene":
                    e.CellStyle.BackColor = Color.Red;
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "Busy":
                    e.CellStyle.BackColor = Color.Purple;
                    e.CellStyle.ForeColor = Color.White;
                    break;
                case "Completed":
                    e.CellStyle.BackColor = Color.LightBlue;
                    e.CellStyle.ForeColor = Color.Black;
                    break;
                case "Maintenance":
                    e.CellStyle.BackColor = Color.LightGray;
                    e.CellStyle.ForeColor = Color.Black;
                    break;
                case "Offline":
                    e.CellStyle.BackColor = Color.DarkGray;
                    e.CellStyle.ForeColor = Color.White;
                    break;
            }
        }
    }

    private async void LoadData()
    {
        try
        {
            // Load stations for filter (only once)
            if (_stations.Count == 0)
            {
                _stations = await _apiClient.GetStationsAsync();
                var stationList = new List<object> { new { Id = -1, Name = "All Stations" } };
                stationList.AddRange(_stations.Select(s => new { s.Id, s.Name }));

                // Temporarily remove event handlers to prevent infinite loops
                _stationFilterComboBox.SelectedIndexChanged -= StationFilter_SelectedIndexChanged;
                _stationFilterComboBox.DataSource = stationList;
                _stationFilterComboBox.DisplayMember = "Name";
                _stationFilterComboBox.ValueMember = "Id";
                _stationFilterComboBox.SelectedIndex = 0;
                _stationFilterComboBox.SelectedIndexChanged += StationFilter_SelectedIndexChanged;
            }

            // Load vehicles
            await RefreshVehicles();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RefreshVehicles()
    {
        try
        {
            var vehicles = await _apiClient.GetVehiclesAsync();
            _vehiclesDataGridView.DataSource = vehicles;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading vehicles: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "Refreshing...";
            await RefreshVehicles();
        }
        finally
        {
            _refreshButton.Enabled = true;
            _refreshButton.Text = "Refresh";
        }
    }

    public async void RefreshData()
    {
        await RefreshVehicles();
    }

    private void CreateVehicleButton_Click(object? sender, EventArgs e)
    {
        using var createVehicleForm = new CreateVehicleForm(_apiClient, _stations);
        if (createVehicleForm.ShowDialog() == DialogResult.OK)
        {
            RefreshVehicles();
        }
    }

    private void StationFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void StatusFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_vehiclesDataGridView.DataSource is not List<VehicleDto> vehicles) return;

        var filteredVehicles = vehicles.AsEnumerable();

        // Filter by station
        if (_stationFilterComboBox.SelectedValue is int stationId && stationId != -1)
        {
            filteredVehicles = filteredVehicles.Where(v => v.StationId == stationId);
        }

        // Filter by status
        if (_statusFilterComboBox.SelectedItem?.ToString() != "All Status")
        {
            var statusText = _statusFilterComboBox.SelectedItem?.ToString();
            if (Enum.TryParse<VehicleStatus>(statusText, out var status))
            {
                filteredVehicles = filteredVehicles.Where(v => v.Status == status);
            }
        }

        _vehiclesDataGridView.DataSource = filteredVehicles.ToList();
    }
}