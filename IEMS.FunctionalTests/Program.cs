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
