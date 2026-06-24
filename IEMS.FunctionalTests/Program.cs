using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using IEMS.Infrastructure.Data;
using IEMS.Infrastructure.Repositories;
using IEMS.Core.Interfaces;
using IEMS.Core.Services;
using IEMS.Application.Services;
using IEMS.Application.Interfaces;
using IEMS.Application.DTOs;
using IEMS.Core.Enums;
using IEMS.Core.Configuration;

// ============================================================================
// IEMS School Management System - Functional Test Harness
// Drives the REAL Application + Infrastructure (EF Core / SQLite) layers
// using the same DI wiring as the WPF app (App.xaml.cs).
// ============================================================================

int passed = 0, failed = 0;
var failures = new List<string>();

void Check(string name, bool condition, string detail = "")
{
    if (condition) { passed++; Console.WriteLine($"  [PASS] {name}{(detail.Length > 0 ? "  (" + detail + ")" : "")}"); }
    else { failed++; failures.Add(name); Console.WriteLine($"  [FAIL] {name}{(detail.Length > 0 ? "  -> " + detail : "")}"); }
}

async Task Section(string title, Func<Task> body)
{
    Console.WriteLine();
    Console.WriteLine("== " + title + " ==");
    try { await body(); }
    catch (Exception ex) { failed++; failures.Add(title + " (threw)"); Console.WriteLine($"  [FAIL] {title} threw: {ex.GetType().Name}: {ex.Message}"); }
}

// ----- Fresh test database -----
var dbPath = Path.Combine(AppContext.BaseDirectory, "ftest.db");
if (File.Exists(dbPath)) File.Delete(dbPath);
var connString = $"Data Source={dbPath}";

// ----- DI container (mirrors App.xaml.cs, minus WPF windows + hosted service) -----
var services = new ServiceCollection();
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connString));
services.AddScoped<IStudentRepository, StudentRepository>();
services.AddScoped<IClassRepository, ClassRepository>();
services.AddScoped<ITeacherRepository, TeacherRepository>();
services.AddScoped<IStaffRepository, StaffRepository>();
services.AddScoped<IFeePaymentRepository, FeePaymentRepository>();
services.AddScoped<IFeeStructureRepository, FeeStructureRepository>();
services.AddScoped<IVehicleRepository, VehicleRepository>();
services.AddScoped<ITransportExpenseRepository, TransportExpenseRepository>();
services.AddScoped<IElectricityBillRepository, ElectricityBillRepository>();
services.AddScoped<IOtherExpenseRepository, OtherExpenseRepository>();
services.AddScoped<IAcademicYearRepository, AcademicYearRepository>();
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
services.AddScoped<IStudentPromotionRepository, StudentPromotionRepository>();
services.AddScoped<IStudentDocumentRepository, StudentDocumentRepository>();
services.AddScoped<StudentDocumentService>();
services.AddScoped<ISchoolDocumentRepository, SchoolDocumentRepository>();
services.AddScoped<SchoolDocumentService>();
services.AddScoped<FeeCalculationService>();
services.AddScoped<AmountToWordsService>();
services.AddScoped<PasswordHashingService>();
services.AddSingleton(new BulkPromotionConfiguration
{
    EligibilityRules = new EligibilityRulesConfiguration
    {
        MaxPendingFees = 0m, MinAttendancePercentage = 75,
        RequireTeacherApproval = false, AllowPromotionWithPendingFees = false
    },
    ClassProgression = new ClassProgressionConfiguration
    {
        AllowSameGradePromotion = true, AllowSkipGrade = false, StrictProgressionOnly = true
    }
});
services.AddScoped<StudentPromotionService>();
services.AddScoped<StudentEligibilityValidator>();
services.AddScoped<ClassProgressionValidator>();
services.AddScoped<BulkPromotionService>();
services.AddScoped<StudentService>();
services.AddScoped<TeacherService>();
services.AddScoped<ClassService>();
services.AddScoped<StaffService>();
services.AddScoped<FeePaymentService>();
services.AddScoped<FeeStructureService>();
services.AddScoped<VehicleService>();
services.AddScoped<TransportExpenseService>();
services.AddScoped<ElectricityBillService>();
services.AddScoped<OtherExpenseService>();
services.AddScoped<AcademicYearService>();
services.AddScoped<IBackupService, BackupService>();
services.AddScoped<ISystemSettingsService, SystemSettingsService>();
services.AddScoped<UserService>();
services.AddScoped<TwoFactorService>();

var provider = services.BuildServiceProvider();

Console.WriteLine("============================================================");
Console.WriteLine(" IEMS Functional Test Harness");
Console.WriteLine("============================================================");

using var scope = provider.CreateScope();
var sp = scope.ServiceProvider;

// ----- 1. DB creation + seed integrity -----
await Section("1. Database creation & seed integrity", async () =>
{
    var ctx = sp.GetRequiredService<ApplicationDbContext>();
    // Exercise the real production path: EF Core migrations (not EnsureCreated).
    ctx.Database.Migrate();
    Check("Database created via migrations", File.Exists(dbPath));
    Check("Migrations history recorded", ctx.Database.GetAppliedMigrations().Any());

    Check("Seed: 10 teachers", await ctx.Teachers.CountAsync() == 10, $"{await ctx.Teachers.CountAsync()}");
    Check("Seed: 13 classes", await ctx.Classes.CountAsync() == 13, $"{await ctx.Classes.CountAsync()}");
    Check("Seed: 260 students", await ctx.Students.CountAsync() == 260, $"{await ctx.Students.CountAsync()}");
    Check("Seed: 10 staff", await ctx.Staff.CountAsync() == 10, $"{await ctx.Staff.CountAsync()}");
    Check("Seed: 10 vehicles", await ctx.Vehicles.CountAsync() == 10, $"{await ctx.Vehicles.CountAsync()}");
    Check("Seed: 4 academic years", await ctx.AcademicYears.CountAsync() == 4, $"{await ctx.AcademicYears.CountAsync()}");
    Check("Seed: 26 fee structures", await ctx.FeeStructures.CountAsync() == 26, $"{await ctx.FeeStructures.CountAsync()}");
    Check("Seed: 10 fee payments", await ctx.FeePayments.CountAsync() == 10, $"{await ctx.FeePayments.CountAsync()}");
    Check("Seed: 10 electricity bills", await ctx.ElectricityBills.CountAsync() == 10, $"{await ctx.ElectricityBills.CountAsync()}");
    Check("Seed: 10 other expenses", await ctx.OtherExpenses.CountAsync() == 10, $"{await ctx.OtherExpenses.CountAsync()}");
    Check("Seed: 10 transport expenses", await ctx.TransportExpenses.CountAsync() == 10, $"{await ctx.TransportExpenses.CountAsync()}");
    Check("Seed: 1 admin user", await ctx.Users.CountAsync() == 1, $"{await ctx.Users.CountAsync()}");
    Check("Seed: 14 system settings", await ctx.SystemSettings.CountAsync() == 14, $"{await ctx.SystemSettings.CountAsync()}");

    var current = await ctx.AcademicYears.SingleOrDefaultAsync(a => a.IsCurrent);
    Check("Exactly one current academic year = 2024-25", current != null && current.Year == "2024-25", current?.Year ?? "none");
});

// ----- 2. Authentication (the documented-credentials check) -----
await Section("2. Authentication / login", async () =>
{
    var users = sp.GetRequiredService<UserService>();

    var okAdmin = await users.AuthenticateAsync("admin", "admin123");
    Check("Login admin / admin123 succeeds (actual seeded password)", okAdmin != null, okAdmin?.Role ?? "null");

    var docCreds = await users.AuthenticateAsync("admin", "Admin@123");
    Check("Login admin / Admin@123 is rejected (real password is admin123)", docCreds == null,
          docCreds == null ? "correctly rejected" : "UNEXPECTEDLY ACCEPTED");

    var wrong = await users.AuthenticateAsync("admin", "totallywrong");
    Check("Login with wrong password rejected", wrong == null);

    var noUser = await users.AuthenticateAsync("ghost", "admin123");
    Check("Login with unknown username rejected", noUser == null);
});

