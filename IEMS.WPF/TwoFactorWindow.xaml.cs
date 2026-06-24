using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using QRCoder;
using IEMS.Application.Services;
using IEMS.Core.Services;

namespace IEMS.WPF
{
    /// <summary>
    /// Single window that handles the whole two-factor lifecycle for one account:
    ///  • Setup wizard — show a QR code, confirm a code, then reveal one-time backup codes.
    ///  • Manage — regenerate backup codes or turn 2FA off.
    /// Which panel is shown is decided in the constructor from whether 2FA is already enabled.
    /// DialogResult is true when anything changed, so the caller can refresh its list.
    /// </summary>
    public partial class TwoFactorWindow : Window
    {
        private const string Issuer = "IEMS – Inspire English Medium School";

        private readonly int _userId;
        private readonly string _username;
        private readonly bool _isSelf;
        private string _secret = string.Empty;   // pending secret during setup
        private string _backupCodesText = string.Empty;
        private bool _changed;

        private enum Mode { Setup, Backup, Manage }
        private Mode _mode;

        public TwoFactorWindow(int userId, string username, string? fullName, bool isSelf,
                               bool alreadyEnabled, string? backupCodesJson)
        {
            InitializeComponent();
            _userId = userId;
            _username = username;
            _isSelf = isSelf;

            if (alreadyEnabled)
                ShowManageMode(backupCodesJson);
            else
                ShowSetupMode(fullName);
        }

        private string Actor => LoginWindow.CurrentUser?.Username ?? _username;

        // ----- Setup -----

        private void ShowSetupMode(string? fullName)
        {
            _mode = Mode.Setup;
            txtHeader.Text = "Set Up Two-Factor Authentication";
            txtHeaderSub.Text = string.IsNullOrWhiteSpace(fullName)
                ? $"Securing the '{_username}' account" : $"Securing {fullName}'s account";

            _secret = TotpService.GenerateSecret();
            var uri = TotpService.BuildOtpAuthUri(_secret, _username, Issuer);
            imgQr.Source = MakeQr(uri);
            txtManualKey.Text = GroupKey(_secret);

            SetupPanel.Visibility = Visibility.Visible;
            BackupPanel.Visibility = Visibility.Collapsed;
            ManagePanel.Visibility = Visibility.Collapsed;

            btnPrimary.Visibility = Visibility.Visible;
            btnPrimary.Content = "✓ Verify & Enable";
            btnClose.Content = "✕ Cancel";

            Loaded += (_, _) => txtVerifyCode.Focus();
        }

        private async void DoVerifyAndEnable()
        {
            txtSetupError.Visibility = Visibility.Collapsed;
            var code = txtVerifyCode.Text.Trim();
            if (code.Length != 6)
            {
                ShowSetupError("Enter the 6-digit code from your authenticator app.");
                return;
            }

            if (!TotpService.ValidateCode(_secret, code))
            {
                ShowSetupError("That code didn't match. Make sure your phone's time is correct, then try the current code.");
                txtVerifyCode.SelectAll();
                txtVerifyCode.Focus();
                return;
            }

            btnPrimary.IsEnabled = false;
            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var twoFactor = scope.ServiceProvider.GetRequiredService<TwoFactorService>();
                var codes = await twoFactor.EnableAsync(_userId, _secret, Actor);
                _changed = true;
                ShowBackupCodes(codes, "Two-Factor Authentication Enabled");
            }
            catch (Exception ex)
            {
                ShowSetupError($"Could not enable two-factor authentication: {ex.Message}");
                btnPrimary.IsEnabled = true;
            }
        }

        // ----- Backup codes display (after enable or regenerate) -----

        private void ShowBackupCodes(List<string> codes, string header)
        {
            _mode = Mode.Backup;
            _backupCodesText = string.Join(Environment.NewLine, codes);
            txtBackupCodes.Text = string.Join("      ", codes); // wrap-friendly spacing

            txtHeader.Text = header;
            txtHeaderSub.Text = "Save your backup codes before closing";

            SetupPanel.Visibility = Visibility.Collapsed;
            ManagePanel.Visibility = Visibility.Collapsed;
            BackupPanel.Visibility = Visibility.Visible;

            btnPrimary.Visibility = Visibility.Collapsed;
            btnClose.Content = "✓ Done";
        }

