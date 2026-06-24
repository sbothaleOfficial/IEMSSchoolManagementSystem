using System;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using IEMS.Application.Services;
using IEMS.Core.Services;

namespace IEMS.WPF
{
    /// <summary>
    /// Self-service password change reachable straight from the login screen — the user proves
    /// ownership with their current password, so no administrator is needed. This is the local-app
    /// equivalent of a "reset password" link: there is no email server, so recovery relies on the
    /// user knowing their existing password (a fully forgotten password is reset by an admin from
    /// User Management).
    /// </summary>
    public partial class ChangePasswordWindow : Window
    {
        // A single generic message for every credential failure so the screen never reveals
        // whether a username exists or which field was wrong (avoids account enumeration).
        private const string GenericCredentialError = "Username or current password is incorrect.";

        public ChangePasswordWindow(string? prefillUsername = null)
        {
            InitializeComponent();

            if (!string.IsNullOrWhiteSpace(prefillUsername))
            {
                txtUsername.Text = prefillUsername;
                Loaded += (_, _) => txtCurrentPassword.Focus();
            }
            else
            {
                Loaded += (_, _) => txtUsername.Focus();
            }
        }

        private async void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;

            var username = txtUsername.Text.Trim();
            var currentPassword = txtCurrentPassword.Password;
            var newPassword = txtNewPassword.Password;
            var confirmPassword = txtConfirmPassword.Password;

            // Field-level validation first (friendly, specific messages).
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(currentPassword))
            {
                ShowError("Please enter your current password.");
                txtCurrentPassword.Focus();
                return;
            }
            if (string.IsNullOrEmpty(newPassword))
            {
                ShowError("Please enter a new password.");
                txtNewPassword.Focus();
                return;
            }

            var (pwValid, pwError) = PasswordPolicy.Validate(newPassword);
            if (!pwValid)
            {
                ShowError(pwError);
                txtNewPassword.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                ShowError("New passwords do not match.");
                txtConfirmPassword.Focus();
                return;
            }

            if (newPassword == currentPassword)
            {
                ShowError("New password must be different from your current password.");
                txtNewPassword.Focus();
                return;
            }

            btnUpdate.IsEnabled = false;
            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();

                // Look up the account; treat "not found" and "disabled" identically to a wrong
                // password so the dialog never leaks which usernames exist.
                var user = await userService.GetByUsernameAsync(username);
                if (user == null || !user.IsActive)
                {
                    ShowError(GenericCredentialError);
                    txtCurrentPassword.Focus();
                    return;
                }

                // ChangePasswordAsync verifies the current password, enforces the password policy,
                // updates the hash and clears the MustChangePassword flag.
                await userService.ChangePasswordAsync(user.Id, currentPassword, newPassword);

                MessageBox.Show(
                    "Your password has been changed successfully.\n\nPlease sign in with your new password.",
                    "Password Changed", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (InvalidOperationException ex)
            {
                // "Current password is incorrect." is folded into the generic message; any other
                // validation error (e.g. a policy message) is shown as-is.
                if (ex.Message.IndexOf("current password", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    ShowError(GenericCredentialError);
                    txtCurrentPassword.Focus();
                }
                else
                {
                    ShowError(ex.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Could not change the password: {ex.Message}");
            }
            finally
            {
                btnUpdate.IsEnabled = true;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}
