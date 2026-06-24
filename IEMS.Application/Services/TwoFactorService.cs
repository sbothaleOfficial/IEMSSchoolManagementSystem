using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IEMS.Core.Interfaces;
using IEMS.Core.Services;

namespace IEMS.Application.Services
{
    /// <summary>
    /// Manages optional two-factor authentication (authenticator-app TOTP) for user accounts:
    /// enabling/disabling, single-use backup recovery codes, and verifying a code at login.
    /// The TOTP maths lives in <see cref="TotpService"/>; this service is the database-aware layer.
    /// </summary>
    public class TwoFactorService
    {
        private readonly IUserRepository _userRepository;

        private const int BackupCodeCount = 10;
        // Characters used for backup codes — no 0/O/1/I/L to avoid confusion when written down.
        private const string BackupCodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public TwoFactorService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        /// <summary>
        /// Turns on 2FA for a user once they have proven they can produce a valid code from the
        /// supplied secret (the caller validates the first code with <see cref="TotpService"/>).
        /// Generates and returns the plaintext backup codes — these are shown to the user ONCE and
        /// only their hashes are stored.
        /// </summary>
        public async Task<List<string>> EnableAsync(int userId, string secretBase32, string modifiedBy)
        {
            if (string.IsNullOrWhiteSpace(secretBase32))
                throw new InvalidOperationException("A two-factor secret is required.");

            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found.");

            var codes = GenerateBackupCodes();

            user.TwoFactorSecret = secretBase32.Trim();
            user.TwoFactorEnabled = true;
            user.TwoFactorBackupCodes = SerializeHashedCodes(codes);
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
            return codes;
        }

        /// <summary>Turns off 2FA and clears the secret and backup codes.</summary>
        public async Task DisableAsync(int userId, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found.");

            user.TwoFactorEnabled = false;
            user.TwoFactorSecret = null;
            user.TwoFactorBackupCodes = null;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
        }

        /// <summary>Issues a fresh set of backup codes, invalidating any previous ones.</summary>
        public async Task<List<string>> RegenerateBackupCodesAsync(int userId, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found.");
            if (!user.TwoFactorEnabled)
                throw new InvalidOperationException("Two-factor authentication is not enabled for this user.");

            var codes = GenerateBackupCodes();
            user.TwoFactorBackupCodes = SerializeHashedCodes(codes);
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
            return codes;
        }

        /// <summary>
        /// Verifies a login code: first as a TOTP code from the authenticator app, then as a
        /// single-use backup code. A matched backup code is consumed (marked used) and persisted.
        /// </summary>
        public async Task<bool> VerifyAsync(int userId, string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.TwoFactorEnabled || string.IsNullOrEmpty(user.TwoFactorSecret))
                return false;

            // 1) Normal path: a 6-digit code from the authenticator app.
            if (TotpService.ValidateCode(user.TwoFactorSecret, code))
                return true;

            // 2) Fallback: a one-time backup code.
            return await TryConsumeBackupCodeAsync(user, code);
        }

        /// <summary>Number of unused backup codes remaining, for display.</summary>
        public int CountRemainingBackupCodes(string? backupCodesJson)
        {
            var entries = DeserializeCodes(backupCodesJson);
            return entries.Count(e => !e.Used);
        }

        // ----- internals -----

        private async Task<bool> TryConsumeBackupCodeAsync(IEMS.Core.Entities.User user, string code)
        {
            var entries = DeserializeCodes(user.TwoFactorBackupCodes);
            if (entries.Count == 0) return false;

            var hash = HashBackupCode(code);
            var match = entries.FirstOrDefault(e => !e.Used &&
                CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(e.Hash), Convert.FromHexString(hash)));

            if (match == null) return false;

            match.Used = true;
            user.TwoFactorBackupCodes = JsonSerializer.Serialize(entries);
            user.ModifiedDate = DateTime.Now;
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private static List<string> GenerateBackupCodes()
        {
            var codes = new List<string>(BackupCodeCount);
            for (int i = 0; i < BackupCodeCount; i++)
                codes.Add(GenerateBackupCode());
            return codes;
        }

        private static string GenerateBackupCode()
        {
            // 10 characters grouped as XXXXX-XXXXX for readability.
            var chars = new char[10];
            Span<byte> rnd = stackalloc byte[10];
            RandomNumberGenerator.Fill(rnd);
            for (int i = 0; i < chars.Length; i++)
                chars[i] = BackupCodeAlphabet[rnd[i] % BackupCodeAlphabet.Length];
            return new string(chars, 0, 5) + "-" + new string(chars, 5, 5);
        }

        private static string SerializeHashedCodes(IEnumerable<string> codes)
            => JsonSerializer.Serialize(codes.Select(c => new BackupCode { Hash = HashBackupCode(c), Used = false }).ToList());

        private static List<BackupCode> DeserializeCodes(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<BackupCode>();
            try { return JsonSerializer.Deserialize<List<BackupCode>>(json) ?? new List<BackupCode>(); }
            catch { return new List<BackupCode>(); }
        }

        private static string HashBackupCode(string code)
        {
            // Normalise (ignore dashes/spaces/case) so the way it is typed doesn't matter, then
            // SHA-256. Backup codes are high-entropy random, so a fast hash is appropriate.
            var normalized = code.Replace("-", "").Replace(" ", "").Trim().ToUpperInvariant();
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            return Convert.ToHexString(bytes);
        }

        private class BackupCode
        {
            public string Hash { get; set; } = string.Empty;
            public bool Used { get; set; }
        }
    }
}