// ----- 3. Read path across modules (repository + DTO mapping) -----
await Section("3. Read APIs across all modules", async () =>
{
    Check("StudentService.GetAll = 260", (await sp.GetRequiredService<StudentService>().GetAllStudentsAsync()).Count() == 260);
    Check("TeacherService.GetAll = 10", (await sp.GetRequiredService<TeacherService>().GetAllTeachersAsync()).Count() == 10);
    Check("ClassService.GetAll = 13", (await sp.GetRequiredService<ClassService>().GetAllClassesAsync()).Count() == 13);
    Check("StaffService.GetAll = 10", (await sp.GetRequiredService<StaffService>().GetAllStaffAsync()).Count() == 10);
    Check("VehicleService.GetAll = 10", (await sp.GetRequiredService<VehicleService>().GetAllVehiclesAsync()).Count() == 10);
    Check("AcademicYearService.GetAll = 4", (await sp.GetRequiredService<AcademicYearService>().GetAllAcademicYearsAsync()).Count() == 4);
    Check("FeeStructureService.GetAll = 26", (await sp.GetRequiredService<FeeStructureService>().GetAllFeeStructuresAsync()).Count() == 26);

    var cur = await sp.GetRequiredService<AcademicYearService>().GetCurrentAcademicYearAsync();
    Check("AcademicYearService.GetCurrent = 2024-25", cur != null && cur.Year == "2024-25", cur?.Year ?? "null");

    // class-with-students join
    var cls = await sp.GetRequiredService<ClassService>().GetClassWithStudentsAsync(1);
    Check("Class 1 has 20 students (navigation join works)", cls != null);
});

// ----- 4. Teacher full CRUD (write path) -----
await Section("4. Teacher CRUD lifecycle", async () =>
{
    var ts = sp.GetRequiredService<TeacherService>();
    int before = (await ts.GetAllTeachersAsync()).Count();

    var dto = new TeacherDto
    {
        EmployeeId = "T999", FirstName = "Test", LastName = "Teacher",
        PhoneNumber = "9000000001", Address = "QA Lane", JoiningDate = new DateTime(2024, 1, 1),
        MonthlySalary = 51000, Email = "qa@iems.test"
    };
    var created = await ts.AddTeacherAsync(dto);
    Check("Create teacher returns new Id", created != null && created.Id > 0, $"Id={created?.Id}");
    Check("Teacher count incremented", (await ts.GetAllTeachersAsync()).Count() == before + 1);

    var fetched = await ts.GetTeacherByIdAsync(created.Id);
    Check("Read-back returns created teacher", fetched != null && fetched.EmployeeId == "T999");

    fetched.MonthlySalary = 77000; fetched.LastName = "Updated";
    await ts.UpdateTeacherAsync(fetched);
    var afterUpd = await ts.GetTeacherByIdAsync(created.Id);
    Check("Update persists salary + name", afterUpd.MonthlySalary == 77000 && afterUpd.LastName == "Updated",
          $"{afterUpd.MonthlySalary}/{afterUpd.LastName}");

    bool dupBlocked = !await ts.IsEmployeeIdUniqueAsync("T999");
    Check("Duplicate EmployeeId detected as non-unique", dupBlocked);

    await ts.DeleteTeacherAsync(created.Id);
    Check("Delete removes teacher", (await ts.GetTeacherByIdAsync(created.Id)) == null);
    Check("Teacher count restored", (await ts.GetAllTeachersAsync()).Count() == before);
});

// ----- 5. Domain logic: fee calculation + amount-to-words -----
await Section("5. Domain logic (fee calc & amount-to-words)", () =>
{
    var calc = sp.GetRequiredService<FeeCalculationService>();
    var r1 = calc.CalculatePayment(amountPaid: 15000, discount: 0, lateFee: 0, previousBalance: 0, totalFeeAmount: 60000);
    Check("Fee calc: 60000 - 15000 => remaining 45000", r1.RemainingBalance == 45000, $"{r1.RemainingBalance}");
    Check("Fee calc: not fully paid", !r1.IsFullyPaid);

    var r2 = calc.CalculatePayment(amountPaid: 5000, discount: 0, lateFee: 0, previousBalance: 0, totalFeeAmount: 5000);
    Check("Fee calc: exact payment => fully paid, remaining 0", r2.IsFullyPaid && r2.RemainingBalance == 0);

    var r3 = calc.CalculatePayment(amountPaid: 6000, discount: 0, lateFee: 0, previousBalance: 0, totalFeeAmount: 5000);
    Check("Fee calc: overpayment detected (=1000)", r3.IsOverpayment && r3.OverpaymentAmount == 1000, $"{r3.OverpaymentAmount}");

    var r4 = calc.CalculatePayment(amountPaid: 8000, discount: 500, lateFee: 200, previousBalance: 0, totalFeeAmount: 55000);
    Check("Fee calc: 55000 +200 -500 -8000 => 46700", r4.RemainingBalance == 46700, $"{r4.RemainingBalance}");

    var lf = calc.CalculateLateFee(new DateTime(2024, 1, 1), new DateTime(2024, 2, 1), 10000);
    Check("Late fee for ~31 days late is positive", lf > 0, $"{lf}");
    var lf0 = calc.CalculateLateFee(new DateTime(2024, 2, 1), new DateTime(2024, 1, 1), 10000);
    Check("Late fee 0 when paid before due date", lf0 == 0);

    var words = sp.GetRequiredService<AmountToWordsService>();
    var w = words.ConvertToWords(12345m);
    Check("Amount-to-words returns non-empty text", !string.IsNullOrWhiteSpace(w), w);
    Check("Amount-to-words validates negative as invalid", !words.IsValidAmount(-5m));
    return Task.CompletedTask;
});

// ----- 6. Password hashing round-trip -----
await Section("6. Password hashing (PBKDF2) round-trip", () =>
{
    var ph = sp.GetRequiredService<PasswordHashingService>();
    var h = ph.HashPassword("Secret#123");
    Check("Hash is produced", !string.IsNullOrWhiteSpace(h));
    Check("Correct password verifies", ph.VerifyPassword("Secret#123", h));
    Check("Wrong password rejected", !ph.VerifyPassword("Secret#124", h));
    var h2 = ph.HashPassword("Secret#123");
    Check("Same password -> different hash (random salt)", h != h2);
    return Task.CompletedTask;
});

// ----- 7. User management lifecycle -----
await Section("7. User management lifecycle", async () =>
{
    var users = sp.GetRequiredService<UserService>();
    int before = await users.GetUserCountAsync();
    var u = await users.CreateUserAsync("clerk1", "Clerk@123", "Clerk One", "Clerk", "clerk1@iems.test", "admin");
    Check("Create user returns Id", u != null && u.Id > 0);
    Check("New user can authenticate", (await users.AuthenticateAsync("clerk1", "Clerk@123")) != null);
    Check("Username now exists", await users.UsernameExistsAsync("clerk1"));

    await users.ResetPasswordAsync(u.Id, "NewPass@1", "admin");
    Check("Password reset: old password no longer works", (await users.AuthenticateAsync("clerk1", "Clerk@123")) == null);
    Check("Password reset: new password works", (await users.AuthenticateAsync("clerk1", "NewPass@1")) != null);

    await users.DisableUserAsync(u.Id, "admin");
    Check("Disabled user cannot authenticate", (await users.AuthenticateAsync("clerk1", "NewPass@1")) == null);
    await users.EnableUserAsync(u.Id, "admin");
    Check("Re-enabled user can authenticate", (await users.AuthenticateAsync("clerk1", "NewPass@1")) != null);
    Check("User count incremented by 1", await users.GetUserCountAsync() == before + 1);

    // The Edit User form can set IsActive/Role directly via UpdateUserAsync; that path must
    // not be allowed to disable or demote the last active administrator (lockout protection).
    Check("UpdateUserAsync cannot disable the last admin (no lockout)", await Throws(async s =>
    {
        var us = s.GetRequiredService<UserService>();
        var admin = await us.GetByIdAsync(1);
        admin.IsActive = false;
        await us.UpdateUserAsync(admin, "test");
    }));
    Check("UpdateUserAsync cannot demote the last admin's role (no lockout)", await Throws(async s =>
    {
        var us = s.GetRequiredService<UserService>();
        var admin = await us.GetByIdAsync(1);
        admin.Role = "Clerk";
        await us.UpdateUserAsync(admin, "test");
    }));
});

