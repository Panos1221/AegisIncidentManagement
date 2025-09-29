using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class PersonnelManagementControl : UserControl
{
    private readonly ApiClient _apiClient;
    private DataGridView _personnelDataGridView;
    private Button _refreshButton;
    private Button _createPersonnelButton;
    private ComboBox _stationFilterComboBox;
    private ComboBox _statusFilterComboBox;
    private List<StationDto> _stations;

    public PersonnelManagementControl(ApiClient apiClient)
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
            Text = "Personnel Management",
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
        _statusFilterComboBox.Items.AddRange(new object[] { "All Status", "Active", "Inactive" });
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

        _createPersonnelButton = new Button
        {
            Text = "Add Personnel",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(390, 35),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createPersonnelButton.FlatAppearance.BorderSize = 0;
        _createPersonnelButton.Click += CreatePersonnelButton_Click;

        toolbarPanel.Controls.AddRange(new Control[]
        {
            titleLabel, stationLabel, _stationFilterComboBox, statusLabel, _statusFilterComboBox,
            _refreshButton, _createPersonnelButton
        });

        // DataGridView
        _personnelDataGridView = new DataGridView
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
        mainPanel.Controls.Add(_personnelDataGridView, 0, 1);
        Controls.Add(mainPanel);
    }

    private void SetupDataGridViewColumns()
    {
        _personnelDataGridView.Columns.AddRange(new DataGridViewColumn[]
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
                Name = "Name",
                HeaderText = "Name",
                DataPropertyName = "Name",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Rank",
                HeaderText = "Rank",
                DataPropertyName = "Rank",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "BadgeNumber",
                HeaderText = "Badge #",
                DataPropertyName = "BadgeNumber",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                HeaderText = "Active",
                DataPropertyName = "IsActive",
                Width = 60,
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
                Name = "AgencyName",
                HeaderText = "Agency",
                DataPropertyName = "AgencyName",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            }
        });

        // Style the header
        _personnelDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _personnelDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        _personnelDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _personnelDataGridView.ColumnHeadersHeight = 35;
        _personnelDataGridView.EnableHeadersVisualStyles = false;

        // Alternate row colors
        _personnelDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _personnelDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
        _personnelDataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 102, 204);
        _personnelDataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;

        // Add status color coding for Active/Inactive
        _personnelDataGridView.CellFormatting += PersonnelDataGridView_CellFormatting;
    }

    private void PersonnelDataGridView_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (_personnelDataGridView.Columns[e.ColumnIndex].Name == "IsActive" && e.Value is bool isActive)
        {
            var row = _personnelDataGridView.Rows[e.RowIndex];
            if (isActive)
            {
                row.DefaultCellStyle.BackColor = Color.White;
                row.DefaultCellStyle.ForeColor = Color.Black;
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                row.DefaultCellStyle.ForeColor = Color.Gray;
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

            // Load personnel
            await RefreshPersonnel();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RefreshPersonnel()
    {
        try
        {
            var personnel = await _apiClient.GetPersonnelAsync();

            // Populate station names if they're missing from the API response
            foreach (var person in personnel)
            {
                if (person.Station == null || string.IsNullOrEmpty(person.Station.Name))
                {
                    var station = _stations.FirstOrDefault(s => s.Id == person.StationId);
                    if (station != null)
                    {
                        person.Station = station;
                    }
                }
            }

            _personnelDataGridView.DataSource = personnel;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading personnel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "Refreshing...";
            await RefreshPersonnel();
        }
        finally
        {
            _refreshButton.Enabled = true;
            _refreshButton.Text = "Refresh";
        }
    }

    public async void RefreshData()
    {
        await RefreshPersonnel();
    }

    private void CreatePersonnelButton_Click(object? sender, EventArgs e)
    {
        using var createPersonnelForm = new CreatePersonnelForm(_apiClient, _stations);
        if (createPersonnelForm.ShowDialog() == DialogResult.OK)
        {
            RefreshPersonnel();
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
        if (_personnelDataGridView.DataSource is not List<PersonnelDto> personnel) return;

        var filteredPersonnel = personnel.AsEnumerable();

        // Filter by station
        if (_stationFilterComboBox.SelectedValue is int stationId && stationId != -1)
        {
            filteredPersonnel = filteredPersonnel.Where(p => p.StationId == stationId);
        }

        // Filter by status
        var statusFilter = _statusFilterComboBox.SelectedItem?.ToString();
        if (statusFilter != "All Status")
        {
            var isActive = statusFilter == "Active";
            filteredPersonnel = filteredPersonnel.Where(p => p.IsActive == isActive);
        }

        _personnelDataGridView.DataSource = filteredPersonnel.ToList();
    }
}