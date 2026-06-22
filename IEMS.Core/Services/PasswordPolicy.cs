using System.Linq;

namespace IEMS.Core.Services
{
    /// <summary>
    /// Single source of truth for the password-strength policy, shared by the
    /// UserService (server-side enforcement) and the WPF password forms. Previously
    /// these rules were copy-pasted into UserService, AddEditUserWindow and
    /// ResetPasswordWindow and could drift apart.
    /// </summary>
    public static class PasswordPolicy
    {
        public const int MinLength = 8;

        /// <summary>Returns (true, null) when the password meets the policy, otherwise (false, reason).</summary>
        public static (bool IsValid, string? Error) Validate(string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return (false, "Password cannot be empty.");
            if (password.Length < MinLength)
                return (false, $"Password must be at least {MinLength} characters long.");
            if (!password.Any(char.IsUpper))
                return (false, "Password must contain at least one uppercase letter.");
            if (!password.Any(char.IsLower))
                return (false, "Password must contain at least one lowercase letter.");
            if (!password.Any(char.IsDigit))
                return (false, "Password must contain at least one number.");
            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return (false, "Password must contain at least one special character.");
            return (true, null);
        }
    }
}