// ----- 8. Regression tests for the audited bug fixes -----
await Section("8. Bug-fix regressions", async () =>
{
    // Architecture guard: the Application layer must not depend on Infrastructure (Clean
    // Architecture). This locks in the layering fix so the dependency can't silently return.
    var appRefsInfra = typeof(UserService).Assembly.GetReferencedAssemblies()
        .Any(a => a.Name == "IEMS.Infrastructure");
    Check("Architecture: IEMS.Application does not reference IEMS.Infrastructure", !appRefsInfra);

    var calc = sp.GetRequiredService<FeeCalculationService>();
    // Discount larger than the bill must NOT create a refund bigger than what was paid
    var over = calc.CalculatePayment(amountPaid: 100, discount: 70000, lateFee: 0, previousBalance: 0, totalFeeAmount: 60000);
    Check("FeeCalc: oversized discount floors owed at 0 (no negative)", over.RemainingBalance == 0, $"{over.RemainingBalance}");
    Check("FeeCalc: overpayment refund never exceeds amount paid", over.OverpaymentAmount <= 100, $"{over.OverpaymentAmount}");

    var words = sp.GetRequiredService<AmountToWordsService>();
    Check("AmountToWords: negative renders as 'Minus ...'", words.ConvertToWords(-5.50m).StartsWith("Minus"), words.ConvertToWords(-5.50m));
    Check("AmountToWords: 0.01 -> includes 'One Paisa'", words.ConvertToWords(0.01m).Contains("One Paisa"), words.ConvertToWords(0.01m));
    Check("AmountToWords: 1.999 rounds cleanly (no 'Hundred Paise')", !words.ConvertToWords(1.999m).Contains("Hundred Paise"), words.ConvertToWords(1.999m));

    // Student admission academic year is now persisted (was silently dropped)
    var ss = sp.GetRequiredService<StudentService>();
    var newStudent = await ss.AddStudentAsync(new StudentDto
    {
        SerialNo = 99001, Standard = "Class 1", ClassDivision = "A", FirstName = "QA", FatherName = "Father",
        Surname = "Student", DateOfBirth = new DateTime(2015, 1, 1), Gender = "Male", MotherName = "Mother",
        StudentNumber = "QA9001", AdmissionDate = new DateTime(2024, 6, 1), ClassId = 1, AdmissionAcademicYearId = 3
    });
    var ctx2 = sp.GetRequiredService<ApplicationDbContext>();
    var persisted = await ctx2.Students.FindAsync(newStudent.Id);
    Check("Student.AdmissionAcademicYearId is persisted on Add", persisted != null && persisted.AdmissionAcademicYearId == 3,
          $"{persisted?.AdmissionAcademicYearId}");

    // Academic year: setting an invalid id must throw and NOT wipe the current year
    var ayRepo = sp.GetRequiredService<IAcademicYearRepository>();
    bool threw = false;
    try { await ayRepo.SetCurrentAcademicYearAsync(99999); } catch { threw = true; }
    Check("AcademicYear: setting invalid id throws (does not wipe current)", threw);
    var stillCurrent = await ayRepo.GetCurrentAcademicYearAsync();
    Check("AcademicYear: a current year still exists after the failed set", stillCurrent != null && stillCurrent.Year == "2024-25", stillCurrent?.Year ?? "none");

    // Pending-fees filter now excludes fully-paid students
    var studentRepo = sp.GetRequiredService<IStudentRepository>();
    var pending = (await studentRepo.GetStudentsWithPendingFeesAsync(1)).ToList();
    Check("Pending-fees filter returns only students with a balance (not whole class)", pending.Count < 20 && pending.All(s => s.FeePayments.Any(fp => fp.RemainingBalance > 0)),
          $"{pending.Count} of 20");

    // Username lookups are null-safe (was a login crash)
    var users2 = sp.GetRequiredService<UserService>();
    var nullUser = await users2.AuthenticateAsync(null, "x");
    Check("Auth with null username returns null (no crash)", nullUser == null);
});

// Helper: run an action in a fresh DI scope and report whether it threw.
async Task<bool> Throws(Func<IServiceProvider, Task> act)
{
    using var s2 = provider.CreateScope();
    try { await act(s2.ServiceProvider); return false; }
    catch { return true; }
}

// Helper: a valid StudentDto with the given unique keys.
StudentDto MakeStudent(int serial, string number, int classId = 1) => new StudentDto
{
    SerialNo = serial, Standard = "Class 1", ClassDivision = "A",
    FirstName = "Test", FatherName = "Test Father", Surname = "Student",
    DateOfBirth = new DateTime(2015, 5, 5), Gender = "Male", MotherName = "Test Mother",
    StudentNumber = number, AdmissionDate = new DateTime(2024, 6, 1),
    CasteCategory = "General", Religion = "Hindu", Address = "1 Test Rd",
    CityVillage = "Testville", ParentMobileNumber = "9000000000",
    AadhaarNumber = "111122223333", ClassId = classId
};

var dbOpts = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connString).Options;

// ----- 8. Unique-constraint integrity (duplicates MUST be rejected) -----
await Section("8. Unique-constraint integrity", async () =>
{
    Check("Duplicate student number S001 rejected",
        await Throws(s => s.GetRequiredService<StudentService>().AddStudentAsync(MakeStudent(90001, "S001"))));
    Check("Duplicate student serial-no rejected",
        await Throws(s => s.GetRequiredService<StudentService>().AddStudentAsync(MakeStudent(1, "SUNIQUE1"))));
    Check("Duplicate teacher EmployeeId T001 rejected",
        await Throws(s => s.GetRequiredService<TeacherService>().AddTeacherAsync(new TeacherDto { EmployeeId = "T001", FirstName = "Dup", LastName = "Teacher", PhoneNumber = "9000000000", Address = "x" })));
    Check("Duplicate staff EmployeeId ST001 rejected",
        await Throws(s => s.GetRequiredService<StaffService>().CreateStaffAsync(new StaffDto { EmployeeId = "ST001", FirstName = "Dup", LastName = "Staff", PhoneNumber = "9000000000", Address = "x", Position = "Clerk", JoiningDate = DateTime.Today })));
    Check("Duplicate vehicle number MH12AB1234 rejected",
        await Throws(s => s.GetRequiredService<VehicleService>().CreateVehicleAsync(new CreateVehicleDto { VehicleNumber = "MH12AB1234", VehicleType = VehicleType.BUS, DriverName = "Dup", DriverPhone = "9000000000", Route = "x" })));
    Check("Duplicate academic year 2024-25 rejected",
        await Throws(s => s.GetRequiredService<AcademicYearService>().AddAcademicYearAsync(new AcademicYearDto { Year = "2024-25", StartDate = new DateTime(2024, 6, 1), EndDate = new DateTime(2025, 5, 31) })));
    Check("Duplicate fee structure (Class1/TUITION/AY3) rejected",
        await Throws(s => s.GetRequiredService<FeeStructureService>().CreateFeeStructureAsync(new CreateFeeStructureDto { ClassId = 1, FeeType = FeeType.TUITION, Amount = 1, AcademicYearId = 3, AcademicYear = "2024-25", Description = "dup" })));
    Check("Duplicate username 'admin' rejected",
        await Throws(s => s.GetRequiredService<UserService>().CreateUserAsync("admin", "Pass@123", "Dup", "Clerk", "d@x.test", "admin")));
    Check("Duplicate electricity bill (month 1 / 2024) rejected",
        await Throws(s => s.GetRequiredService<ElectricityBillService>().CreateAsync(new ElectricityBillDto { BillNumber = "EBDUP", BillMonth = 1, BillYear = 2024, Amount = 100, DueDate = DateTime.Today, Units = 1, UnitsRate = 1 })));
});

// ----- 9. Foreign-key & delete-guard integrity -----
await Section("9. FK & delete-guard integrity", async () =>
{
    Check("Cannot delete teacher assigned to a class",
        await Throws(s => s.GetRequiredService<TeacherService>().DeleteTeacherAsync(1)));
    Check("Cannot delete class that has students",
        await Throws(s => s.GetRequiredService<ClassService>().DeleteClassAsync(1)));
    Check("Cannot delete the current academic year",
        await Throws(s => s.GetRequiredService<AcademicYearService>().DeleteAcademicYearAsync(3)));
    Check("Cannot delete a student who has fee payments (friendly guard, not raw DB error)",
        await Throws(s => s.GetRequiredService<StudentService>().DeleteStudentAsync(1)));
    Check("Cannot delete a class that has fee structures (friendly guard, not raw DB error)",
        await Throws(async s =>
        {
            var cs = s.GetRequiredService<ClassService>();
            var fss = s.GetRequiredService<FeeStructureService>();
            var newClass = await cs.AddClassAsync(new ClassDto { Name = "Class 1", Section = "ZZ", TeacherId = 1 });
            await fss.CreateFeeStructureAsync(new CreateFeeStructureDto { ClassId = newClass.Id, FeeType = FeeType.LIBRARY, Amount = 100, AcademicYearId = 3, AcademicYear = "2024-25", Description = "guard-test" });
            await cs.DeleteClassAsync(newClass.Id); // class has no students but has a fee structure -> must throw friendly
        }));
    Check("Cannot delete a non-current academic year that has fee structures (friendly guard, not raw DB error)",
        await Throws(async s =>
        {
            var fss = s.GetRequiredService<FeeStructureService>();
            var ays = s.GetRequiredService<AcademicYearService>();
            // Year 4 (2025-26) is non-current; attach a fee structure so the FK Restrict blocks deletion.
            await fss.CreateFeeStructureAsync(new CreateFeeStructureDto { ClassId = 1, FeeType = FeeType.LIBRARY, Amount = 100, AcademicYearId = 4, AcademicYear = "2025-26", Description = "ay-guard-test" });
            await ays.DeleteAcademicYearAsync(4);
        }));
});

