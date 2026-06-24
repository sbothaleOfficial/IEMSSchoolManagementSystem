using System;
using System.Security.Cryptography;
using System.Text;

namespace IEMS.Core.Services
{
    /// <summary>
    /// Time-based One-Time Password (TOTP) generator/validator per RFC 6238, compatible with
    /// Google Authenticator, Microsoft Authenticator, Authy, etc. Uses the defaults every
    /// authenticator app expects: HMAC-SHA1, a 30-second period and 6 digits. Pure crypto with no
    /// external dependencies, so it works completely offline — no SMS gateway or email required.
    /// </summary>
    public static class TotpService
    {
        private const int Period = 30;       // seconds per code
        private const int Digits = 6;
        private const int SecretBytes = 20;  // 160-bit shared secret (RFC 6238 recommendation)

        /// <summary>Creates a new random base32 secret to register with an authenticator app.</summary>
        public static string GenerateSecret()
        {
            var bytes = new byte[SecretBytes];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base32Encode(bytes);
        }

        /// <summary>
        /// Builds the otpauth:// URI an authenticator app reads from the QR code. The label is
        /// "Issuer:Account" and the issuer is also repeated as a parameter, which is what the major
        /// apps expect.
        /// </summary>
        public static string BuildOtpAuthUri(string secretBase32, string account, string issuer)
        {
            string Esc(string s) => Uri.EscapeDataString(s);
            return $"otpauth://totp/{Esc(issuer)}:{Esc(account)}?secret={secretBase32}" +
                   $"&issuer={Esc(issuer)}&algorithm=SHA1&digits={Digits}&period={Period}";
        }

        /// <summary>
        /// Validates a code against the secret, accepting the current 30-second step plus
        /// <paramref name="window"/> steps either side to tolerate clock drift between the PC and phone.
        /// </summary>
        public static bool ValidateCode(string secretBase32, string code, int window = 1, DateTimeOffset? now = null)
        {
            if (string.IsNullOrWhiteSpace(secretBase32) || string.IsNullOrWhiteSpace(code))
                return false;

            code = code.Trim();
            if (code.Length != Digits)
                return false;

            byte[] key;
            try { key = Base32Decode(secretBase32); }
            catch { return false; }
            if (key.Length == 0) return false;

            long counter = GetCounter(now ?? DateTimeOffset.UtcNow);
            for (long i = -window; i <= window; i++)
            {
                if (FixedTimeEquals(ComputeCode(key, counter + i), code))
                    return true;
            }
            return false;
        }

        /// <summary>Returns the current code for a secret. Used by tests and (optionally) diagnostics.</summary>
        public static string GetCode(string secretBase32, DateTimeOffset? now = null)
        {
            var key = Base32Decode(secretBase32);
            return ComputeCode(key, GetCounter(now ?? DateTimeOffset.UtcNow));
        }

        private static long GetCounter(DateTimeOffset time) => time.ToUnixTimeSeconds() / Period;

        private static string ComputeCode(byte[] key, long counter)
        {
            byte[] counterBytes = BitConverter.GetBytes(counter);
            if (BitConverter.IsLittleEndian) Array.Reverse(counterBytes); // RFC uses big-endian

            using var hmac = new HMACSHA1(key);
            byte[] hash = hmac.ComputeHash(counterBytes);

            // Dynamic truncation (RFC 4226 §5.3).
            int offset = hash[hash.Length - 1] & 0x0F;
            int binary = ((hash[offset] & 0x7F) << 24)
                       | ((hash[offset + 1] & 0xFF) << 16)
                       | ((hash[offset + 2] & 0xFF) << 8)
                       | (hash[offset + 3] & 0xFF);

            int otp = binary % (int)Math.Pow(10, Digits);
            return otp.ToString().PadLeft(Digits, '0');
        }

        private static bool FixedTimeEquals(string a, string b)
            => CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(a), Encoding.ASCII.GetBytes(b));

        // ----- Base32 (RFC 4648, no padding) -----
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string Base32Encode(byte[] data)
        {
            if (data == null || data.Length == 0) return string.Empty;

            var sb = new StringBuilder((data.Length * 8 + 4) / 5);
            int buffer = 0, bitsLeft = 0;
            foreach (byte b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    int index = (buffer >> (bitsLeft - 5)) & 0x1F;
                    bitsLeft -= 5;
                    sb.Append(Base32Alphabet[index]);
                }
                buffer &= (1 << bitsLeft) - 1; // drop consumed high bits so the int can't overflow
            }
            if (bitsLeft > 0)
                sb.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
            return sb.ToString();
        }

        public static byte[] Base32Decode(string input)
        {
            if (string.IsNullOrEmpty(input)) return Array.Empty<byte>();

            input = input.Trim().TrimEnd('=').ToUpperInvariant().Replace(" ", "");
            var output = new System.Collections.Generic.List<byte>(input.Length * 5 / 8);
            int buffer = 0, bitsLeft = 0;
            foreach (char c in input)
            {
                int val = Base32Alphabet.IndexOf(c);
                if (val < 0) throw new FormatException($"Invalid base32 character '{c}'.");
                buffer = (buffer << 5) | val;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
                buffer &= (1 << bitsLeft) - 1;
            }
            return output.ToArray();
        }
    }
}
