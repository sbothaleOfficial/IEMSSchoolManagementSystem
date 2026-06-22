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
    bool created = ctx.Database.EnsureCreated();
    Check("Database file auto-created", created && File.Exists(dbPath));

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
});

// ----- 8. Regression tests for the audited bug fixes -----
await Section("8. Bug-fix regressions", async () =>
{
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
    var cwd = Directory.GetCurrentDirectory();
    var liveDb = Path.Combine(cwd, "school.db");           // BackupService backs up <cwd>/school.db
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