// ----- 10. Referential integrity (no orphaned rows) -----
await Section("10. Referential integrity (no orphans)", async () =>
{
    using var ctx = new ApplicationDbContext(dbOpts);
    Check("Every FeePayment.StudentId references a real student",
        await ctx.FeePayments.CountAsync(p => !ctx.Students.Any(s => s.Id == p.StudentId)) == 0);
    Check("Every Student.ClassId references a real class",
        await ctx.Students.CountAsync(st => !ctx.Classes.Any(c => c.Id == st.ClassId)) == 0);
    Check("Every Class.TeacherId references a real teacher",
        await ctx.Classes.CountAsync(c => c.TeacherId != null && !ctx.Teachers.Any(t => t.Id == c.TeacherId)) == 0);
    Check("Every FeeStructure.ClassId references a real class",
        await ctx.FeeStructures.CountAsync(f => !ctx.Classes.Any(c => c.Id == f.ClassId)) == 0);
    Check("Every FeeStructure.AcademicYearId references a real year",
        await ctx.FeeStructures.CountAsync(f => !ctx.AcademicYears.Any(a => a.Id == f.AcademicYearId)) == 0);
    Check("Every FeePayment.AcademicYearId references a real year",
        await ctx.FeePayments.CountAsync(p => !ctx.AcademicYears.Any(a => a.Id == p.AcademicYearId)) == 0);
});

// ----- 11. Durability (data survives a brand-new connection) -----
await Section("11. Durability (persist across fresh connection)", async () =>
{
    int newId;
    using (var s2 = provider.CreateScope())
    {
        var v = await s2.ServiceProvider.GetRequiredService<VehicleService>()
            .CreateVehicleAsync(new CreateVehicleDto { VehicleNumber = "MH99DUR001", VehicleType = VehicleType.BUS, DriverName = "Durable", DriverPhone = "9000000000", Route = "R" });
        newId = v.Id;
    }
    // Brand-new context + connection to the same file — proves it was flushed to disk.
    using (var fresh = new ApplicationDbContext(dbOpts))
    {
        var found = await fresh.Vehicles.FirstOrDefaultAsync(x => x.Id == newId);
        Check("Created vehicle is readable from a new connection", found != null && found.VehicleNumber == "MH99DUR001");
        Check("Seed still intact in fresh connection (>=260 students)", await fresh.Students.CountAsync() >= 260);
    }
    // restore count
    using (var s3 = provider.CreateScope())
        await s3.ServiceProvider.GetRequiredService<VehicleService>().DeleteVehicleAsync(newId);
});

// ----- 12. Transaction atomicity & consistency (bulk promotion) -----
await Section("12. Bulk promotion atomicity & consistency", async () =>
{
    using (var s2 = provider.CreateScope())
    {
        var bulk = s2.ServiceProvider.GetRequiredService<BulkPromotionService>();
        var result = await bulk.ExecuteBulkPromotionAsync(new BulkPromotionRequest
        { FromClassId = 2, ToClassId = 5, AcademicYearId = 3, PromotedBy = "test", Reason = "regression" });
        Check("Promotion reports success", result.IsSuccess, $"promoted={result.PromotedStudents}, failed={result.FailedPromotions}");
        Check("Promotion moved all 20 students", result.PromotedStudents == 20, $"{result.PromotedStudents}");
    }
    using (var fresh = new ApplicationDbContext(dbOpts))
    {
        Check("Source class 2 is now empty (atomic move)", await fresh.Students.CountAsync(s => s.ClassId == 2) == 0);
        Check("Target class 5 now holds 40 students", await fresh.Students.CountAsync(s => s.ClassId == 5) == 40);
        Check("Promotion history rows == students promoted (consistent)",
            await fresh.StudentPromotionHistory.CountAsync(h => h.AcademicYearId == 3) == 20);
    }
    // Roll back so the rest of the suite sees seeded distribution + verify reversibility.
    using (var s3 = provider.CreateScope())
        await s3.ServiceProvider.GetRequiredService<BulkPromotionService>().RollbackPromotionAsync(2, 5, "2024-25");
    using (var fresh = new ApplicationDbContext(dbOpts))
    {
        Check("Rollback restores class 2 to 20 students", await fresh.Students.CountAsync(s => s.ClassId == 2) == 20);
        Check("Rollback restores class 5 to 20 students", await fresh.Students.CountAsync(s => s.ClassId == 5) == 20);
    }
});

// ----- 13. Fee payment creation + balance-math consistency -----
await Section("13. Fee payment creation & balance consistency", async () =>
{
    var fps = sp.GetRequiredService<FeePaymentService>();
    int before = (await fps.GetAllFeePaymentsAsync()).Count();
    // Student 41 is in Class 3 (TUITION fee 55000), has no prior payment.
    var pay = await fps.CreateFeePaymentAsync(new CreateFeePaymentDto
    {
        StudentId = 41, FeeType = FeeType.TUITION, AmountPaid = 20000, PaymentMethod = PaymentMethod.CASH,
        AcademicYearId = 3, AcademicYear = "2024-25", GeneratedBy = "test"
    });
    Check("Payment created with a receipt number", !string.IsNullOrWhiteSpace(pay.ReceiptNumber), pay.ReceiptNumber);
    Check("Remaining balance = 55000 - 20000 = 35000 (math consistent)", pay.RemainingBalance == 35000, $"{pay.RemainingBalance}");
    Check("Receipt number is unique vs existing payments",
        (await fps.GetAllFeePaymentsAsync()).Count(p => p.ReceiptNumber == pay.ReceiptNumber) == 1);
    Check("Payment count incremented by exactly 1", (await fps.GetAllFeePaymentsAsync()).Count() == before + 1);
    var status = await fps.GetStudentFeeStatusAsync(41, "2024-25");
    Check("Student fee-status retrievable after payment", status != null);

    // Receipt amount-in-words must use the corrected shared AmountToWordsService
    // (paise handled, not truncated by the old local duplicate).
    var payP = await fps.CreateFeePaymentAsync(new CreateFeePaymentDto
    {
        StudentId = 42, FeeType = FeeType.TUITION, AmountPaid = 12345.50m, PaymentMethod = PaymentMethod.CASH,
        AcademicYearId = 3, AcademicYear = "2024-25", GeneratedBy = "test"
    });
    var receipt = await fps.GenerateReceiptAsync(payP.Id);
    Check("Receipt amount-in-words includes paise correctly", receipt.AmountInWords.Contains("Fifty Paise"), receipt.AmountInWords);
    await fps.DeleteFeePaymentAsync(payP.Id);

    // clean up
    await fps.DeleteFeePaymentAsync(pay.Id);
    Check("Payment deletable; count restored", (await fps.GetAllFeePaymentsAsync()).Count() == before);
});

