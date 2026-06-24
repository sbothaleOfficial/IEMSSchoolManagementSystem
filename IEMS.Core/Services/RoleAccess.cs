using System;
using System.Collections.Generic;
using System.Linq;

namespace IEMS.Core.Services
{
    /// <summary>The top-level feature areas (dashboard modules) that access can be granted to.</summary>
    public enum AppModule
    {
        Students,
        Staff,
        Transport,
        Finance,
        SchoolDocuments,
        AcademicYear,
        Backup,
        SystemSettings,
        UserManagement,
        AuditTrail
    }

    /// <summary>
    /// Central role → module permission map for the whole app. One source of truth so the dashboard
    /// (which cards are shown) and the module guards (which windows may be opened) can never drift
    /// apart. Admin always has full access; every other role is allow-listed. An unknown or empty
    /// role gets nothing (fail closed).
    /// </summary>
    public static class RoleAccess
    {
        private static readonly Dictionary<string, HashSet<AppModule>> Map =
            new(StringComparer.OrdinalIgnoreCase)
            {
                // Principal: everything except the system-administration tools.
                ["Principal"] = new HashSet<AppModule>
                {
                    AppModule.Students, AppModule.Staff, AppModule.Transport, AppModule.Finance,
                    AppModule.SchoolDocuments, AppModule.AcademicYear, AppModule.AuditTrail
                },
                // Accountant: money + the bits needed to record it.
                ["Accountant"] = new HashSet<AppModule>
                {
                    AppModule.Finance, AppModule.Transport, AppModule.SchoolDocuments
                },
                // Clerk: day-to-day front-office work (admissions, fee collection via Students,
                // certificates, ID cards, documents, student transport). No staff salaries, no admin.
                ["Clerk"] = new HashSet<AppModule>
                {
                    AppModule.Students, AppModule.Transport, AppModule.SchoolDocuments
                },
                // Teacher: students and shared documents only.
                ["Teacher"] = new HashSet<AppModule>
                {
                    AppModule.Students, AppModule.SchoolDocuments
                }
            };

        /// <summary>True if the given role may access the given module.</summary>
        public static bool CanAccess(string? role, AppModule module)
        {
            if (string.IsNullOrWhiteSpace(role))
                return false;
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
                return true;
            return Map.TryGetValue(role.Trim(), out var allowed) && allowed.Contains(module);
        }

        /// <summary>The modules a role may access (Admin = all).</summary>
        public static IReadOnlyCollection<AppModule> ModulesFor(string? role)
        {
            return Enum.GetValues(typeof(AppModule)).Cast<AppModule>()
                       .Where(m => CanAccess(role, m))
                       .ToList();
        }
    }
}