        private void BtnCopyCodes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(_backupCodesText);
                MessageBox.Show("Backup codes copied to the clipboard.", "Copied",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not copy: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnSaveCodes_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                Title = "Save backup codes",
                Filter = "Text file (*.txt)|*.txt",
                FileName = $"IEMS_backup_codes_{_username}.txt"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var content =
                    "IEMS School Management System — Two-Factor Backup Codes" + Environment.NewLine +
                    $"Account: {_username}" + Environment.NewLine +
                    "Each code can be used once if you don't have your authenticator app." + Environment.NewLine +
                    new string('-', 50) + Environment.NewLine +
                    _backupCodesText + Environment.NewLine;
                File.WriteAllText(dlg.FileName, content);
                MessageBox.Show("Backup codes saved.", "Saved",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not save the file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ----- Manage (already enabled) -----

        private void ShowManageMode(string? backupCodesJson)
        {
            _mode = Mode.Manage;
            txtHeader.Text = "Two-Factor Authentication";
            txtHeaderSub.Text = $"'{_username}' account";

            using (var scope = App.ServiceProvider.CreateScope())
            {
                var twoFactor = scope.ServiceProvider.GetRequiredService<TwoFactorService>();
                int remaining = twoFactor.CountRemainingBackupCodes(backupCodesJson);
                txtBackupRemaining.Text = $"You have {remaining} unused backup code" + (remaining == 1 ? "" : "s") +
                                          " remaining.";
            }

            // Only the account's own owner can regenerate their codes (they hold the phone).
            btnRegenerate.Visibility = _isSelf ? Visibility.Visible : Visibility.Collapsed;

            if (!_isSelf)
            {
                txtManageNote.Text = "You are managing another user's account as an administrator. " +
                                     "Turning off two-factor here is for recovery when that user has lost access.";
                txtManageNote.Visibility = Visibility.Visible;
            }

            SetupPanel.Visibility = Visibility.Collapsed;
            BackupPanel.Visibility = Visibility.Collapsed;
            ManagePanel.Visibility = Visibility.Visible;

            btnPrimary.Visibility = Visibility.Collapsed;
            btnClose.Content = "✕ Close";
        }

        private async void BtnRegenerate_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                "Generate a new set of backup codes? Your existing backup codes will stop working.",
                "Regenerate Backup Codes", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var twoFactor = scope.ServiceProvider.GetRequiredService<TwoFactorService>();
                var codes = await twoFactor.RegenerateBackupCodesAsync(_userId, Actor);
                _changed = true;
                ShowBackupCodes(codes, "New Backup Codes");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not regenerate backup codes: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnDisable_Click(object sender, RoutedEventArgs e)
        {
            var confirm = MessageBox.Show(
                _isSelf
                    ? "Turn off two-factor authentication for your account? You'll sign in with just your password."
                    : $"Turn off two-factor authentication for '{_username}'? This is an administrator recovery action.",
                "Turn Off Two-Factor", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var scope = App.ServiceProvider.CreateScope();
                var twoFactor = scope.ServiceProvider.GetRequiredService<TwoFactorService>();
                await twoFactor.DisableAsync(_userId, Actor);
                _changed = true;
                MessageBox.Show("Two-factor authentication has been turned off.", "Done",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = _changed;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not turn off two-factor authentication: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----- Footer -----

        private void BtnPrimary_Click(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.Setup) DoVerifyAndEnable();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = _changed;
            Close();
        }

        private void ShowSetupError(string message)
        {
            txtSetupError.Text = message;
            txtSetupError.Visibility = Visibility.Visible;
        }

        // ----- helpers -----

        private static string GroupKey(string key)
        {
            // Display the base32 secret in groups of 4 for easier manual entry.
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < key.Length; i++)
            {
                if (i > 0 && i % 4 == 0) sb.Append(' ');
                sb.Append(key[i]);
            }
            return sb.ToString();
        }

        private static BitmapImage MakeQr(string text)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
            var png = new PngByteQRCode(data).GetGraphic(10);
            using var ms = new MemoryStream(png);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
    }
}