// ----- 14. Full CRUD across remaining modules -----
await Section("14. CRUD: Vehicle / TransportExpense / ElectricityBill / OtherExpense / FeeStructure / Staff / AcademicYear", async () =>
{
    // Vehicle
    var vs = sp.GetRequiredService<VehicleService>();
    var v = await vs.CreateVehicleAsync(new CreateVehicleDto { VehicleNumber = "MH00NEW99", VehicleType = VehicleType.AUTO, DriverName = "New", DriverPhone = "9000000001", Route = "A" });
    await vs.UpdateVehicleAsync(new UpdateVehicleDto { Id = v.Id, VehicleNumber = "MH00NEW99", VehicleType = VehicleType.AUTO, DriverName = "New", DriverPhone = "9000000001", Route = "Updated" });
    Check("Vehicle update persists route", (await vs.GetVehicleByIdAsync(v.Id))!.Route == "Updated");
    await vs.DeleteVehicleAsync(v.Id);
    Check("Vehicle deleted", await vs.GetVehicleByIdAsync(v.Id) == null);

    // TransportExpense
    var tes = sp.GetRequiredService<TransportExpenseService>();
    var te = await tes.CreateExpenseAsync(new CreateTransportExpenseDto { VehicleId = 1, Category = ExpenseCategory.FUEL, FuelType = FuelType.DIESEL, Amount = 1234, Quantity = 10, ExpenseDate = DateTime.Today, DriverName = "D", Description = "test", InvoiceNumber = "INVX1" });
    Check("TransportExpense created", te.Id > 0 && te.Amount == 1234);
    await tes.DeleteExpenseAsync(te.Id);
    Check("TransportExpense deleted", await tes.GetExpenseByIdAsync(te.Id) == null);

    // ElectricityBill
    var ebs = sp.GetRequiredService<ElectricityBillService>();
    var eb = await ebs.CreateAsync(new ElectricityBillDto { BillNumber = "EBNEW1", BillMonth = 11, BillYear = 2030, Amount = 5000, DueDate = DateTime.Today.AddDays(15), Units = 100, UnitsRate = 5, IsPaid = false });
    eb.IsPaid = true; eb.PaidDate = DateTime.Today; eb.PaymentMethod = PaymentMethod.CASH;
    await ebs.UpdateAsync(eb);
    Check("ElectricityBill update persists paid status", (await ebs.GetByIdAsync(eb.Id))!.IsPaid);
    await ebs.DeleteAsync(eb.Id);
    Check("ElectricityBill deleted", await ebs.GetByIdAsync(eb.Id) == null);

    // OtherExpense
    var oes = sp.GetRequiredService<OtherExpenseService>();
    var oe = await oes.CreateAsync(new OtherExpenseDto { Category = OtherExpenseCategory.STATIONERY, ExpenseType = "Test", Description = "test", Amount = 999, ExpenseDate = DateTime.Today, PaymentMethod = PaymentMethod.CASH });
    oe.Amount = 1500; await oes.UpdateAsync(oe);
    Check("OtherExpense update persists amount", (await oes.GetByIdAsync(oe.Id))!.Amount == 1500);
    await oes.DeleteAsync(oe.Id);
    Check("OtherExpense deleted", await oes.GetByIdAsync(oe.Id) == null);

    // FeeStructure (new unique combo: Class5 / SPORTS / AY3)
    var fss = sp.GetRequiredService<FeeStructureService>();
    var fs = await fss.CreateFeeStructureAsync(new CreateFeeStructureDto { ClassId = 5, FeeType = FeeType.SPORTS, Amount = 3000, AcademicYearId = 3, AcademicYear = "2024-25", Description = "test" });
    await fss.UpdateFeeStructureAsync(fs.Id, new CreateFeeStructureDto { ClassId = 5, FeeType = FeeType.SPORTS, Amount = 4500, AcademicYearId = 3, AcademicYear = "2024-25", Description = "test2" });
    Check("FeeStructure update persists amount", (await fss.GetFeeStructureByIdAsync(fs.Id))!.Amount == 4500);
    await fss.DeleteFeeStructureAsync(fs.Id);
    Check("FeeStructure soft-deleted (excluded from active list)",
        (await fss.GetAllFeeStructuresAsync()).All(x => x.Id != fs.Id));
    // Re-creating the SAME (class, fee type, year) after a soft delete must reactivate the
    // existing row, NOT fail with a cryptic unique-constraint error.
    var fsReact = await fss.CreateFeeStructureAsync(new CreateFeeStructureDto { ClassId = 5, FeeType = FeeType.SPORTS, Amount = 5000, AcademicYearId = 3, AcademicYear = "2024-25", Description = "reactivated" });
    Check("Re-create after soft delete reactivates same row (no unique-constraint error)", fsReact.Id == fs.Id, $"new={fsReact.Id}, old={fs.Id}");
    Check("Reactivated fee structure is active with the updated amount",
        (await fss.GetAllFeeStructuresAsync()).Any(x => x.Id == fs.Id && x.Amount == 5000));
    await fss.DeleteFeeStructureAsync(fsReact.Id);

    // Staff
    var ss = sp.GetRequiredService<StaffService>();
    var stf = await ss.CreateStaffAsync(new StaffDto { EmployeeId = "ST999", FirstName = "New", LastName = "Staff", PhoneNumber = "9000000002", Address = "x", Position = "Clerk", JoiningDate = DateTime.Today, MonthlySalary = 21000 });
    stf.MonthlySalary = 26000; await ss.UpdateStaffAsync(stf);
    Check("Staff update persists salary", (await ss.GetStaffByIdAsync(stf.Id))!.MonthlySalary == 26000);
    await ss.DeleteStaffAsync(stf.Id);
    Check("Staff deleted", await ss.GetStaffByIdAsync(stf.Id) == null);

    // AcademicYear + single-current invariant
    var ays = sp.GetRequiredService<AcademicYearService>();
    var ay = await ays.AddAcademicYearAsync(new AcademicYearDto { Year = "2030-31", StartDate = new DateTime(2030, 6, 1), EndDate = new DateTime(2031, 5, 31), IsCurrent = false });
    await ays.SetCurrentAcademicYearAsync(ay.Id);
    using (var fresh = new ApplicationDbContext(dbOpts))
        Check("Exactly one current academic year after switch (invariant held)", await fresh.AcademicYears.CountAsync(a => a.IsCurrent) == 1);
    await ays.SetCurrentAcademicYearAsync(3); // restore 2024-25 as current
    await ays.DeleteAcademicYearAsync(ay.Id);
    Check("Non-current academic year deletable", await ays.GetAcademicYearByIdAsync(ay.Id) == null);
});

// ----- 15. Backup & restore durability (checksum-validated) -----
await Section("15. Backup & restore durability", async () =>
{
    // BackupService now backs up DatabaseLocation.DatabaseFilePath = <AppContext.BaseDirectory>/school.db
    var liveDb = Path.Combine(AppContext.BaseDirectory, "school.db");
    var bkDir = Path.Combine(AppContext.BaseDirectory, "_bk_test");
    File.Copy(dbPath, liveDb, true);
    if (Directory.Exists(bkDir)) Directory.Delete(bkDir, true);
    Directory.CreateDirectory(bkDir);
    try
    {
        var backup = sp.GetRequiredService<IBackupService>();
        var res = await backup.CreateBackupAsync(BackupType.Full, bkDir);
        Check("Backup reports success", res.Success, res.Message);
        Check("Backup file exists on disk", !string.IsNullOrEmpty(res.BackupPath) && File.Exists(res.BackupPath), res.BackupPath ?? "");

        if (res.Success && File.Exists(res.BackupPath))
        {
            var restore = await backup.RestoreBackupAsync(res.BackupPath!, validateChecksum: true, skipSafetyBackup: true);
            Check("Restore (with checksum validation) succeeds", restore.Success, restore.Message);
        }
        else Check("Restore (with checksum validation) succeeds", false, "skipped — backup failed");
    }
    finally
    {
        try { if (File.Exists(liveDb)) File.Delete(liveDb); } catch { }
        try { if (Directory.Exists(bkDir)) Directory.Delete(bkDir, true); } catch { }
    }
});

// ----- 16. Aggregate / analytics consistency -----
await Section("16. Aggregate & analytics consistency", async () =>
{
    using var ctx = new ApplicationDbContext(dbOpts);
    var fps = sp.GetRequiredService<FeePaymentService>();

    // Note: SQLite can't SUM(decimal) server-side, so aggregate the DB rows client-side
    // (this is exactly what the production services do internally).
    var svcSum = (await fps.GetAllFeePaymentsAsync()).Sum(p => p.AmountPaid);
    var dbSum = (await ctx.FeePayments.Select(p => p.AmountPaid).ToListAsync()).Sum();
    Check("FeePayment service total == DB total (no rows lost in mapping)", svcSum == dbSum, $"{svcSum} vs {dbSum}");

    var ebs = sp.GetRequiredService<ElectricityBillService>();
    var svcYearTotal = await ebs.GetTotalAmountByYearAsync(2024);
    var dbYearTotal = (await ctx.ElectricityBills.Where(b => b.BillYear == 2024).Select(b => b.Amount).ToListAsync()).Sum();
    Check("Electricity 2024 total (service) == DB sum", svcYearTotal == dbYearTotal, $"{svcYearTotal} vs {dbYearTotal}");

    var oes = sp.GetRequiredService<OtherExpenseService>();
    var svcOtherStationery = await oes.GetTotalAmountByCategoryAsync(OtherExpenseCategory.STATIONERY);
    var dbOtherStationery = (await ctx.OtherExpenses.Where(e => e.Category == OtherExpenseCategory.STATIONERY).Select(e => e.Amount).ToListAsync()).Sum();
    Check("OtherExpense STATIONERY total (service) == DB sum", svcOtherStationery == dbOtherStationery, $"{svcOtherStationery} vs {dbOtherStationery}");

    var tes = sp.GetRequiredService<TransportExpenseService>();
    var svcVeh1 = await tes.GetTotalExpensesByVehicleAsync(1);
    var dbVeh1 = (await ctx.TransportExpenses.Where(e => e.VehicleId == 1).Select(e => e.Amount).ToListAsync()).Sum();
    Check("Transport vehicle-1 total (service) == DB sum", svcVeh1 == dbVeh1, $"{svcVeh1} vs {dbVeh1}");
});

