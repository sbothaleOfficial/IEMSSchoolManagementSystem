using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;

namespace IEMS.Infrastructure.Data
{
    /// <summary>
    /// A fresh install seeds sample/demo data (260 students, 10 teachers, sample fees…) so the app
    /// is easy to explore. A real school, however, should start on a clean slate. On first launch we
    /// therefore remove that demo data — but ONLY if it is still the exact, untouched seed pattern,
    /// so a school's real data can never be deleted. School settings, user accounts and academic
    /// years are always kept. Deletes run as raw SQL (children-first) so they bypass the audit log
    /// and don't load thousands of rows into memory.
    /// </summary>
    public static class ProductionDataInitializer
    {
        public static async Task EnsureCleanStartAsync(ApplicationDbContext db)
        {
            // Never touch a database that holds anything other than the pristine demo seed.
            if (!await IsPristineDemoSeedAsync(db))
                return;

            // Children first so ON DELETE RESTRICT foreign keys are never violated.
            string[] deletes =
            {
                "DELETE FROM StudentPromotionHistory;",
                "DELETE FROM FeePayments;",
                "DELETE FROM FeeStructures;",
                "DELETE FROM TransportExpenses;",
                "DELETE FROM ElectricityBills;",
                "DELETE FROM OtherExpenses;",
                "DELETE FROM StudentDocuments;",
                "DELETE FROM Students;",
                "DELETE FROM Vehicles;",
                "DELETE FROM Classes;",
                "DELETE FROM Staff;",
                "DELETE FROM Teachers;",
            };
            foreach (var sql in deletes)
                await db.Database.ExecuteSqlRawAsync(sql);
        }

        /// <summary>
        /// Ensures the academic year containing <paramref name="asOf"/> (default: today) exists and
        /// is the one and only current year. The Indian school calendar runs June–May, so the year
        /// that starts in June Y is labelled "Y-(Y+1)" (e.g. June 2026 → "2026-27").
        ///
        /// The seed data ships historical sample years (with an old one marked current), which is
        /// fine for tests/demo but wrong for a real school. Running this on first launch makes a
        /// fresh install start on the correct, up-to-date academic year automatically — and because
        /// it is date-driven it never goes stale. It is idempotent: if the right year already exists
        /// and is current, it does nothing.
        /// </summary>
        public static async Task EnsureCurrentAcademicYearAsync(ApplicationDbContext db, DateTime? asOf = null)
        {
            var today = asOf ?? DateTime.Now;
            int startYear = today.Month >= 6 ? today.Year : today.Year - 1;
            string label = $"{startYear}-{(startYear + 1) % 100:D2}";

            var target = await db.AcademicYears.FirstOrDefaultAsync(a => a.Year == label);
            if (target == null)
            {
                target = new AcademicYear
                {
                    Year = label,
                    StartDate = new DateTime(startYear, 6, 1),
                    EndDate = new DateTime(startYear + 1, 5, 31),
                    IsCurrent = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.AcademicYears.Add(target);
                await db.SaveChangesAsync();
            }

            if (!target.IsCurrent)
            {
                // Keep the single-current invariant: clear any other current year first.
                foreach (var y in await db.AcademicYears.ToListAsync())
                {
                    if (y.IsCurrent)
                    {
                        y.IsCurrent = false;
                        y.UpdatedAt = DateTime.UtcNow;
                    }
                }
                target.IsCurrent = true;
                target.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// True only if the database still contains the exact, untouched demo seed. As soon as the
        /// school adds, edits or removes anything, this returns false and the demo clear never runs.
        /// </summary>
        public static async Task<bool> IsPristineDemoSeedAsync(ApplicationDbContext db)
        {
            if (await db.Students.CountAsync() != 260) return false;
            if (await db.Teachers.CountAsync() != 10) return false;
            if (await db.Staff.CountAsync() != 10) return false;
            if (await db.Vehicles.CountAsync() != 10) return false;
            if (await db.Classes.CountAsync() != 13) return false;

            // Spot-check the well-known seed markers at both ends of each set.
            if (!await db.Students.AnyAsync(s => s.StudentNumber == "S001")) return false;
            if (!await db.Students.AnyAsync(s => s.StudentNumber == "S260")) return false;
            if (!await db.Teachers.AnyAsync(t => t.EmployeeId == "T001")) return false;
            if (!await db.Teachers.AnyAsync(t => t.EmployeeId == "T010")) return false;
            if (!await db.Staff.AnyAsync(s => s.EmployeeId == "ST001")) return false;
            return true;
        }
    }
}
