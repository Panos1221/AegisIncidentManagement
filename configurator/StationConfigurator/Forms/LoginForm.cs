using StationConfigurator.Services;

namespace StationConfigurator.Forms;

public partial class LoginForm : Form
{
    private readonly ApiClient _apiClient;
    private TextBox _emailTextBox;
    private TextBox _passwordTextBox;
    private Button _loginButton;

    public LoginForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Form setup
        Text = "Station Configurator - Admin Login";
        Size = new Size(400, 280);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Title label
        var titleLabel = new Label
        {
            Text = "Station Configurator",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            ForeColor = Color.FromArgb(0, 102, 204),
            Size = new Size(350, 30),
            Location = new Point(25, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var subtitleLabel = new Label
        {
            Text = "Administrative Access Required",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Size = new Size(350, 20),
            Location = new Point(25, 55),
            TextAlign = ContentAlignment.MiddleCenter
        };

        // Email controls
        var emailLabel = new Label
        {
            Text = "Email:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 25),
            Location = new Point(30, 95),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _emailTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(110, 95),
            Text = "dispatcher@fireservice.gr"
        };

        // Password controls
        var passwordLabel = new Label
        {
            Text = "Password:",
            Font = new Font("Segoe UI", 10),
            Size = new Size(80, 25),
            Location = new Point(30, 130),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _passwordTextBox = new TextBox
        {
            Font = new Font("Segoe UI", 10),
            Size = new Size(250, 25),
            Location = new Point(110, 130),
            UseSystemPasswordChar = true,
            Text = "1"
        };

        // Login button
        _loginButton = new Button
        {
            Text = "Login",
            Font = new Font("Segoe UI", 10, FontStyle.Bold),
            Size = new Size(100, 35),
            Location = new Point(150, 175),
            BackColor = Color.FromArgb(0, 102, 204),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false
        };
        _loginButton.FlatAppearance.BorderSize = 0;
        _loginButton.Click += LoginButton_Click;

        // Add controls
        Controls.AddRange(new Control[] { titleLabel, subtitleLabel, emailLabel, _emailTextBox, passwordLabel, _passwordTextBox, _loginButton });

        // Set tab order
        _emailTextBox.TabIndex = 0;
        _passwordTextBox.TabIndex = 1;
        _loginButton.TabIndex = 2;

        // Set Enter key handling
        _passwordTextBox.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
                LoginButton_Click(_loginButton, EventArgs.Empty);
        };

        AcceptButton = _loginButton;
    }

    private async void LoginButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_emailTextBox.Text) || string.IsNullOrWhiteSpace(_passwordTextBox.Text))
        {
            MessageBox.Show("Please enter both email and password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _loginButton.Enabled = false;
        _loginButton.Text = "Logging in...";

        try
        {
            var success = await _apiClient.LoginAsync(_emailTextBox.Text.Trim(), _passwordTextBox.Text);
            if (success)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Invalid credentials. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _passwordTextBox.Clear();
                _passwordTextBox.Focus();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _loginButton.Enabled = true;
            _loginButton.Text = "Login";
        }
    }
}