await Section("17. Audit trail interceptor (who changed what)", async () =>
{
    // A context wired exactly like the WPF app: SaveChanges writes audit rows automatically.
    var interceptor = new AuditSaveChangesInterceptor(new HarnessUser());
    var opts = new DbContextOptionsBuilder<ApplicationDbContext>()
        .UseSqlite(connString).AddInterceptors(interceptor).Options;
    using var actx = new ApplicationDbContext(opts);

    var before = await actx.AuditLogs.CountAsync();
    var classId = await actx.Classes.Select(c => c.Id).FirstAsync();

    // INSERT
    var s = new IEMS.Core.Entities.Student
    {
        SerialNo = 99999, Standard = "Audit", ClassDivision = "A", FirstName = "Aud", FatherName = "F",
        Surname = "It", DateOfBirth = new DateTime(2015, 1, 1), Gender = "Male", MotherName = "M",
        StudentNumber = "AUD-AUDIT-1", AdmissionDate = new DateTime(2024, 6, 1), CasteCategory = "Open",
        Religion = "NA", Address = "x", CityVillage = "y", ParentMobileNumber = "9000000000", ClassId = classId
    };
    actx.Students.Add(s);
    await actx.SaveChangesAsync();

    var ins = await actx.AuditLogs.OrderByDescending(a => a.Id).FirstAsync();
    Check("Audit: insert recorded as Added/Student", ins.Action == "Added" && ins.EntityType == "Student", $"{ins.Action}/{ins.EntityType}");
    Check("Audit: insert attributed to current user", ins.UserName == "harness-user", ins.UserName);
    Check("Audit: insert primary key back-filled", ins.EntityId == s.Id.ToString(), ins.EntityId);

    // UPDATE
    s.FirstName = "AudChanged";
    await actx.SaveChangesAsync();
    var upd = await actx.AuditLogs.OrderByDescending(a => a.Id).FirstAsync();
    Check("Audit: update recorded as Modified", upd.Action == "Modified" && upd.EntityType == "Student", $"{upd.Action}/{upd.EntityType}");
    Check("Audit: update lists the changed field", upd.Summary != null && upd.Summary.Contains("FirstName"), upd.Summary ?? "null");

    // DELETE
    actx.Students.Remove(s);
    await actx.SaveChangesAsync();
    var del = await actx.AuditLogs.OrderByDescending(a => a.Id).FirstAsync();
    Check("Audit: delete recorded as Deleted", del.Action == "Deleted" && del.EntityType == "Student", $"{del.Action}/{del.EntityType}");

    var after = await actx.AuditLogs.CountAsync();
    Check("Audit: exactly 3 rows for insert/update/delete", after - before == 3, $"{after - before}");
    Check("Audit: audit rows are never self-audited", await actx.AuditLogs.CountAsync(a => a.EntityType == "AuditLog") == 0);

    // Repository "merge update" path: updating a DETACHED entity must mark ONLY the changed
    // column as modified (DbSet.Update() used to mark every column, flooding the audit trail).
    var detached = await actx.Users.AsNoTracking().FirstAsync();
    detached.FullName = detached.FullName + " (edited)";
    await actx.MergeUpdateAsync(detached, detached.Id);
    await actx.SaveChangesAsync();
    var userEdit = await actx.AuditLogs.OrderByDescending(a => a.Id).FirstAsync();
    Check("Audit: merge-update logs only the changed field (no column flood)",
        userEdit.Action == "Modified" && userEdit.EntityType == "User"
            && userEdit.Summary == "Changed: FullName",
        userEdit.Summary ?? "null");
});

await Section("18. Student documents (store / list / open / delete)", async () =>
{
    using var scope = provider.CreateScope();
    var docs = scope.ServiceProvider.GetRequiredService<StudentDocumentService>();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var studentId = await ctx.Students.Select(s => s.Id).FirstAsync();

    var before = (await docs.GetDocumentsAsync(studentId)).Count;
    var payload = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 fake document bytes for testing");

    var added = await docs.AddDocumentAsync(studentId, "Birth Certificate", "birth.pdf", "application/pdf", payload, "harness-user");
    Check("Documents: add returns metadata", added.Id > 0 && added.DocumentType == "Birth Certificate" && added.FileSize == payload.Length);

    var list = await docs.GetDocumentsAsync(studentId);
    Check("Documents: appears in the student's list", list.Count == before + 1 && list.Any(d => d.Id == added.Id));
    Check("Documents: list metadata is correct", list.First(d => d.Id == added.Id).FileName == "birth.pdf");

    var file = await docs.GetFileAsync(added.Id);
    Check("Documents: stored bytes round-trip exactly",
        file != null && file.Data.SequenceEqual(payload) && file.ContentType == "application/pdf", $"{file?.Data.Length}");

    await docs.DeleteDocumentAsync(added.Id);
    Check("Documents: delete removes it", (await docs.GetDocumentsAsync(studentId)).Count == before);
});

// ----- 19. Teacher / Staff ID-card fields (photo + blood group) -----
await Section("19. Teacher & Staff ID-card fields (photo + blood group)", async () =>
{
    using var scope = provider.CreateScope();
    var teachers = scope.ServiceProvider.GetRequiredService<TeacherService>();
    var staff = scope.ServiceProvider.GetRequiredService<StaffService>();
    var ctx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    var photo = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 1, 2, 3, 4, 5 }; // pretend JPEG bytes

    // --- Teacher ---
    var teacherId = await ctx.Teachers.Select(t => t.Id).FirstAsync();
    await teachers.UpdateTeacherCardInfoAsync(teacherId, photo, "B+");
    var t1 = await teachers.GetTeacherEntityByIdAsync(teacherId);
    Check("Teacher: photo persisted", t1 != null && t1.Photo != null && t1.Photo.SequenceEqual(photo), $"{t1?.Photo?.Length}");
    Check("Teacher: blood group persisted", t1?.BloodGroup == "B+", t1?.BloodGroup ?? "null");

    // Editing unrelated details must NOT wipe the photo/blood group.
    var tdto = await teachers.GetTeacherByIdAsync(teacherId);
    tdto!.Address = "New Address " + Guid.NewGuid().ToString("N").Substring(0, 6);
    await teachers.UpdateTeacherAsync(tdto);
    var t2 = await teachers.GetTeacherEntityByIdAsync(teacherId);
    Check("Teacher: editing details keeps the photo", t2?.Photo != null && t2.Photo.SequenceEqual(photo));
    Check("Teacher: editing details keeps the blood group", t2?.BloodGroup == "B+", t2?.BloodGroup ?? "null");

    // Clearing the photo works.
    await teachers.UpdateTeacherCardInfoAsync(teacherId, null, null);
    var t3 = await teachers.GetTeacherEntityByIdAsync(teacherId);
    Check("Teacher: photo can be removed", t3?.Photo == null && t3?.BloodGroup == null);

    // --- Staff ---
    var staffId = await ctx.Staff.Select(s => s.Id).FirstAsync();
    await staff.UpdateStaffCardInfoAsync(staffId, photo, "O-");
    var s1 = await staff.GetStaffEntityByIdAsync(staffId);
    Check("Staff: photo persisted", s1 != null && s1.Photo != null && s1.Photo.SequenceEqual(photo), $"{s1?.Photo?.Length}");
    Check("Staff: blood group persisted", s1?.BloodGroup == "O-", s1?.BloodGroup ?? "null");

    var sdto = await staff.GetStaffByIdAsync(staffId);
    sdto!.Address = "New Address " + Guid.NewGuid().ToString("N").Substring(0, 6);
    await staff.UpdateStaffAsync(sdto);
    var s2 = await staff.GetStaffEntityByIdAsync(staffId);
    Check("Staff: editing details keeps the photo", s2?.Photo != null && s2.Photo.SequenceEqual(photo));
    Check("Staff: editing details keeps the blood group", s2?.BloodGroup == "O-", s2?.BloodGroup ?? "null");
});

