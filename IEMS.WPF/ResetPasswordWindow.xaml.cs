using System.Linq;
using System.Windows;

namespace IEMS.WPF
{
    public partial class ResetPasswordWindow : Window
    {
        public string NewPassword { get; private set; } = string.Empty;

        public ResetPasswordWindow()
        {
            InitializeComponent();
            txtNewPassword.Focus();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Hide previous errors
            txtError.Visibility = Visibility.Collapsed;

            // Validate
            if (string.IsNullOrWhiteSpace(txtNewPassword.Password))
            {
                ShowError("Password is required");
                txtNewPassword.Focus();
                return;
            }

            var password = txtNewPassword.Password;

            // Shared password policy (same rule the UserService enforces).
            var (pwValid, pwError) = IEMS.Core.Services.PasswordPolicy.Validate(password);
            if (!pwValid)
            {
                ShowError(pwError);
                txtNewPassword.Focus();
                return;
            }

            if (txtNewPassword.Password != txtConfirmPassword.Password)
            {
                ShowError("Passwords do not match");
                txtConfirmPassword.Focus();
                return;
            }

            NewPassword = txtNewPassword.Password;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}