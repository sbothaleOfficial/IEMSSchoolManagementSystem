using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.Core.Services;

namespace IEMS.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHashingService _passwordHashingService;

        public UserService(IUserRepository userRepository, PasswordHashingService passwordHashingService)
        {
            _userRepository = userRepository;
            _passwordHashingService = passwordHashingService;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Verify password
            if (!_passwordHashingService.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            // Update last login with race condition protection
            try
            {
                // Re-fetch user to ensure we have the latest state before updating
                var currentUser = await _userRepository.GetByIdAsync(user.Id);

                // Verify user is still active before updating LastLogin
                if (currentUser == null || !currentUser.IsActive)
                {
                    return null;
                }

                currentUser.LastLogin = DateTime.Now;

                // Transparently upgrade an old/weak password hash to the current policy
                // (e.g. legacy 10,000-iteration hashes) on a successful login.
                if (_passwordHashingService.NeedsRehash(currentUser.PasswordHash))
                {
                    currentUser.PasswordHash = _passwordHashingService.HashPassword(password);
                }

                await _userRepository.UpdateAsync(currentUser);

                return currentUser;
            }
            catch (Exception ex)
            {
                // Log the error but still return the authenticated user
                // The LastLogin update is not critical for authentication success
                System.Diagnostics.Debug.WriteLine($"Warning: Failed to update LastLogin for user '{username}': {ex.Message}");
                return user;
            }
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<IEnumerable<User>> GetActiveUsersAsync()
        {
            return await _userRepository.GetActiveUsersAsync();
        }

        public async Task<User> CreateUserAsync(string username, string password, string fullName, string role, string email, string createdBy)
        {
            // Normalize username to lowercase for consistency
            username = username?.Trim().ToLower() ?? throw new InvalidOperationException("Username cannot be empty.");

            // Validate password strength
            ValidatePasswordStrength(password);

            // Validate email format
            ValidateEmail(email);

            // Validate role
            if (!IsValidRole(role))
            {
                throw new InvalidOperationException($"Invalid role '{role}'. Valid roles are: {string.Join(", ", GetValidRoles())}");
            }

            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(username))
            {
                throw new InvalidOperationException($"Username '{username}' already exists.");
            }

            var user = new User
            {
                Username = username,
                PasswordHash = _passwordHashingService.HashPassword(password),
                FullName = fullName,
                Role = role,
                Email = email,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatedBy = createdBy,
                MustChangePassword = false
            };

            return await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(User user, string modifiedBy)
        {
            // Validate email format
            ValidateEmail(user.Email);

            // Validate role
            if (!IsValidRole(user.Role))
            {
                throw new InvalidOperationException($"Invalid role '{user.Role}'. Valid roles are: {string.Join(", ", GetValidRoles())}");
            }

            // An edit must never leave the system with zero active administrators. The Edit User
            // form sets IsActive and Role directly, which would otherwise bypass the last-admin
            // guard in DisableUserAsync and the self-disable guard in the UI (e.g. disabling or
            // demoting the last admin would lock everyone out).
            var allUsers = await _userRepository.GetAllAsync();
            var otherActiveAdmins = allUsers.Count(u => u.Id != user.Id && u.Role == "Admin" && u.IsActive);
            var thisIsActiveAdmin = user.Role == "Admin" && user.IsActive;
            if (otherActiveAdmins == 0 && !thisIsActiveAdmin)
            {
                throw new InvalidOperationException(
                    "This change would leave no active administrator. At least one active admin account is required.");
            }

            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;
            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetPasswordAsync(int userId, string newPassword, string modifiedBy)
        {
            // Validate password strength
            ValidatePasswordStrength(newPassword);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Audit trail for password reset
            System.Diagnostics.Debug.WriteLine($"[AUDIT] Password reset for user '{user.Username}' (ID: {userId}) by '{modifiedBy}' at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            user.PasswordHash = _passwordHashingService.HashPassword(newPassword);
            user.MustChangePassword = true;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);

            // Confirm audit trail
            System.Diagnostics.Debug.WriteLine($"[AUDIT] Password reset completed successfully for user '{user.Username}'");
        }

        public async Task ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            // Validate password strength
            ValidatePasswordStrength(newPassword);

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Verify current password
            if (!_passwordHashingService.VerifyPassword(currentPassword, user.PasswordHash))
            {
                throw new InvalidOperationException("Current password is incorrect.");
            }

            // Audit trail for password change
            System.Diagnostics.Debug.WriteLine($"[AUDIT] Password changed by user '{user.Username}' (ID: {userId}) at {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

            user.PasswordHash = _passwordHashingService.HashPassword(newPassword);
            user.MustChangePassword = false;
            user.ModifiedDate = DateTime.Now;

            await _userRepository.UpdateAsync(user);

            // Confirm audit trail
            System.Diagnostics.Debug.WriteLine($"[AUDIT] Password change completed successfully for user '{user.Username}'");
        }

        public async Task DisableUserAsync(int userId, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Protect last admin from being disabled
            if (user.Role == "Admin" && user.IsActive)
            {
                var allUsers = await _userRepository.GetAllAsync();
                var activeAdminCount = allUsers.Count(u => u.Role == "Admin" && u.IsActive && u.Id != userId);

                if (activeAdminCount == 0)
                {
                    throw new InvalidOperationException("Cannot disable the last active administrator. At least one active admin account is required.");
                }
            }

            user.IsActive = false;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
        }

        public async Task EnableUserAsync(int userId, string modifiedBy)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            user.IsActive = true;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = modifiedBy;

            await _userRepository.UpdateAsync(user);
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeUserId = null)
        {
            return await _userRepository.UsernameExistsAsync(username, excludeUserId);
        }

        public async Task<int> GetUserCountAsync()
        {
            return await _userRepository.GetUserCountAsync();
        }

        public async Task EnsureDefaultAdminExistsAsync()
        {
            // Check if any users exist
            var userCount = await GetUserCountAsync();

            if (userCount == 0)
            {
                // Create default admin account with strong password
                // IMPORTANT: Change this password immediately after first login
                var defaultPassword = GenerateStrongPassword();
                await CreateUserAsync(
                    username: "admin",
                    password: defaultPassword,
                    fullName: "System Administrator",
                    role: "Admin",
                    email: "admin@iemsschool.edu.in",
                    createdBy: "System"
                );

                // Log the default password for first-time setup
                // In production, this should be logged to a secure file or shown once in setup wizard
                System.Diagnostics.Debug.WriteLine($"[IMPORTANT] Default admin password: {defaultPassword}");
                System.Diagnostics.Debug.WriteLine($"[IMPORTANT] Please change this password immediately after first login!");
            }
        }

        private string GenerateStrongPassword()
        {
            // Generate a cryptographically secure random 16-character password with mixed characters
            const string validChars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
            var chars = new char[16];

            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                var randomBytes = new byte[16];
                rng.GetBytes(randomBytes);

                for (int i = 0; i < 16; i++)
                {
                    chars[i] = validChars[randomBytes[i] % validChars.Length];
                }
            }

            return new string(chars);
        }

        private bool IsValidRole(string role)
        {
            return Enum.TryParse<UserRole>(role, true, out _);
        }

        private IEnumerable<string> GetValidRoles()
        {
            return Enum.GetNames(typeof(UserRole));
        }

        private void ValidatePasswordStrength(string password)
        {
            // Delegates to the shared PasswordPolicy (IEMS.Core.Services) so the rule lives in one place.
            var (isValid, error) = PasswordPolicy.Validate(password);
            if (!isValid)
            {
                throw new InvalidOperationException(error);
            }
        }

        private void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException("Email address cannot be empty.");
            }

            try
            {
                var mailAddress = new MailAddress(email);
                // Additional check to ensure the parsed address matches the input
                if (mailAddress.Address != email)
                {
                    throw new InvalidOperationException("Invalid email address format.");
                }
            }
            catch (FormatException)
            {
                throw new InvalidOperationException("Invalid email address format.");
            }
        }
    }
}