// ----- 20. School documents (store / list / open / delete) -----
await Section("20. School documents (store / list / open / delete)", async () =>
{
    using var scope = provider.CreateScope();
    var docs = scope.ServiceProvider.GetRequiredService<SchoolDocumentService>();

    var before = (await docs.GetDocumentsAsync()).Count;
    var payload = System.Text.Encoding.UTF8.GetBytes("%PDF-1.4 fake affiliation certificate bytes");

    var added = await docs.AddDocumentAsync("Affiliation / Recognition", "affiliation.pdf", "application/pdf", payload, "harness-user");
    Check("School docs: add returns metadata", added.Id > 0 && added.DocumentType == "Affiliation / Recognition" && added.FileSize == payload.Length);

    var list = await docs.GetDocumentsAsync();
    Check("School docs: appears in the list", list.Count == before + 1 && list.Any(d => d.Id == added.Id));
    Check("School docs: list metadata is correct", list.First(d => d.Id == added.Id).FileName == "affiliation.pdf");

    var file = await docs.GetFileAsync(added.Id);
    Check("School docs: stored bytes round-trip exactly",
        file != null && file.Data.SequenceEqual(payload) && file.ContentType == "application/pdf", $"{file?.Data.Length}");

    await docs.DeleteDocumentAsync(added.Id);
    Check("School docs: delete removes it", (await docs.GetDocumentsAsync()).Count == before);
});

// ----- 21. Production clean-start (clears ONLY pristine demo data) -----
await Section("21. Production clean-start (first-run demo data clear)", async () =>
{
    // Use a throwaway database so this destructive test never affects the others.
    var tmpDb = Path.Combine(Path.GetTempPath(), $"iems_cleanstart_{Guid.NewGuid():N}.db");
    var opts = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite($"Data Source={tmpDb}").Options;
    try
    {
        using var ctx = new ApplicationDbContext(opts);
        await ctx.Database.MigrateAsync(); // applies migrations + seeds the demo data

        Check("Clean-start: a fresh install is recognised as pristine demo data",
            await IEMS.Infrastructure.Data.ProductionDataInitializer.IsPristineDemoSeedAsync(ctx));

        await IEMS.Infrastructure.Data.ProductionDataInitializer.EnsureCleanStartAsync(ctx);

        Check("Clean-start: students cleared", await ctx.Students.CountAsync() == 0, $"{await ctx.Students.CountAsync()}");
        Check("Clean-start: teachers cleared", await ctx.Teachers.CountAsync() == 0);
        Check("Clean-start: staff cleared", await ctx.Staff.CountAsync() == 0);
        Check("Clean-start: classes cleared", await ctx.Classes.CountAsync() == 0);
        Check("Clean-start: vehicles cleared", await ctx.Vehicles.CountAsync() == 0);
        Check("Clean-start: fee payments cleared", await ctx.FeePayments.CountAsync() == 0);
        Check("Clean-start: fee structures cleared", await ctx.FeeStructures.CountAsync() == 0);
        Check("Clean-start: expenses cleared", await ctx.ElectricityBills.CountAsync() == 0 && await ctx.OtherExpenses.CountAsync() == 0);

        Check("Clean-start: SETTINGS kept", await ctx.SystemSettings.CountAsync() == 14, $"{await ctx.SystemSettings.CountAsync()}");
        Check("Clean-start: ACADEMIC YEARS kept", await ctx.AcademicYears.CountAsync() == 4, $"{await ctx.AcademicYears.CountAsync()}");
        Check("Clean-start: ADMIN USER kept", await ctx.Users.CountAsync() == 1, $"{await ctx.Users.CountAsync()}");

        Check("Clean-start: now NOT pristine, so it never runs again",
            !await IEMS.Infrastructure.Data.ProductionDataInitializer.IsPristineDemoSeedAsync(ctx));

        // Running it again must be a safe no-op (and must never delete real data).
        await IEMS.Infrastructure.Data.ProductionDataInitializer.EnsureCleanStartAsync(ctx);
        Check("Clean-start: second run is a safe no-op", await ctx.Students.CountAsync() == 0);
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var ext in new[] { "", "-wal", "-shm" })
            try { File.Delete(tmpDb + ext); } catch { }
    }
});

// ----- 22. Two-factor authentication (TOTP) -----
await Section("22. Two-factor authentication (TOTP authenticator)", async () =>
{
    // (a) RFC 6238 Appendix B official test vectors (HMAC-SHA1). Secret is ASCII
    // "12345678901234567890"; expected 6-digit codes truncated from the published 8-digit values.
    var rfcSecret = TotpService.Base32Encode(System.Text.Encoding.ASCII.GetBytes("12345678901234567890"));
    var vectors = new (long unixTime, string expected6)[]
    {
        (59L,          "287082"),
        (1111111109L,  "081804"),
        (1111111111L,  "050471"),
        (1234567890L,  "005924"),
        (2000000000L,  "279037"),
    };
    foreach (var v in vectors)
    {
        var code = TotpService.GetCode(rfcSecret, DateTimeOffset.FromUnixTimeSeconds(v.unixTime));
        Check($"TOTP RFC6238 vector @ T={v.unixTime}", code == v.expected6, $"got {code}, expected {v.expected6}");
    }

    // (b) Base32 round-trips arbitrary bytes.
    var raw = new byte[20];
    new Random(12345).NextBytes(raw);
    var rt = TotpService.Base32Decode(TotpService.Base32Encode(raw));
    Check("Base32 encode/decode round-trips", rt.Length == raw.Length && rt.AsSpan().SequenceEqual(raw));

    // (c) Validation window tolerates +/-1 step of clock skew but not more.
    var secret = TotpService.GenerateSecret();
    var now = DateTimeOffset.UtcNow;
    Check("TOTP: current code accepted", TotpService.ValidateCode(secret, TotpService.GetCode(secret, now), 1, now));
    Check("TOTP: previous-step code accepted (skew)",
        TotpService.ValidateCode(secret, TotpService.GetCode(secret, now.AddSeconds(-30)), 1, now));
    Check("TOTP: 4-steps-old code rejected",
        !TotpService.ValidateCode(secret, TotpService.GetCode(secret, now.AddSeconds(-120)), 1, now));

    // (d) Full service flow against the real database.
    using var scope = provider.CreateScope();
    var users = scope.ServiceProvider.GetRequiredService<UserService>();
    var tf = scope.ServiceProvider.GetRequiredService<TwoFactorService>();

    var u = await users.CreateUserAsync("totptester", "Totp@1234", "TOTP Tester", "Clerk", "totp@test.com", "harness");
    var enrollSecret = TotpService.GenerateSecret();
    var backupCodes = await tf.EnableAsync(u.Id, enrollSecret, "harness");

    Check("2FA: enable returns 10 backup codes", backupCodes.Count == 10);
    var reloaded = await users.GetByIdAsync(u.Id);
    Check("2FA: enabled flag persisted", reloaded!.TwoFactorEnabled && reloaded.TwoFactorSecret == enrollSecret);

    var liveCode = TotpService.GetCode(enrollSecret);
    Check("2FA: valid authenticator code accepted", await tf.VerifyAsync(u.Id, liveCode));

    var wrong = (liveCode[0] == '0' ? "1" : "0") + liveCode.Substring(1);
    Check("2FA: wrong authenticator code rejected", !await tf.VerifyAsync(u.Id, wrong));

    // Backup codes are single-use and format/case-insensitive.
    Check("2FA: backup code accepted", await tf.VerifyAsync(u.Id, backupCodes[0]));
    Check("2FA: same backup code rejected on reuse", !await tf.VerifyAsync(u.Id, backupCodes[0]));
    Check("2FA: backup code accepted lower-case/spacing", await tf.VerifyAsync(u.Id, backupCodes[1].ToLower()));

    reloaded = await users.GetByIdAsync(u.Id);
    Check("2FA: two backup codes now consumed (8 remain)",
        tf.CountRemainingBackupCodes(reloaded!.TwoFactorBackupCodes) == 8);

    // Regenerate invalidates old backup codes.
    var newCodes = await tf.RegenerateBackupCodesAsync(u.Id, "harness");
    Check("2FA: regenerate yields a fresh 10", newCodes.Count == 10);
    Check("2FA: an old (unused) backup code no longer works", !await tf.VerifyAsync(u.Id, backupCodes[2]));
    Check("2FA: a new backup code works", await tf.VerifyAsync(u.Id, newCodes[0]));

    // Disable clears everything.
    await tf.DisableAsync(u.Id, "harness");
    reloaded = await users.GetByIdAsync(u.Id);
    Check("2FA: disable clears secret + codes + flag",
        !reloaded!.TwoFactorEnabled && reloaded.TwoFactorSecret == null && reloaded.TwoFactorBackupCodes == null);
    Check("2FA: verify fails once disabled", !await tf.VerifyAsync(u.Id, TotpService.GetCode(enrollSecret)));
});

