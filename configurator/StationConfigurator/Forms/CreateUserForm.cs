using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class CreateUserForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly List<StationDto> _stations;
    private TextBox _nameTextBox;
    private TextBox _emailTextBox;
    private TextBox _passwordTextBox;
    private ComboBox _roleComboBox;
    private ComboBox _agencyComboBox;
    private ComboBox _stationComboBox;
    private CheckBox _isActiveCheckBox;
    private Button _createButton;
    private Button _cancelButton;
    private List<AgencyDto> _agencies;

    public CreateUserForm(ApiClient apiClient, List<StationDto> stations)
    {
        _apiClient = apiClient;
        _stations = stations;
        _agencies = new List<AgencyDto>();
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        // Form setup
        Text = "Create New User";
        Size = new Size(500, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Title
        var titleLabel = new Label
        {
            Text = "Create New User Account",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Size = new Size(400, 30),
            Location = new Point(30, 20)
        };

        // Name
        var nameLabel = new Label
        {
            Text = "Full Name:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 70)
        };

        _nameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 70)
        };

        // Email
        var emailLabel = new Label
        {
            Text = "Email:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 110)
        };

        _emailTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 110)
        };

        // Password
        var passwordLabel = new Label
        {
            Text = "Password:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 150)
        };

        _passwordTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 150),
            UseSystemPasswordChar = true
        };

        // Role
        var roleLabel = new Label
        {
            Text = "Role:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 190)
        };

        _roleComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 190),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _roleComboBox.Items.AddRange(new object[] { "Member", "Dispatcher" });
        _roleComboBox.SelectedIndex = 0;

        // Agency
        var agencyLabel = new Label
        {
            Text = "Agency:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 230)
        };

        _agencyComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 230),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _agencyComboBox.SelectedIndexChanged += AgencyComboBox_SelectedIndexChanged;

        // Station
        var stationLabel = new Label
        {
            Text = "Station:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 25),
            Location = new Point(30, 270)
        };

        _stationComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(140, 270),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Active checkbox
        _isActiveCheckBox = new CheckBox
        {
            Text = "Account is active",
            Font = new Font("Segoe UI", 10),
            Size = new Size(200, 25),
            Location = new Point(140, 310),
            Checked = true
        };

        // Buttons
        _createButton = new Button
        {
            Text = "Create User",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(110, 35),
            Location = new Point(250, 360),
            BackColor = Color.FromArgb(0, 153, 51),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _createButton.FlatAppearance.BorderSize = 0;
        _createButton.Click += CreateButton_Click;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Font = new Font("Segoe UI", 10),
            Size = new Size(100, 35),
            Location = new Point(370, 360),
            BackColor = Color.Gray,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _cancelButton.FlatAppearance.BorderSize = 0;
        _cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };

        // Add all controls
        Controls.AddRange(new Control[]
        {
            titleLabel, nameLabel, _nameTextBox, emailLabel, _emailTextBox, passwordLabel, _passwordTextBox,
            roleLabel, _roleComboBox, agencyLabel, _agencyComboBox, stationLabel, _stationComboBox,
            _isActiveCheckBox, _createButton, _cancelButton
        });

        // Set tab order
        _nameTextBox.TabIndex = 0;
        _emailTextBox.TabIndex = 1;
        _passwordTextBox.TabIndex = 2;
        _roleComboBox.TabIndex = 3;
        _agencyComboBox.TabIndex = 4;
        _stationComboBox.TabIndex = 5;
        _isActiveCheckBox.TabIndex = 6;
        _createButton.TabIndex = 7;
        _cancelButton.TabIndex = 8;

        AcceptButton = _createButton;
        CancelButton = _cancelButton;
    }

    private async void LoadData()
    {
        try
        {
            _agencies = await _apiClient.GetAgenciesAsync();

            _agencyComboBox.DataSource = _agencies;
            _agencyComboBox.DisplayMember = "Name";
            _agencyComboBox.ValueMember = "Id";

            if (_agencies.Any())
            {
                _agencyComboBox.SelectedIndex = 0;
                UpdateStationComboBox();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AgencyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateStationComboBox();
    }

    private void UpdateStationComboBox()
    {
        if (_agencyComboBox.SelectedValue is int selectedAgencyId)
        {
            var agencyStations = _stations.Where(s => s.AgencyId == selectedAgencyId).ToList();
            agencyStations.Insert(0, new StationDto { Id = -1, Name = "No Station (Dispatcher only)" });

            _stationComboBox.DataSource = agencyStations;
            _stationComboBox.DisplayMember = "Name";
            _stationComboBox.ValueMember = "Id";

            if (agencyStations.Any())
                _stationComboBox.SelectedIndex = 0;
        }
    }

    private async void CreateButton_Click(object? sender, EventArgs e)
    {
        if (!ValidateInput())
            return;

        _createButton.Enabled = false;
        _createButton.Text = "Creating...";

        try
        {
            var userRole = _roleComboBox.SelectedItem?.ToString() == "Dispatcher" ? UserRole.Dispatcher : UserRole.Member;
            var stationId = _stationComboBox.SelectedValue is int id && id != -1 ? id : (int?)null;

            var createUserDto = new CreateUserDto
            {
                Name = _nameTextBox.Text.Trim(),
                Email = _emailTextBox.Text.Trim(),
                Password = _passwordTextBox.Text,
                Role = userRole,
                AgencyId = (int)_agencyComboBox.SelectedValue!,
                StationId = stationId,
                SupabaseUserId = Guid.NewGuid().ToString() // Generate a unique ID for now
            };

            var result = await _apiClient.CreateUserAsync(createUserDto);
            if (result != null)
            {
                MessageBox.Show("User created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating user: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _createButton.Enabled = true;
            _createButton.Text = "Create User";
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_nameTextBox.Text))
        {
            MessageBox.Show("Please enter a full name.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _nameTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_emailTextBox.Text) || !_emailTextBox.Text.Contains("@"))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _emailTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_passwordTextBox.Text) || _passwordTextBox.Text.Length < 6)
        {
            MessageBox.Show("Please enter a password with at least 6 characters.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _passwordTextBox.Focus();
            return false;
        }

        if (_agencyComboBox.SelectedValue == null)
        {
            MessageBox.Show("Please select an agency.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _agencyComboBox.Focus();
            return false;
        }

        // Validate that Members must have a station
        if (_roleComboBox.SelectedItem?.ToString() == "Member" &&
            (_stationComboBox.SelectedValue == null || (int)_stationComboBox.SelectedValue == -1))
        {
            MessageBox.Show("Members must be assigned to a station. Only Dispatchers can work without a specific station.",
                "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _stationComboBox.Focus();
            return false;
        }

        return true;
    }
}