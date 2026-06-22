using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using IEMS.Infrastructure.Data;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Repositories;
using IEMS.Application.Services;
using IEMS.Application.Interfaces;
using IEMS.Core.Services;
using IEMS.Core.Configuration;

namespace IEMS.WPF;

public partial class App : System.Windows.Application
{
    private IHost? _host;
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Configure file logging and global crash handling FIRST, so anything that goes wrong
        // from here on is captured to disk instead of vanishing.
        ConfigureLogging();
        SetupGlobalExceptionHandling();
        Log.Information("IEMS application starting");

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlite(DatabaseLocation.ConnectionString));

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

                    // Configuration
                    var bulkPromotionConfig = new BulkPromotionConfiguration
                    {
                        EligibilityRules = new EligibilityRulesConfiguration
                        {
                            MaxPendingFees = 0m,  // Set to 0 since we don't allow any pending fees
                            MinAttendancePercentage = 75,
                            RequireTeacherApproval = false,
                            AllowPromotionWithPendingFees = false  // No pending fees allowed
                        },
                        ClassProgression = new ClassProgressionConfiguration
                        {
                            AllowSameGradePromotion = true,
                            AllowSkipGrade = false,
                            StrictProgressionOnly = true
                        }
                    };
                    services.AddSingleton(bulkPromotionConfig);

                    // Domain Services (will be used by FeePaymentService)
                    services.AddScoped<FeeCalculationService>();
                    services.AddScoped<AmountToWordsService>();
                    services.AddScoped<StudentPromotionService>();
                    services.AddScoped<StudentEligibilityValidator>();
                    services.AddScoped<ClassProgressionValidator>();
                    services.AddScoped<PasswordHashingService>();

                    // Application Services
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
                    services.AddScoped<BulkPromotionService>();
                    services.AddScoped<IBackupService, BackupService>();
                    services.AddScoped<ISystemSettingsService, SystemSettingsService>();
                    services.AddScoped<UserService>();
                    services.AddHostedService<AutomaticBackupService>();

                    services.AddTransient<MainWindow>();
                    services.AddTransient<FeeStructureManagementWindow>();
                    services.AddTransient<AddEditFeeStructureWindow>();
                })
                .Build();

            await _host.StartAsync();

            ServiceProvider = _host.Services;

            using (var scope = _host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                // Ensure default admin user exists
                var userService = scope.ServiceProvider.GetRequiredService<UserService>();
                await userService.EnsureDefaultAdminExistsAsync();
            }

            // Show login window first
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            Log.Information("IEMS startup complete");
            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed");
            MessageBox.Show($"Application startup failed: {ex.Message}\n\nDetails: {ex.InnerException?.Message ?? ex.StackTrace}",
                           "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown(1);
        }
    }

    /// <summary>Writes rolling daily log files to %LOCALAPPDATA%\IEMS\logs (kept 31 days).</summary>
    private static void ConfigureLogging()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "IEMS", "logs");
        Directory.CreateDirectory(logDir);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            // Keep our own logs readable: don't record every EF Core SQL command / host plumbing line.
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.File(
                Path.Combine(logDir, "iems-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 31,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    /// <summary>
    /// Catches exceptions that escape UI handlers, background threads, and faulted tasks.
    /// Without this, any unhandled exception crashed the app silently with no log.
    /// </summary>
    private void SetupGlobalExceptionHandling()
    {
        DispatcherUnhandledException += (s, args) =>
        {
            Log.Error(args.Exception, "Unhandled UI (Dispatcher) exception");
            MessageBox.Show(
                $"An unexpected error occurred:\n\n{args.Exception.Message}\n\n" +
                "The details have been written to the log file. If the app becomes unstable, please restart it.",
                "Unexpected Error", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true; // keep the app alive instead of crashing
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            Log.Fatal(args.ExceptionObject as Exception, "Unhandled non-UI exception (terminating={Terminating})", args.IsTerminating);
            Log.CloseAndFlush();
        };

        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            Log.Error(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("IEMS application exiting (code {Code})", e.ApplicationExitCode);
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}