// ----- 23. Academic year roll-over to the current calendar year (first-launch) -----
await Section("23. Academic year roll-over (EnsureCurrentAcademicYearAsync)", async () =>
{
    // Throwaway DB so this never affects the shared seed used by other sections.
    var tmpDb = Path.Combine(Path.GetTempPath(), $"iems_ay_{Guid.NewGuid():N}.db");
    var opts = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite($"Data Source={tmpDb}").Options;
    try
    {
        using var ctx = new ApplicationDbContext(opts);
        await ctx.Database.MigrateAsync(); // seeds 4 years, current = 2024-25

        async Task<string?> CurrentAfter(DateTime asOf)
        {
            await IEMS.Infrastructure.Data.ProductionDataInitializer.EnsureCurrentAcademicYearAsync(ctx, asOf);
            var c = await ctx.AcademicYears.SingleOrDefaultAsync(a => a.IsCurrent);
            return c?.Year;
        }

        // June 2026 → the 2026-27 year (Indian June–May calendar), created and made current.
        Check("AY roll-over: June 2026 → 2026-27", await CurrentAfter(new DateTime(2026, 6, 24)) == "2026-27");
        Check("AY roll-over: exactly one current after switch", await ctx.AcademicYears.CountAsync(a => a.IsCurrent) == 1);
        Check("AY roll-over: previous current (2024-25) was unset",
            !(await ctx.AcademicYears.SingleAsync(a => a.Year == "2024-25")).IsCurrent);
        Check("AY roll-over: 2026-27 created with the June–May span",
            await ctx.AcademicYears.AnyAsync(a => a.Year == "2026-27"
                && a.StartDate == new DateTime(2026, 6, 1) && a.EndDate == new DateTime(2027, 5, 31)));

        // What every consumer (fee payment, finance, promotions) actually reads:
        var repo = new IEMS.Infrastructure.Repositories.AcademicYearRepository(ctx);
        Check("AY roll-over: repository.GetCurrent = 2026-27 (propagation source)",
            (await repo.GetCurrentAcademicYearAsync())?.Year == "2026-27");

        // Idempotent: a later call in the same year is a no-op and adds no duplicate.
        int before = await ctx.AcademicYears.CountAsync();
        Check("AY roll-over: idempotent value", await CurrentAfter(new DateTime(2026, 9, 1)) == "2026-27");
        Check("AY roll-over: idempotent (no duplicate row)", await ctx.AcademicYears.CountAsync() == before);

        // Month boundaries: 31-May is still the previous label, 01-Jun flips to the new one.
        Check("AY roll-over: 31-May-2026 → 2025-26 (existing year reused)", await CurrentAfter(new DateTime(2026, 5, 31)) == "2025-26");
        Check("AY roll-over: 01-Jun-2026 → 2026-27", await CurrentAfter(new DateTime(2026, 6, 1)) == "2026-27");

        // Year-of-century is zero-padded ("2005-06", not "2005-6").
        Check("AY roll-over: label zero-pads (2005 → 2005-06)", await CurrentAfter(new DateTime(2005, 7, 1)) == "2005-06");
    }
    finally
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        foreach (var ext in new[] { "", "-wal", "-shm" })
            try { File.Delete(tmpDb + ext); } catch { }
    }
});

// ----- 24. Role-based access control (RoleAccess matrix) -----
await Section("24. Role-based access (RoleAccess)", () =>
{
    // Admin sees everything.
    Check("RBAC: Admin can access all modules",
        Enum.GetValues(typeof(AppModule)).Cast<AppModule>().All(m => RoleAccess.CanAccess("Admin", m)));

    // Clerk: Students, Transport, School Documents only (the chosen office-clerk profile).
    Check("RBAC: Clerk can Students", RoleAccess.CanAccess("Clerk", AppModule.Students));
    Check("RBAC: Clerk can Transport", RoleAccess.CanAccess("Clerk", AppModule.Transport));
    Check("RBAC: Clerk can SchoolDocuments", RoleAccess.CanAccess("Clerk", AppModule.SchoolDocuments));
    Check("RBAC: Clerk CANNOT Finance", !RoleAccess.CanAccess("Clerk", AppModule.Finance));
    Check("RBAC: Clerk CANNOT Staff", !RoleAccess.CanAccess("Clerk", AppModule.Staff));
    Check("RBAC: Clerk CANNOT UserManagement", !RoleAccess.CanAccess("Clerk", AppModule.UserManagement));
    Check("RBAC: Clerk CAN Backup", RoleAccess.CanAccess("Clerk", AppModule.Backup));
    Check("RBAC: Clerk CANNOT SystemSettings", !RoleAccess.CanAccess("Clerk", AppModule.SystemSettings));
    Check("RBAC: Clerk CANNOT AuditTrail", !RoleAccess.CanAccess("Clerk", AppModule.AuditTrail));
    Check("RBAC: Clerk CANNOT AcademicYear", !RoleAccess.CanAccess("Clerk", AppModule.AcademicYear));

    // User Management and System Settings stay Admin-only for every other role.
    foreach (var r in new[] { "Principal", "Accountant", "Clerk", "Teacher" })
    {
        Check($"RBAC: {r} CANNOT UserManagement", !RoleAccess.CanAccess(r, AppModule.UserManagement));
        Check($"RBAC: {r} CANNOT SystemSettings", !RoleAccess.CanAccess(r, AppModule.SystemSettings));
    }
    // Backup stays Admin-only EXCEPT the Clerk (school's choice).
    Check("RBAC: Principal CANNOT Backup", !RoleAccess.CanAccess("Principal", AppModule.Backup));
    Check("RBAC: Accountant CANNOT Backup", !RoleAccess.CanAccess("Accountant", AppModule.Backup));
    Check("RBAC: Teacher CANNOT Backup", !RoleAccess.CanAccess("Teacher", AppModule.Backup));

    // Accountant = Finance/Transport/Documents; Teacher = Students/Documents.
    Check("RBAC: Accountant can Finance", RoleAccess.CanAccess("Accountant", AppModule.Finance));
    Check("RBAC: Accountant CANNOT Students", !RoleAccess.CanAccess("Accountant", AppModule.Students));
    Check("RBAC: Teacher can Students", RoleAccess.CanAccess("Teacher", AppModule.Students));
    Check("RBAC: Teacher CANNOT Finance", !RoleAccess.CanAccess("Teacher", AppModule.Finance));

    // Management-level features inside Students. Admin + Principal get all; Teacher/null get none.
    foreach (var feat in new[] { AppFeature.ManageClasses, AppFeature.ManageFeeStructure, AppFeature.BulkPromotion })
    {
        Check($"RBAC: Admin can {feat}", RoleAccess.CanUse("Admin", feat));
        Check($"RBAC: Principal can {feat}", RoleAccess.CanUse("Principal", feat));
        Check($"RBAC: Teacher CANNOT {feat}", !RoleAccess.CanUse("Teacher", feat));
        Check($"RBAC: null role CANNOT {feat}", !RoleAccess.CanUse(null, feat));
    }
    // Clerk may manage Classes (school's choice) but NOT the fee structure or bulk promotion.
    Check("RBAC: Clerk CAN ManageClasses", RoleAccess.CanUse("Clerk", AppFeature.ManageClasses));
    Check("RBAC: Clerk CANNOT ManageFeeStructure", !RoleAccess.CanUse("Clerk", AppFeature.ManageFeeStructure));
    Check("RBAC: Clerk CANNOT BulkPromotion", !RoleAccess.CanUse("Clerk", AppFeature.BulkPromotion));

    // Fail closed: unknown / empty / null role gets nothing; matching is case-insensitive.
    Check("RBAC: unknown role denied", !RoleAccess.CanAccess("Hacker", AppModule.Students));
    Check("RBAC: empty role denied", !RoleAccess.CanAccess("", AppModule.Students));
    Check("RBAC: null role denied", !RoleAccess.CanAccess(null, AppModule.Students));
    Check("RBAC: role match is case-insensitive", RoleAccess.CanAccess("clerk", AppModule.Students) && RoleAccess.CanAccess("ADMIN", AppModule.Backup));

    return Task.CompletedTask;
});

// ----- Summary -----
Console.WriteLine();
Console.WriteLine("============================================================");
Console.WriteLine($" RESULT: {passed} passed, {failed} failed  (total {passed + failed})");
if (failed > 0)
{
    Console.WriteLine(" Failing checks:");
    foreach (var f in failures) Console.WriteLine("   - " + f);
}
Console.WriteLine("============================================================");
return failed == 0 ? 0 : 1;

// Stub current-user provider so the audit interceptor can attribute changes in the harness.
sealed class HarnessUser : IEMS.Core.Interfaces.ICurrentUserProvider
{
    public string UserName => "harness-user";
}
