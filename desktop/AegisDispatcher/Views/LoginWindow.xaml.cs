using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Extensions.DependencyInjection;
using AegisDispatcher.Models;
using AegisDispatcher.Services;

namespace AegisDispatcher.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _authService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILoggingService _loggingService;

        public LoginWindow(IAuthService authService, IServiceProvider serviceProvider, ILoggingService loggingService)
        {
            InitializeComponent();
            _authService = authService;
            _serviceProvider = serviceProvider;
            _loggingService = loggingService;

            // Set up animations and focus
            Loaded += OnWindowLoaded;
            
            _loggingService.LogDebug("LoginWindow initialized");
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Start entrance animations
            StartEntranceAnimations();
            
            // Focus on email textbox after animations
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(800)
            };
            timer.Tick += (s, args) =>
            {
                EmailTextBox.Focus();
                timer.Stop();
            };
            timer.Start();
        }

        private void StartEntranceAnimations()
        {
            // Fade in the entire window
            var fadeInStoryboard = (Storyboard)FindResource("FadeInAnimation");
            fadeInStoryboard.Begin(this);

            // Slide in login form from left
            var slideLeftStoryboard = (Storyboard)FindResource("SlideInFromLeft");
            slideLeftStoryboard.Begin(LoginFormPanel);

            // Slide in demo accounts from right
            var slideRightStoryboard = (Storyboard)FindResource("SlideInFromRight");
            slideRightStoryboard.Begin(DemoAccountsPanel);
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformLogin();
        }

        private async void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformLogin();
            }
        }

        private async void QuickLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is Button button && button.Tag is string email)
                {
                    _loggingService.LogInformation("Quick login attempted for: {Email}", email);
                    
                    EmailTextBox.Text = email;
                    PasswordBox.Password = "1"; // Default development password (matches SeedData)
                    
                    _loggingService.LogDebug("Quick login credentials set, attempting login");
                    await PerformLogin();
                }
                else
                {
                    _loggingService.LogWarning("Quick login button clicked but no email tag found");
                    ShowError("Quick login configuration error. Please use manual login.");
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Error during quick login");
                ShowError($"Quick login failed: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private async Task PerformLogin()
        {
            try
            {
                _loggingService.LogInformation("Login attempt started for: {Email}", EmailTextBox.Text);
                
                // Validate inputs
                if (string.IsNullOrWhiteSpace(EmailTextBox.Text))
                {
                    _loggingService.LogWarning("Login attempt failed: empty email");
                    ShowError("Please enter your email address.");
                    EmailTextBox.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    _loggingService.LogWarning("Login attempt failed: empty password");
                    ShowError("Please enter your password.");
                    PasswordBox.Focus();
                    return;
                }

                // Show loading state
                SetLoadingState(true);
                HideError();

                var loginRequest = new LoginRequest
                {
                    Email = EmailTextBox.Text.Trim(),
                    Password = PasswordBox.Password
                };

                _loggingService.LogDebug("Calling AuthService.LoginAsync");
                var user = await _authService.LoginAsync(loginRequest);

                if (user != null)
                {
                    _loggingService.LogInformation("User authenticated: {Email}, Role: {Role}", user.Email, user.Role);
                    
                    // Check if user is a dispatcher
                    if (!_authService.IsDispatcher())
                    {
                        _loggingService.LogWarning("Access denied for non-dispatcher user: {Email}, Role: {Role}", user.Email, user.Role);
                        ShowError("Access denied. Only dispatchers can use this application.");
                        _authService.Logout();
                        SetLoadingState(false);
                        return;
                    }

                    // Login successful, open main window
                    _loggingService.LogInformation("Login successful, opening main window");
                    await OpenMainWindow();
                }
                else
                {
                    _loggingService.LogWarning("Authentication failed: invalid credentials for {Email}", EmailTextBox.Text);
                    ShowError("Invalid email or password. Please try again.");
                    SetLoadingState(false);
                    PasswordBox.Focus();
                    PasswordBox.SelectAll();
                }
            }
            catch (AuthenticationException ex)
            {
                _loggingService.LogError(ex, "Authentication exception during login for {Email}", EmailTextBox.Text);
                ShowError($"Login failed: {ex.Message}");
                SetLoadingState(false);
            }
            catch (ApiException ex)
            {
                _loggingService.LogError(ex, "API exception during login for {Email}", EmailTextBox.Text);
                ShowError($"Connection error: {ex.Message}");
                SetLoadingState(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError(ex, "Unexpected error during login for {Email}", EmailTextBox.Text);
                ShowError($"An unexpected error occurred: {ex.Message}");
                SetLoadingState(false);
            }
        }

        private async Task OpenMainWindow()
        {
            try
            {
                // Create and show the main window
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                
                // Hide this login window
                this.Hide();
                
                // Show main window
                mainWindow.Show();
                
                // Close login window after main window is shown
                this.Close();
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open main application: {ex.Message}");
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            EmailTextBox.IsEnabled = !isLoading;
            PasswordBox.IsEnabled = !isLoading;
            DemoAccountsPanel.IsEnabled = !isLoading;

            if (isLoading)
            {
                LoadingIndicator.Visibility = Visibility.Visible;
                LoginButton.Content = "Signing In...";
            }
            else
            {
                LoadingIndicator.Visibility = Visibility.Collapsed;
                LoginButton.Content = "Sign In";
            }
        }

        private void ShowError(string message)
        {
            ErrorMessage.Text = message;
            ErrorMessage.Visibility = Visibility.Visible;

            // Auto-hide error after 5 seconds
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            timer.Tick += (s, e) =>
            {
                HideError();
                timer.Stop();
            };
            timer.Start();
        }

        private void HideError()
        {
            ErrorMessage.Visibility = Visibility.Collapsed;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Allow dragging the window
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
