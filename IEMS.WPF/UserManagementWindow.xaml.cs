using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using IEMS.Application.Services;
using IEMS.Core.Entities;

namespace IEMS.WPF
{
    public partial class UserManagementWindow : Window
    {
        private readonly UserService _userService;
        private List<UserDisplayModel> _allUsers;

        public UserManagementWindow(UserService userService)
        {
            InitializeComponent();
            _userService = userService;
            _allUsers = new List<UserDisplayModel>();

            Loaded += async (s, e) => await LoadUsers();
        }

        private async System.Threading.Tasks.Task LoadUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                _allUsers = users.Select(u => new UserDisplayModel
                {
                    Id = u.Id,
                    Username = u.Username,
                    FullName = u.FullName,
                    Role = u.Role,
                    Email = u.Email ?? string.Empty, // DB column is nullable; keep the display model non-null so search can't NRE
                    IsActive = u.IsActive,
                    LastLogin = u.LastLogin,
                    CreatedDate = u.CreatedDate,
                    CreatedBy = u.CreatedBy,
                    TwoFactorEnabled = u.TwoFactorEnabled
                }).ToList();

                ApplyFilters();
                lblStatus.Text = $"Loaded {_allUsers.Count} user(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading users";
            }
        }

        private void ApplyFilters()
        {
            if (_allUsers == null || dgUsers == null)
                return;

            var filtered = _allUsers.AsEnumerable();

            // Search filter
            if (txtSearch != null && !string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                var searchText = txtSearch.Text.ToLower();
                filtered = filtered.Where(u =>
                    u.Username.ToLower().Contains(searchText) ||
                    u.FullName.ToLower().Contains(searchText) ||
                    u.Email.ToLower().Contains(searchText));
            }

            // Role filter
            if (cmbRoleFilter != null && cmbRoleFilter.SelectedIndex > 0)
            {
                var selectedRole = ((ComboBoxItem)cmbRoleFilter.SelectedItem).Content.ToString();
                filtered = filtered.Where(u => u.Role == selectedRole);
            }

            // Status filter
            if (cmbStatusFilter != null)
            {
                if (cmbStatusFilter.SelectedIndex == 1) // Active
                {
                    filtered = filtered.Where(u => u.IsActive);
                }
                else if (cmbStatusFilter.SelectedIndex == 2) // Disabled
                {
                    filtered = filtered.Where(u => !u.IsActive);
                }
            }

            dgUsers.ItemsSource = filtered.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbRoleFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbStatusFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private async void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var addEditWindow = new AddEditUserWindow(_userService, null);
                if (addEditWindow.ShowDialog() == true)
                {
                    await LoadUsers();
                    lblStatus.Text = "User added successfully";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add user window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if it's from DataGrid row button or top button
                var button = sender as Button;
                var user = button?.Tag as UserDisplayModel;

                // If no user tagged, get from selected row
                if (user == null && dgUsers.SelectedItem is UserDisplayModel selectedUser)
                {
                    user = selectedUser;
                }

                if (user == null)
                {
                    MessageBox.Show("Please select a user to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Get full user entity
                var fullUser = await _userService.GetByIdAsync(user.Id);
                if (fullUser != null)
                {
                    var addEditWindow = new AddEditUserWindow(_userService, fullUser);
                    if (addEditWindow.ShowDialog() == true)
                    {
                        await LoadUsers();
                        lblStatus.Text = "User updated successfully";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error editing user: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnResetPassword_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if it's from DataGrid row button or top button
                var button = sender as Button;
                var user = button?.Tag as UserDisplayModel;

                // If no user tagged, get from selected row
                if (user == null && dgUsers.SelectedItem is UserDisplayModel selectedUser)
                {
                    user = selectedUser;
                }

                if (user == null)
                {
                    MessageBox.Show("Please select a user to reset password.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prompt for new password
                var passwordWindow = new ResetPasswordWindow();
                if (passwordWindow.ShowDialog() == true)
                {
                    var newPassword = passwordWindow.NewPassword;

                    var result = MessageBox.Show(
                        $"Are you sure you want to reset the password for user '{user.Username}'?\n\nThe user will be required to change the password on next login.",
                        "Confirm Password Reset",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _userService.ResetPasswordAsync(user.Id, newPassword, LoginWindow.CurrentUser?.Username ?? "System");

                        MessageBox.Show(
                            $"Password reset successfully for user '{user.Username}'!\n\nThe password has been reset. Please provide the new password to the user through a secure channel.\n\nThe user will be required to change the password on next login.",
                            "Password Reset",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        lblStatus.Text = $"Password reset for user '{user.Username}'";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnToggleStatus_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if it's from DataGrid row button or top button
                var button = sender as Button;
                var user = button?.Tag as UserDisplayModel;

                // If no user tagged, get from selected row
                if (user == null && dgUsers.SelectedItem is UserDisplayModel selectedUser)
                {
                    user = selectedUser;
                }

                if (user == null)
                {
                    MessageBox.Show("Please select a user to toggle status.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Prevent disabling yourself
                if (user.Username == LoginWindow.CurrentUser?.Username)
                {
                    MessageBox.Show("You cannot disable your own account!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var action = user.IsActive ? "disable" : "enable";
                var result = MessageBox.Show(
                    $"Are you sure you want to {action} user '{user.Username}'?",
                    $"Confirm {action.ToUpper()}",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    if (user.IsActive)
                    {
                        await _userService.DisableUserAsync(user.Id, LoginWindow.CurrentUser?.Username ?? "System");
                    }
                    else
                    {
                        await _userService.EnableUserAsync(user.Id, LoginWindow.CurrentUser?.Username ?? "System");
                    }

                    await LoadUsers();
                    lblStatus.Text = $"User '{user.Username}' {action}d successfully";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error toggling user status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnTwoFactor_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dgUsers.SelectedItem is not UserDisplayModel user)
                {
                    MessageBox.Show("Please select a user.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                bool isSelf = user.Id == (LoginWindow.CurrentUser?.Id ?? -1);

                // 2FA is registered against the user's own phone, so only the account owner can turn
                // it ON. An admin can only turn an existing one OFF (recovery), handled inside the window.
                if (!user.TwoFactorEnabled && !isSelf)
                {
                    MessageBox.Show(
                        $"Two-factor authentication is set up by each user on their own account, using their own phone.\n\nSign in as '{user.Username}' to enable it.",
                        "Two-Factor Authentication", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Read the latest 2FA state from a fresh scope so the window opens in the right mode
                // even if it was just changed (this window's DbContext may hold a cached copy).
                IEMS.Core.Entities.User? fullUser;
                using (var scope = App.ServiceProvider.CreateScope())
                {
                    var users = scope.ServiceProvider.GetRequiredService<UserService>();
                    fullUser = await users.GetByIdAsync(user.Id);
                }
                if (fullUser == null) return;

                var window = new TwoFactorWindow(
                    fullUser.Id, fullUser.Username, fullUser.FullName, isSelf,
                    fullUser.TwoFactorEnabled, fullUser.TwoFactorBackupCodes) { Owner = this };

                if (window.ShowDialog() == true)
                {
                    await LoadUsers();
                    lblStatus.Text = $"Two-factor settings updated for '{user.Username}'";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error managing two-factor authentication: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

    // Display model for DataGrid
    public class UserDisplayModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public bool TwoFactorEnabled { get; set; }

        public string TwoFactorDisplay => TwoFactorEnabled ? "🔐 On" : "Off";
        public string StatusDisplay => IsActive ? "Active" : "Disabled";
        public string LastLoginDisplay => LastLogin?.ToString("MM/dd/yyyy HH:mm") ?? "Never";
        public string ToggleButtonText => IsActive ? "🚫 Disable" : "✅ Enable";
    }
}