using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class CreatePersonnelForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly List<StationDto> _stations;
    private TextBox _nameTextBox;
    private TextBox _rankTextBox;
    private TextBox _badgeNumberTextBox;
    private ComboBox _agencyComboBox;
    private ComboBox _stationComboBox;
    private CheckBox _isActiveCheckBox;
    private Button _createButton;
    private Button _cancelButton;
    private List<AgencyDto> _agencies;

    public CreatePersonnelForm(ApiClient apiClient, List<StationDto> stations)
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
        Text = "Add New Personnel";
        Size = new Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Title
        var titleLabel = new Label
        {
            Text = "Add New Personnel",
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
            Size = new Size(120, 25),
            Location = new Point(30, 70)
        };

        _nameTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(160, 70),
            PlaceholderText = "Enter full name"
        };

        // Rank
        var rankLabel = new Label
        {
            Text = "Rank/Position:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 110)
        };

        _rankTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(160, 110),
            PlaceholderText = "e.g., Firefighter, Captain, Lieutenant"
        };

        // Badge Number
        var badgeLabel = new Label
        {
            Text = "Badge Number:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 150)
        };

        _badgeNumberTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(160, 150),
            PlaceholderText = "Optional badge/ID number"
        };

        // Agency
        var agencyLabel = new Label
        {
            Text = "Agency:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 190)
        };

        _agencyComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(160, 190),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _agencyComboBox.SelectedIndexChanged += AgencyComboBox_SelectedIndexChanged;

        // Station
        var stationLabel = new Label
        {
            Text = "Station:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 230)
        };

        _stationComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(280, 25),
            Location = new Point(160, 230),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Active checkbox
        _isActiveCheckBox = new CheckBox
        {
            Text = "Personnel is active",
            Font = new Font("Segoe UI", 10),
            Size = new Size(200, 25),
            Location = new Point(160, 270),
            Checked = true
        };

        // Note
        var noteLabel = new Label
        {
            Text = "Note: This creates personnel records for station management.\nFor user accounts with login access, use the Users tab.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Size = new Size(400, 35),
            Location = new Point(30, 300)
        };

        // Buttons
        _createButton = new Button
        {
            Text = "Add Personnel",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(250, 350),
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
            Location = new Point(380, 350),
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
            titleLabel, nameLabel, _nameTextBox, rankLabel, _rankTextBox,
            badgeLabel, _badgeNumberTextBox, agencyLabel, _agencyComboBox,
            stationLabel, _stationComboBox, _isActiveCheckBox, noteLabel,
            _createButton, _cancelButton
        });

        // Set tab order
        _nameTextBox.TabIndex = 0;
        _rankTextBox.TabIndex = 1;
        _badgeNumberTextBox.TabIndex = 2;
        _agencyComboBox.TabIndex = 3;
        _stationComboBox.TabIndex = 4;
        _isActiveCheckBox.TabIndex = 5;
        _createButton.TabIndex = 6;
        _cancelButton.TabIndex = 7;

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
        _createButton.Text = "Adding...";

        try
        {
            var createPersonnelDto = new CreatePersonnelDto
            {
                Name = _nameTextBox.Text.Trim(),
                Rank = _rankTextBox.Text.Trim(),
                BadgeNumber = string.IsNullOrWhiteSpace(_badgeNumberTextBox.Text) ? null : _badgeNumberTextBox.Text.Trim(),
                AgencyId = (int)_agencyComboBox.SelectedValue!,
                StationId = (int)_stationComboBox.SelectedValue!,
                IsActive = _isActiveCheckBox.Checked
            };

            var result = await _apiClient.CreatePersonnelAsync(createPersonnelDto);
            if (result != null)
            {
                MessageBox.Show($"Personnel '{result.Name}' added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adding personnel: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _createButton.Enabled = true;
            _createButton.Text = "Add Personnel";
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

        if (string.IsNullOrWhiteSpace(_rankTextBox.Text))
        {
            MessageBox.Show("Please enter a rank or position.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _rankTextBox.Focus();
            return false;
        }

        if (_agencyComboBox.SelectedValue == null)
        {
            MessageBox.Show("Please select an agency.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _agencyComboBox.Focus();
            return false;
        }

        if (_stationComboBox.SelectedValue == null)
        {
            MessageBox.Show("Please select a station.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _stationComboBox.Focus();
            return false;
        }

        return true;
    }
}