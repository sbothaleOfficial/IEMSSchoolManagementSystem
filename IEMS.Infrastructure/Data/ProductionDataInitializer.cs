using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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
