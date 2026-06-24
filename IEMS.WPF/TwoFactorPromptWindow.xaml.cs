using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using IEMS.Application.Services;

namespace IEMS.WPF
{
    /// <summary>
    /// Login-time second-factor challenge. Shown after the password is verified when the account has
    /// two-factor authentication enabled. Accepts a 6-digit authenticator code or a one-time backup
    /// code. DialogResult is true only when the code is valid.
    /// </summary>
    public partial class TwoFactorPromptWindow : Window
    {
        private readonly int _userId;
        private bool _backupMode;

        public TwoFactorPromptWindow(int userId, string? fullName = null)
        {
            InitializeComponent();
            _userId = userId;

            if (!string.IsNullOrWhiteSpace(fullName))
                txtSubtitle.Text = $"Signing in as {fullName}";

            Loaded += (_, _) => txtCode.Focus();
            KeyDown += (s, e) => { if (e.Key == Key.Enter) BtnVerify_Click(s, e); };
        }

        private void TxtToggle_Click(object sender, MouseButtonEventArgs e)
        {
            _backupMode = !_backupMode;
            txtError.Visibility = Visibility.Collapsed;
            txtCode.Clear();

            if (_backupMode)
            {
                txtPrompt.Text = "Backup code";
                txtCode.MaxLength = 11;            // "XXXXX-XXXXX"
                txtCode.FontSize = 18;
                txtToggle.Text = "Use the authenticator app instead";
                txtSubtitle.Text = "Enter one of your saved backup codes";
            }
            else
            {
                txtPrompt.Text = "Verification code";
                txtCode.MaxLength = 6;
                txtCode.FontSize = 22;
                txtToggle.Text = "Use a backup code instead";
                txtSubtitle.Text = "Enter the 6-digit code from your authenticator app";
            }
            txtCode.Focus();
        }

        private async void BtnVerify_Click(object sender, RoutedEventArgs e)
        {
            txtError.Visibility = Visibility.Collapsed;
            var code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            {
                ShowError(_backupMode ? "Please enter a backup code." : "Please enter the 6-digit code.");
                return;
            }

            btnVerify.IsEnabled = false;
            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var twoFactor = scope.ServiceProvider.GetRequiredService<TwoFactorService>();

                if (await twoFactor.VerifyAsync(_userId, code))
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ShowError(_backupMode
                        ? "That backup code is not valid or has already been used."
                        : "That code is incorrect or has expired. Check your authenticator app and try again.");
                    txtCode.SelectAll();
                    txtCode.Focus();
                }
            }
            catch (Exception ex)
            {
                ShowError($"Could not verify the code: {ex.Message}");
            }
            finally
            {
                btnVerify.IsEnabled = true;
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
