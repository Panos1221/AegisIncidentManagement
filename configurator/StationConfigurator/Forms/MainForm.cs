using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class MainForm : Form
{
    private readonly ApiClient _apiClient;
    private TabControl _tabControl;
    private Button _createStationButton;
    private ComboBox _stationFilterComboBox;
    private Label _statusLabel;

    public MainForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
        LoadInitialData();
    }

    private void InitializeComponent()
    {
        // Form setup
        Text = "Station Configurator - Administrative Console";
        Size = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1000, 600);

        // Create main layout panel
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(10)
        };
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

        // Header panel
        var headerPanel = CreateHeaderPanel();
        mainPanel.Controls.Add(headerPanel, 0, 0);

        // Tab control for different management sections
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10),
        };

        // Add tabs
        _tabControl.TabPages.Add(CreateStationTab());
        _tabControl.TabPages.Add(CreateUserTab());
        _tabControl.TabPages.Add(CreatePersonnelTab());
        _tabControl.TabPages.Add(CreateVehicleTab());
        _tabControl.TabPages.Add(CreateOverviewTab());

        mainPanel.Controls.Add(_tabControl, 0, 1);

        // Status bar
        _statusLabel = new Label
        {
            Text = "Ready",
            Dock = DockStyle.Fill,
            ForeColor = Color.Gray,
            Font = new Font("Segoe UI", 9),
            TextAlign = ContentAlignment.MiddleLeft
        };
        mainPanel.Controls.Add(_statusLabel, 0, 2);

        Controls.Add(mainPanel);
    }

    private Panel CreateHeaderPanel()
    {
        var headerPanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(245, 245, 245)
        };

        var titleLabel = new Label
        {
            Text = "Station Configurator",
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Location = new Point(10, 15),
            Size = new Size(300, 35)
        };

        var subtitleLabel = new Label
        {
            Text = "Configure stations, users, personnel, and vehicles",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            Location = new Point(10, 45),
            Size = new Size(400, 20)
        };

        _createStationButton = new Button
        {
            Text = "Create New Station",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(150, 35),
            Location = new Point(800, 20),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createStationButton.FlatAppearance.BorderSize = 0;
        _createStationButton.Click += CreateStationButton_Click;

        var logoutButton = new Button
        {
            Text = "Logout",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 35),
            Location = new Point(960, 20),
            BackColor = Color.FromArgb(204, 51, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        logoutButton.FlatAppearance.BorderSize = 0;
        logoutButton.Click += (s, e) =>
        {
            _apiClient.Logout();
            Close();
        };

        headerPanel.Controls.AddRange(new Control[] { titleLabel, subtitleLabel, _createStationButton, logoutButton });
        return headerPanel;
    }

    private TabPage CreateStationTab()
    {
        var tabPage = new TabPage("Stations")
        {
            Font = new Font("Segoe UI", 10)
        };

        var stationPanel = new StationManagementControl(_apiClient)
        {
            Dock = DockStyle.Fill
        };

        tabPage.Controls.Add(stationPanel);
        return tabPage;
    }

    private TabPage CreateUserTab()
    {
        var tabPage = new TabPage("Users")
        {
            Font = new Font("Segoe UI", 10)
        };

        var userPanel = new UserManagementControl(_apiClient)
        {
            Dock = DockStyle.Fill
        };

        tabPage.Controls.Add(userPanel);
        return tabPage;
    }

    private TabPage CreatePersonnelTab()
    {
        var tabPage = new TabPage("Personnel")
        {
            Font = new Font("Segoe UI", 10)
        };

        var personnelPanel = new PersonnelManagementControl(_apiClient)
        {
            Dock = DockStyle.Fill
        };

        tabPage.Controls.Add(personnelPanel);
        return tabPage;
    }

    private TabPage CreateVehicleTab()
    {
        var tabPage = new TabPage("Vehicles")
        {
            Font = new Font("Segoe UI", 10)
        };

        var vehiclePanel = new VehicleManagementControl(_apiClient)
        {
            Dock = DockStyle.Fill
        };

        tabPage.Controls.Add(vehiclePanel);
        return tabPage;
    }

    private TabPage CreateOverviewTab()
    {
        var tabPage = new TabPage("Overview")
        {
            Font = new Font("Segoe UI", 10)
        };

        var overviewPanel = new OverviewControl(_apiClient)
        {
            Dock = DockStyle.Fill
        };

        tabPage.Controls.Add(overviewPanel);
        return tabPage;
    }

    private async void LoadInitialData()
    {
        try
        {
            _statusLabel.Text = "Loading data...";
            await Task.Delay(100); // Allow UI to update
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading initial data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _statusLabel.Text = "Ready";
        }
    }

    private void CreateStationButton_Click(object? sender, EventArgs e)
    {
        using var createStationForm = new CreateStationForm(_apiClient);
        if (createStationForm.ShowDialog() == DialogResult.OK)
        {
            // Refresh the station list in the current tab
            if (_tabControl.SelectedTab?.Controls[0] is StationManagementControl stationControl)
            {
                stationControl.RefreshData();
            }
            _statusLabel.Text = "Station created successfully";
        }
    }

    public void UpdateStatus(string message)
    {
        _statusLabel.Text = message;
    }
}