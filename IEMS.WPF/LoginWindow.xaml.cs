using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using IEMS.Application.Services;
using IEMS.Core.Entities;
using IEMS.Core.Services;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace IEMS.WPF
{
    public partial class LoginWindow : Window
    {
        public static User? CurrentUser { get; internal set; }
        private const string RememberMeFilePath = "remember_me.json";

        public LoginWindow()
        {
            InitializeComponent();

            // Load remembered username if exists
            LoadRememberedCredentials();

            // Set focus to appropriate field
            Loaded += (s, e) =>
            {
                if (!string.IsNullOrEmpty(txtUsername.Text))
                    txtPassword.Focus();
                else
                    txtUsername.Focus();
            };

            // Handle Enter key press
            KeyDown += LoginWindow_KeyDown;
        }

        private void LoginWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Ignore Enter while a login is already in flight — otherwise repeated Enter
            // presses during the auth delay launch concurrent logins / multiple main windows.
            if (e.Key == Key.Enter && btnLogin.IsEnabled)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            var username = txtUsername.Text.Trim();
            var password = txtPassword.Password;

            // Clear previous error messages
            txtErrorMessage.Visibility = Visibility.Collapsed;

            // Basic validation
            if (string.IsNullOrEmpty(username))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return;
            }

            // Show loading overlay
            ShowLoading(true);

            try
            {
                // Simulate authentication delay
                await Task.Delay(1000);

                // Simple authentication logic (you can enhance this later)
                bool isAuthenticated = await AuthenticateUser(username, password);

                if (isAuthenticated)
                {
                    // Save username if "Remember me" is checked
                    SaveRememberedCredentials(username, chkRememberMe.IsChecked == true);

                    // Authentication successful
                    try
                    {
                        // Create a scope for resolving MainWindow with all its dependencies
                        var scope = App.ServiceProvider.CreateScope();
                        var mainWindow = scope.ServiceProvider.GetRequiredService<MainWindow>();

                        // Store the scope so it doesn't get disposed
                        mainWindow.Tag = scope;

                        // Set this flag to prevent app shutdown when login window closes
                        System.Windows.Application.Current.MainWindow = mainWindow;

                        // Transfer the window state and position
                        mainWindow.WindowState = this.WindowState;
                        if (this.WindowState == WindowState.Normal)
                        {
                            mainWindow.Left = this.Left;
                            mainWindow.Top = this.Top;
                            mainWindow.Width = this.Width;
                            mainWindow.Height = this.Height;
                        }

                        // Show main window first, then close login window
                        mainWindow.Show();

                        // Dispose scope when main window closes
                        mainWindow.Closed += (s, args) =>
                        {
                            if (mainWindow.Tag is IServiceScope serviceScope)
                            {
                                serviceScope.Dispose();
                            }
                        };

                        this.Close();
                    }
                    catch (Exception mainWindowEx)
                    {
                        MessageBox.Show($"Failed to open main window: {mainWindowEx.Message}\n\nInner Exception: {mainWindowEx.InnerException?.Message}\n\nStack trace: {mainWindowEx.StackTrace}",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ShowError($"Failed to open main window. Please check the error details.");
                        ShowLoading(false);
                    }
                }
                else
                {
                    ShowError("Invalid username or password. Please try again.");
                    txtPassword.Clear();
                    txtUsername.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}\n\nStack trace: {ex.StackTrace}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ShowError($"Login failed: {ex.Message}");
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                    // Debug: Check if user exists
                    var existingUser = await userService.GetByUsernameAsync(username);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: User lookup for '{username}': {(existingUser != null ? "Found" : "Not Found")}");
                    if (existingUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"DEBUG: User IsActive: {existingUser.IsActive}");
                        System.Diagnostics.Debug.WriteLine($"DEBUG: User PasswordHash: {existingUser.PasswordHash?.Substring(0, Math.Min(20, existingUser.PasswordHash.Length))}...");
                    }

                    var user = await userService.AuthenticateAsync(username, password);
                    System.Diagnostics.Debug.WriteLine($"DEBUG: Authentication result: {(user != null ? "Success" : "Failed")}");

                    if (user != null)
                    {
                        CurrentUser = user;

                        // Check if user must change password
                        if (user.MustChangePassword)
                        {
                            ShowLoading(false);

                            MessageBox.Show(
                                "Your password must be changed before you can continue.\n\nPlease enter a new password that meets the following requirements:\n" +
                                "• At least 8 characters long\n" +
                                "• Contains uppercase and lowercase letters\n" +
                                "• Contains at least one number\n" +
                                "• Contains at least one special character",
                                "Password Change Required",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            var passwordWindow = new ResetPasswordWindow();
                            if (passwordWindow.ShowDialog() == true)
                            {
                                try
                                {
                                    // Use ChangePasswordAsync which properly handles the password change and clears MustChangePassword flag
                                    // We pass the old password (which we don't have) but since ResetPasswordAsync is for admin reset,
                                    // we'll need to use a different approach for forced password change

                                    // Directly update the password hash and clear the flag
                                    var passwordHashingService = scope.ServiceProvider.GetRequiredService<PasswordHashingService>();
                                    user.PasswordHash = passwordHashingService.HashPassword(passwordWindow.NewPassword);
                                    user.MustChangePassword = false;
                                    user.ModifiedDate = DateTime.Now;
                                    user.ModifiedBy = user.Username;

                                    await userService.UpdateUserAsync(user, user.Username);

                                    MessageBox.Show(
                                        "Your password has been changed successfully.\n\nYou can now access the system.",
                                        "Password Changed",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Information);

                                    ShowLoading(true);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Failed to change password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return false;
                                }
                            }
                            else
                            {
                                MessageBox.Show("You must change your password to access the system.", "Password Change Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return false;
                            }
                        }

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowError(string message)
        {
            txtErrorMessage.Text = message;
            txtErrorMessage.Visibility = Visibility.Visible;
        }

        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            btnLogin.IsEnabled = !show;
        }

        private void TxtResetPassword_Click(object sender, MouseButtonEventArgs e)
        {
            // Self-service password change: the user proves ownership with their current password.
            // Pre-fill whatever username is already typed so they don't re-enter it.
            var dialog = new ChangePasswordWindow(txtUsername.Text.Trim())
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                // Password changed — clear the old password and focus it so they can sign in fresh.
                txtPassword.Clear();
                txtErrorMessage.Visibility = Visibility.Collapsed;
                txtPassword.Focus();
            }
        }

        private void TxtAdminContact_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                // Open phone dialer with the contact number
                Process.Start(new ProcessStartInfo
                {
                    FileName = "tel:+916361986696",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // If phone dialer doesn't work, try opening in default browser
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://wa.me/916361986696",
                        UseShellExecute = true
                    });
                }
                catch
                {
                    MessageBox.Show("Contact Administrator:\nSuraj Bothale\n+91 6361986696",
                                  "Administrator Contact",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // If login window is closed without successful login (MainWindow not set or not visible), exit the application
            var mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow == null || mainWindow == this)
            {
                System.Windows.Application.Current.Shutdown();
            }
            base.OnClosed(e);
        }

        private void LoadRememberedCredentials()
        {
            try
            {
                if (File.Exists(RememberMeFilePath))
                {
                    var json = File.ReadAllText(RememberMeFilePath);
                    var data = JsonSerializer.Deserialize<RememberMeData>(json);

                    if (data != null && data.RememberMe)
                    {
                        txtUsername.Text = data.Username ?? "";
                        chkRememberMe.IsChecked = true;
                    }
                }
            }
            catch
            {
                // If loading fails, just ignore and start fresh
            }
        }

        private void SaveRememberedCredentials(string username, bool rememberMe)
        {
            try
            {
                var data = new RememberMeData
                {
                    Username = rememberMe ? username : "",
                    RememberMe = rememberMe
                };

                var json = JsonSerializer.Serialize(data);
                File.WriteAllText(RememberMeFilePath, json);
            }
            catch
            {
                // If saving fails, just ignore - not critical
            }
        }

        private class RememberMeData
        {
            public string? Username { get; set; }
            public bool RememberMe { get; set; }
        }
    }
}