using System;
using System.Buffers.Binary;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace IEMS.Core.Services
{
    public class PasswordHashingService
    {
        private const int SaltSize = 16;           // 128 bits
        private const int HashSize = 32;           // 256 bits
        private const int IterationHeaderSize = 4; // big-endian Int32 prepended to new hashes

        // OWASP 2025 guidance for PBKDF2-HMAC-SHA256 is >= 600,000 iterations.
        private const int CurrentIterations = 600_000;
        // Hashes created before versioning were 48 bytes (no header) and used this count.
        private const int LegacyIterations = 10_000;

        public string HashPassword(string password)
        {
            // Generate a random salt
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: CurrentIterations,
                numBytesRequested: HashSize);

            // Versioned layout: [iterations:4 big-endian][salt:16][hash:32]. Storing the iteration
            // count means it can be raised again later without breaking existing stored hashes.
            byte[] hashBytes = new byte[IterationHeaderSize + SaltSize + HashSize];
            BinaryPrimitives.WriteInt32BigEndian(hashBytes.AsSpan(0, IterationHeaderSize), CurrentIterations);
            Array.Copy(salt, 0, hashBytes, IterationHeaderSize, SaltSize);
            Array.Copy(hash, 0, hashBytes, IterationHeaderSize + SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                    return false;

                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                int iterations;
                int saltOffset;

                if (hashBytes.Length == IterationHeaderSize + SaltSize + HashSize)
                {
                    // New versioned hash: read the iteration count from the header.
                    iterations = BinaryPrimitives.ReadInt32BigEndian(hashBytes.AsSpan(0, IterationHeaderSize));
                    saltOffset = IterationHeaderSize;
                }
                else if (hashBytes.Length == SaltSize + HashSize)
                {
                    // Legacy hash (pre-versioning): fixed 10,000 iterations, no header.
                    iterations = LegacyIterations;
                    saltOffset = 0;
                }
                else
                {
                    // Truncated/corrupt stored hash.
                    return false;
                }

                if (iterations <= 0)
                    return false;

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, saltOffset, salt, 0, SaltSize);

                byte[] storedHash = new byte[HashSize];
                Array.Copy(hashBytes, saltOffset + SaltSize, storedHash, 0, HashSize);

                byte[] hash = KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: iterations,
                    numBytesRequested: HashSize);

                // Constant-time comparison to prevent timing attacks.
                return CryptographicOperations.FixedTimeEquals(storedHash, hash);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// True if a stored hash uses an older/weaker format or iteration count than the current
        /// policy, so callers can transparently re-hash on a successful login.
        /// </summary>
        public bool NeedsRehash(string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(hashedPassword))
                    return true;

                byte[] hashBytes = Convert.FromBase64String(hashedPassword);
                if (hashBytes.Length != IterationHeaderSize + SaltSize + HashSize)
                    return true; // legacy/unknown layout

                int iterations = BinaryPrimitives.ReadInt32BigEndian(hashBytes.AsSpan(0, IterationHeaderSize));
                return iterations < CurrentIterations;
            }
            catch
            {
                return true;
            }
        }
    }
}
