using StationConfigurator.Services;
using StationConfigurator.Models;

namespace StationConfigurator.Forms;

public partial class CreateVehicleForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly List<StationDto> _stations;
    private TextBox _callsignTextBox;
    private TextBox _typeTextBox;
    private TextBox _plateNumberTextBox;
    private ComboBox _stationComboBox;
    private NumericUpDown _waterCapacityNumericUpDown;
    private Button _createButton;
    private Button _cancelButton;

    public CreateVehicleForm(ApiClient apiClient, List<StationDto> stations)
    {
        _apiClient = apiClient;
        _stations = stations;
        InitializeComponent();
        LoadData();
    }

    private void InitializeComponent()
    {
        // Form setup
        Text = "Create New Vehicle";
        Size = new Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Title
        var titleLabel = new Label
        {
            Text = "Create New Vehicle",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Size = new Size(400, 30),
            Location = new Point(30, 20)
        };

        // Callsign
        var callsignLabel = new Label
        {
            Text = "Callsign:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 70)
        };

        _callsignTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(160, 70),
            PlaceholderText = "e.g., E-101, T-201, R-301"
        };

        // Type
        var typeLabel = new Label
        {
            Text = "Vehicle Type:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 110)
        };

        _typeTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(160, 110),
            PlaceholderText = "e.g., Fire Engine, Tanker, Rescue"
        };

        // Plate Number
        var plateLabel = new Label
        {
            Text = "Plate Number:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 150)
        };

        _plateNumberTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(160, 150),
            PlaceholderText = "Vehicle registration number"
        };

        // Station
        var stationLabel = new Label
        {
            Text = "Assigned Station:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 190)
        };

        _stationComboBox = new ComboBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(160, 190),
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Water Capacity
        var waterCapacityLabel = new Label
        {
            Text = "Water Capacity:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(120, 25),
            Location = new Point(30, 230)
        };

        _waterCapacityNumericUpDown = new NumericUpDown
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(150, 25),
            Location = new Point(160, 230),
            Minimum = 0,
            Maximum = 50000,
            DecimalPlaces = 0,
            Value = 3000,
            ThousandsSeparator = true
        };

        var litersLabel = new Label
        {
            Text = "liters",
            Font = new Font("Segoe UI", 10),
            Size = new Size(50, 25),
            Location = new Point(320, 230)
        };

        // Note
        var noteLabel = new Label
        {
            Text = "Note: Vehicle will be created with 'Available' status. Other properties\ncan be updated later through the main application.",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Size = new Size(400, 35),
            Location = new Point(30, 270)
        };

        // Buttons
        _createButton = new Button
        {
            Text = "Create Vehicle",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(120, 35),
            Location = new Point(230, 320),
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
            Location = new Point(360, 320),
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
            titleLabel, callsignLabel, _callsignTextBox, typeLabel, _typeTextBox,
            plateLabel, _plateNumberTextBox, stationLabel, _stationComboBox,
            waterCapacityLabel, _waterCapacityNumericUpDown, litersLabel, noteLabel,
            _createButton, _cancelButton
        });

        // Set tab order
        _callsignTextBox.TabIndex = 0;
        _typeTextBox.TabIndex = 1;
        _plateNumberTextBox.TabIndex = 2;
        _stationComboBox.TabIndex = 3;
        _waterCapacityNumericUpDown.TabIndex = 4;
        _createButton.TabIndex = 5;
        _cancelButton.TabIndex = 6;

        AcceptButton = _createButton;
        CancelButton = _cancelButton;
    }

    private void LoadData()
    {
        try
        {
            _stationComboBox.DataSource = _stations;
            _stationComboBox.DisplayMember = "Name";
            _stationComboBox.ValueMember = "Id";

            if (_stations.Any())
                _stationComboBox.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading stations: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            var createVehicleDto = new CreateVehicleDto
            {
                Callsign = _callsignTextBox.Text.Trim(),
                Type = _typeTextBox.Text.Trim(),
                PlateNumber = _plateNumberTextBox.Text.Trim(),
                StationId = (int)_stationComboBox.SelectedValue!,
                WaterCapacityLiters = _waterCapacityNumericUpDown.Value > 0 ? (double)_waterCapacityNumericUpDown.Value : null
            };

            var result = await _apiClient.CreateVehicleAsync(createVehicleDto);
            if (result != null)
            {
                MessageBox.Show($"Vehicle '{result.Callsign}' created successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating vehicle: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _createButton.Enabled = true;
            _createButton.Text = "Create Vehicle";
        }
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(_callsignTextBox.Text))
        {
            MessageBox.Show("Please enter a callsign for the vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _callsignTextBox.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(_typeTextBox.Text))
        {
            MessageBox.Show("Please enter a vehicle type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _typeTextBox.Focus();
            return false;
        }

        if (_stationComboBox.SelectedValue == null)
        {
            MessageBox.Show("Please select a station for the vehicle.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            _stationComboBox.Focus();
            return false;
        }

        return true;
    }
}