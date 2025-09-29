using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class UserManagementControl : UserControl
{
    private readonly ApiClient _apiClient;
    private DataGridView _usersDataGridView;
    private Button _refreshButton;
    private Button _createUserButton;
    private ComboBox _stationFilterComboBox;
    private ComboBox _roleFilterComboBox;
    private List<StationDto> _stations;

    public UserManagementControl(ApiClient apiClient)
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
            Text = "User Management",
            Font = new Font("Segoe UI", 12, FontStyle.Bold),
            Location = new Point(0, 5),
            Size = new Size(200, 25)
        };

        var filterLabel = new Label
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

        var roleLabel = new Label
        {
            Text = "Filter by Role:",
            Font = new Font("Segoe UI", 9),
            Location = new Point(160, 30),
            Size = new Size(100, 20)
        };

        _roleFilterComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(160, 50),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _roleFilterComboBox.Items.AddRange(new object[] { "All Roles", "Dispatcher", "Member" });
        _roleFilterComboBox.SelectedIndex = 0;
        _roleFilterComboBox.SelectedIndexChanged += RoleFilter_SelectedIndexChanged;

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

        _createUserButton = new Button
        {
            Text = "Create User",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(390, 35),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createUserButton.FlatAppearance.BorderSize = 0;
        _createUserButton.Click += CreateUserButton_Click;

        toolbarPanel.Controls.AddRange(new Control[]
        {
            titleLabel, filterLabel, _stationFilterComboBox, roleLabel, _roleFilterComboBox,
            _refreshButton, _createUserButton
        });

        // DataGridView
        _usersDataGridView = new DataGridView
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
        mainPanel.Controls.Add(_usersDataGridView, 0, 1);
        Controls.Add(mainPanel);
    }

    private void SetupDataGridViewColumns()
    {
        _usersDataGridView.Columns.AddRange(new DataGridViewColumn[]
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
                Name = "Email",
                HeaderText = "Email",
                DataPropertyName = "Email",
                Width = 200,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "Role",
                HeaderText = "Role",
                DataPropertyName = "Role",
                Width = 100,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "AgencyName",
                HeaderText = "Agency",
                DataPropertyName = "AgencyName",
                Width = 120,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewTextBoxColumn
            {
                Name = "StationName",
                HeaderText = "Station",
                DataPropertyName = "StationName",
                Width = 150,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleLeft }
            },
            new DataGridViewCheckBoxColumn
            {
                Name = "IsActive",
                HeaderText = "Active",
                DataPropertyName = "IsActive",
                Width = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            }
        });

        // Style the header
        _usersDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
        _usersDataGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
        _usersDataGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        _usersDataGridView.ColumnHeadersHeight = 35;
        _usersDataGridView.EnableHeadersVisualStyles = false;

        // Alternate row colors
        _usersDataGridView.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        _usersDataGridView.RowsDefaultCellStyle.BackColor = Color.White;
        _usersDataGridView.RowsDefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 102, 204);
        _usersDataGridView.RowsDefaultCellStyle.SelectionForeColor = Color.White;
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

            // Load users
            await RefreshUsers();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RefreshUsers()
    {
        try
        {
            var users = await _apiClient.GetUsersAsync();
            _usersDataGridView.DataSource = users;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _refreshButton.Enabled = false;
            _refreshButton.Text = "Refreshing...";
            await RefreshUsers();
        }
        finally
        {
            _refreshButton.Enabled = true;
            _refreshButton.Text = "Refresh";
        }
    }

    public async void RefreshData()
    {
        await RefreshUsers();
    }

    private void CreateUserButton_Click(object? sender, EventArgs e)
    {
        using var createUserForm = new CreateUserForm(_apiClient, _stations);
        if (createUserForm.ShowDialog() == DialogResult.OK)
        {
            RefreshUsers();
        }
    }

    private void StationFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void RoleFilter_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_usersDataGridView.DataSource is not List<UserDto> users) return;

        var filteredUsers = users.AsEnumerable();

        // Filter by station
        if (_stationFilterComboBox.SelectedValue is int stationId && stationId != -1)
        {
            filteredUsers = filteredUsers.Where(u => u.StationId == stationId);
        }

        // Filter by role
        if (_roleFilterComboBox.SelectedItem?.ToString() != "All Roles")
        {
            var roleText = _roleFilterComboBox.SelectedItem?.ToString();
            if (Enum.TryParse<UserRole>(roleText, out var role))
            {
                filteredUsers = filteredUsers.Where(u => u.Role == role);
            }
        }

        _usersDataGridView.DataSource = filteredUsers.ToList();
    }
}