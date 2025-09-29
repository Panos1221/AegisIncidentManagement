using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class OverviewControl : UserControl
{
    private readonly ApiClient _apiClient;
    private Label _stationsCountLabel;
    private Label _usersCountLabel;
    private Label _personnelCountLabel;
    private Label _vehiclesCountLabel;
    private Label _lastRefreshLabel;
    private Button _refreshButton;
    private DataGridView _recentStationsDataGridView;
    private Panel _statsPanel;

    public OverviewControl(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        // Main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(15)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 120));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        // Header
        var headerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 50
        };

        var titleLabel = new Label
        {
            Text = "System Overview",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Location = new Point(0, 10),
            Size = new Size(200, 30)
        };

        _refreshButton = new Button
        {
            Text = "Refresh Data",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 35),
            Location = new Point(700, 5),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _refreshButton.FlatAppearance.BorderSize = 0;
        _refreshButton.Click += RefreshButton_Click;

        headerPanel.Controls.AddRange(new Control[] { titleLabel, _refreshButton });

        // Statistics Panel
        _statsPanel = CreateStatsPanel();

        // Recent Stations Panel
        var recentPanel = CreateRecentStationsPanel();

        mainPanel.Controls.Add(headerPanel, 0, 0);
        mainPanel.Controls.Add(_statsPanel, 0, 1);
        mainPanel.Controls.Add(recentPanel, 0, 2);
        Controls.Add(mainPanel);
    }

    private Panel CreateStatsPanel()
    {
        var statsPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(248, 249, 250),
            Padding = new Padding(10)
        };

        // Create 4 stat cards
        var cardWidth = 180;
        var cardHeight = 80;
        var cardSpacing = 20;

        // Stations card
        var stationsCard = CreateStatCard("Stations", "0", Color.FromArgb(52, 144, 220), 0);
        _stationsCountLabel = stationsCard.Controls.OfType<Label>().First(l => l.Name == "CountLabel");

        // Users card
        var usersCard = CreateStatCard("Users", "0", Color.FromArgb(92, 184, 92), cardWidth + cardSpacing);
        _usersCountLabel = usersCard.Controls.OfType<Label>().First(l => l.Name == "CountLabel");

        // Personnel card
        var personnelCard = CreateStatCard("Personnel", "0", Color.FromArgb(240, 173, 78), (cardWidth + cardSpacing) * 2);
        _personnelCountLabel = personnelCard.Controls.OfType<Label>().First(l => l.Name == "CountLabel");

        // Vehicles card
        var vehiclesCard = CreateStatCard("Vehicles", "0", Color.FromArgb(217, 83, 79), (cardWidth + cardSpacing) * 3);
        _vehiclesCountLabel = vehiclesCard.Controls.OfType<Label>().First(l => l.Name == "CountLabel");

        statsPanel.Controls.AddRange(new Control[] { stationsCard, usersCard, personnelCard, vehiclesCard });

        return statsPanel;
    }

    private Panel CreateStatCard(string title, string count, Color color, int xOffset)
    {
        var card = new Panel
        {
            Size = new Size(180, 80),
            Location = new Point(xOffset, 10),
            BackColor = Color.White,
            BorderStyle = BorderStyle.FixedSingle
        };

        // Color accent bar
        var accentBar = new Panel
        {
            Size = new Size(5, 80),
            Location = new Point(0, 0),
            BackColor = color
        };

        var titleLabel = new Label
        {
            Text = title,
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            Location = new Point(15, 15),
            Size = new Size(150, 20)
        };

        var countLabel = new Label
        {
            Name = "CountLabel",
            Text = count,
            Font = new Font("Segoe UI", 20, FontStyle.Bold),
            ForeColor = color,
            Location = new Point(15, 35),
            Size = new Size(150, 30)
        };

        card.Controls.AddRange(new Control[] { accentBar, titleLabel, countLabel });
        return card;
    }

    private Panel CreateRecentStationsPanel()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Fill
        };

        var titleLabel = new Label
        {
            Text = "Recent Stations",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(0, 10),
            Size = new Size(200, 25)
        };

        _recentStationsDataGridView = new DataGridView
        {
            Location = new Point(0, 45),
            Size = new Size(800, 300),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
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

        SetupRecentStationsGrid();

        _lastRefreshLabel = new Label
        {
            Text = "Last updated: Never",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Location = new Point(0, 355),
            Size = new Size(300, 20),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Left
        };

        panel.Controls.AddRange(new Control[] { titleLabel, _recentStationsDataGridView, _lastRefreshLabel });
        return panel;
    }

    private void SetupRecentStationsGrid()
    {
        _recentStationsDataGridView.Columns.AddRange(new DataGridViewColumn[]
        {
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
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "F4" }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Longitude",
                HeaderText = "Longitude",
                DataPropertyName = "Longitude",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter, Format = "F4" }
            }
        });

        // Style headers
        _recentStationsDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _recentStationsDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        _recentStationsDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _recentStationsDataGridView.ColumnHeadersHeight = 35;
        _recentStationsDataGridView.EnableHeadersVisualStyles = false;

        // Alternate row colors
        _recentStationsDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _recentStationsDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
        _recentStationsDataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 102, 204);
        _recentStationsDataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;
    }

    private async void LoadData()
    {
        await RefreshDataAsync();
    }

    private async Task RefreshDataAsync()
    {
        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "Refreshing...";

            // Load counts in parallel
            var stationsTask = _apiClient.GetStationsAsync();
            var usersTask = _apiClient.GetUsersAsync();
            var personnelTask = _apiClient.GetPersonnelAsync();
            var vehiclesTask = _apiClient.GetVehiclesAsync();

            await Task.WhenAll(stationsTask, usersTask, personnelTask, vehiclesTask);

            var stations = await stationsTask;
            var users = await usersTask;
            var personnel = await personnelTask;
            var vehicles = await vehiclesTask;

            // Update counts
            _stationsCountLabel.Text = stations.Count.ToString();
            _usersCountLabel.Text = users.Count.ToString();
            _personnelCountLabel.Text = personnel.Count.ToString();
            _vehiclesCountLabel.Text = vehicles.Count.ToString();

            // Show recent stations (limit to 10)
            _recentStationsDataGridView.DataSource = stations.Take(10).ToList();

            _lastRefreshLabel.Text = $"Last updated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refreshing data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _refreshButton.Enabled = true;
            _refreshButton.Text = "Refresh Data";
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshDataAsync();
    }

    public void RefreshData()
    {
        _ = Task.Run(async () => await RefreshDataAsync());
    }
}