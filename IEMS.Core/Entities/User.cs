using System;

namespace IEMS.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Admin, Principal, Teacher, Clerk, Accountant
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public bool MustChangePassword { get; set; } = false;

        // ----- Two-factor authentication (TOTP / authenticator app) -----
        // Opt-in per user. When enabled, login requires a 6-digit code from the user's
        // authenticator app in addition to the password.
        public bool TwoFactorEnabled { get; set; } = false;
        // Base32-encoded shared secret registered with the authenticator app. Null when 2FA is off.
        public string? TwoFactorSecret { get; set; }
        // JSON array of single-use recovery codes (stored as SHA-256 hashes) for when the phone is
        // unavailable. Null when 2FA is off.
        public string? TwoFactorBackupCodes { get; set; }
    }
}