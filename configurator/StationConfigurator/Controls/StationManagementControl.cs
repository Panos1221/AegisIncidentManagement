using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class StationManagementControl : UserControl
{
    private readonly ApiClient _apiClient;
    private DataGridView _stationsDataGridView;
    private Button _refreshButton;
    private Button _createStationButton;
    private ComboBox _agencyFilterComboBox;

    public StationManagementControl(ApiClient apiClient)
    {
        _apiClient = apiClient;
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
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Toolbar panel
        var toolbarPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 50
        };

        var titleLabel = new Label
        {
            Text = "Station Management",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(0, 5),
            Size = new Size(200, 25)
        };

        _agencyFilterComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(200, 25),
            Location = new Point(0, 30),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _agencyFilterComboBox.SelectedIndexChanged += AgencyFilter_SelectedIndexChanged;

        _refreshButton = new Button
        {
            Text = "Refresh",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 35),
            Location = new Point(220, 20),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _refreshButton.FlatAppearance.BorderSize = 0;
        _refreshButton.Click += RefreshButton_Click;

        _createStationButton = new Button
        {
            Text = "Create Station",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(310, 20),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createStationButton.FlatAppearance.BorderSize = 0;
        _createStationButton.Click += CreateStationButton_Click;

        toolbarPanel.Controls.AddRange(new Control[] { titleLabel, _agencyFilterComboBox, _refreshButton, _createStationButton });

        // DataGridView
        _stationsDataGridView = new DataGridView
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
        mainPanel.Controls.Add(_stationsDataGridView, 0, 1);
        Controls.Add(mainPanel);
    }

    private void SetupDataGridViewColumns()
    {
        _stationsDataGridView.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn
            {
                Name = "Id",
                HeaderText = "ID",
                DataPropertyName = "Id",
                Width = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Station Name",
                DataPropertyName = "Name",
                Width = 200,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "AgencyName",
                HeaderText = "Agency",
                DataPropertyName = "AgencyName",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Latitude",
                HeaderText = "Latitude",
                DataPropertyName = "Latitude",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "F6" }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Longitude",
                HeaderText = "Longitude",
                DataPropertyName = "Longitude",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "F6" }
            }
        });

        // Style the header
        _stationsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _stationsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        _stationsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _stationsDataGridView.ColumnHeadersHeight = 35;
        _stationsDataGridView.EnableHeadersVisualStyles = false;

        // Alternate row colors
        _stationsDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _stationsDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
        _stationsDataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 102, 204);
        _stationsDataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;
    }

    private async void LoadData()
    {
        try
        {
            var stations = await _apiClient.GetStationsAsync();
            _stationsDataGridView.DataSource = stations;

            // Load agencies for filter (only if not already loaded)
            if (_agencyFilterComboBox.DataSource == null)
            {
                var agencies = await _apiClient.GetAgenciesAsync();
                var agencyList = new List<object> { new { Id = -1, Name = "All Agencies" } };
                agencyList.AddRange(agencies.Select(a => new { a.Id, a.Name }));

                // Temporarily remove event handler to prevent infinite loop
                _agencyFilterComboBox.SelectedIndexChanged -= AgencyFilter_SelectedIndexChanged;
                _agencyFilterComboBox.DataSource = agencyList;
                _agencyFilterComboBox.DisplayMember = "Name";
                _agencyFilterComboBox.ValueMember = "Id";
                _agencyFilterComboBox.SelectedIndex = 0;
                // Re-add event handler
                _agencyFilterComboBox.SelectedIndexChanged += AgencyFilter_SelectedIndexChanged;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading stations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "Refreshing...";

            var stations = await _apiClient.GetStationsAsync();
            _stationsDataGridView.DataSource = stations;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _refreshButton.Enabled = true;
            _refreshButton.Text = "Refresh";
        }
    }

    public void RefreshData()
    {
        _ = Task.Run(async () => await RefreshDataAsync());
    }

    private void CreateStationButton_Click(object? sender, EventArgs e)
    {
        using var createStationForm = new CreateStationForm(_apiClient);
        if (createStationForm.ShowDialog() == DialogResult.OK)
        {
            RefreshData();
        }
    }

    private void AgencyFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        // Apply client-side filtering instead of reloading data
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        // This method can be implemented later for client-side filtering
        // For now, do nothing to prevent infinite loops
    }
}