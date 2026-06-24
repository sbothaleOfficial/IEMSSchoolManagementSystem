using System;
using System.Windows;
using IEMS.Application.Services;
using IEMS.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace IEMS.WPF;

public partial class MainWindow : Window
{
    // Each module window is opened in its OWN DI scope (and therefore its own DbContext),
    // disposed when the modal window closes. Previously every window shared the one
    // session-long DbContext, which accumulated tracked entities for the whole session and
    // leaked one screen's edits into another.
    private readonly IServiceScopeFactory _scopeFactory;

    public MainWindow(IServiceScopeFactory scopeFactory)
    {
        InitializeComponent();
        _scopeFactory = scopeFactory;

        if (LoginWindow.CurrentUser != null)
        {
            txtWelcomeUser.Text = $"Welcome, {LoginWindow.CurrentUser.FullName}";
            ApplyRoleVisibility(LoginWindow.CurrentUser.Role);
        }

        lblStatus.Text = "Dashboard loaded successfully";
    }

    /// <summary>
    /// Shows only the dashboard cards the signed-in role is allowed to use. Driven by the single
    /// <see cref="RoleAccess"/> permission map, so the cards shown always match what OpenModule
    /// will actually allow.
    /// </summary>
    private void ApplyRoleVisibility(string? role)
    {
        var cards = new (FrameworkElement card, AppModule module)[]
        {
            (cardStudents,        AppModule.Students),
            (cardTransport,       AppModule.Transport),
            (cardStaff,           AppModule.Staff),
            (cardFinance,         AppModule.Finance),
            (cardBackup,          AppModule.Backup),
            (cardSystemSettings,  AppModule.SystemSettings),
            (cardUserManagement,  AppModule.UserManagement),
            (cardAcademicYear,    AppModule.AcademicYear),
            (cardAuditTrail,      AppModule.AuditTrail),
            (cardSchoolDocuments, AppModule.SchoolDocuments),
        };

        foreach (var (card, module) in cards)
            card.Visibility = RoleAccess.CanAccess(role, module) ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Opens a module window inside a fresh DI scope, shows it modally, then disposes the
    /// scope (and its DbContext) once the window closes.
    /// </summary>
    private void OpenModule(AppModule module, Func<IServiceProvider, Window> createWindow, string moduleName)
    {
        // Defence in depth: even though the card is hidden for roles that can't use this module,
        // re-check on open so the action can never run for an unauthorised role.
        if (!RoleAccess.CanAccess(LoginWindow.CurrentUser?.Role, module))
        {
            MessageBox.Show($"You don't have permission to open {moduleName}.",
                "Access denied", MessageBoxButton.OK, MessageBoxImage.Warning);
            lblStatus.Text = $"Access denied: {moduleName}";
            return;
        }

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var window = createWindow(scope.ServiceProvider);
            window.Owner = this;
            window.ShowDialog();
            lblStatus.Text = $"{moduleName} module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening {moduleName}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = $"Error opening {moduleName}";
        }
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes)
            return;

        LoginWindow.CurrentUser = null;

        var loginWindow = new LoginWindow();
        loginWindow.Show();
        System.Windows.Application.Current.MainWindow = loginWindow;

        // Dispose the login-time service scope that created this window, if present.
        if (this.Tag is IServiceScope scope)
        {
            scope.Dispose();
        }

        this.Close();
    }

    // ---- Dashboard navigation (each opens in its own scope) ----

    private void BtnStudentManagement_Click(object sender, RoutedEventArgs e) => OpenStudents("Student Management");
    private void BtnStudentsModule_Click(object sender, RoutedEventArgs e) => OpenStudents("Students Management");
    private void BtnFeesModule_Click(object sender, RoutedEventArgs e) => OpenStudents("Student Fees Management");

    private void OpenStudents(string name) => OpenModule(AppModule.Students, sp => new StudentsManagementWindow(
        sp.GetRequiredService<StudentService>(),
        sp.GetRequiredService<ClassService>(),
        sp.GetRequiredService<TeacherService>(),
        sp.GetRequiredService<FeePaymentService>(),
        sp.GetRequiredService<FeeStructureService>(),
        sp.GetRequiredService<BulkPromotionService>(),
        sp.GetRequiredService<AcademicYearService>(),
        sp.GetRequiredService<StudentDocumentService>()), name);

    private void BtnTransportManagement_Click(object sender, RoutedEventArgs e) => OpenTransport("Transport Management");
    private void BtnBusesModule_Click(object sender, RoutedEventArgs e) => OpenTransport("Transport Management");
    private void BtnRoutesModule_Click(object sender, RoutedEventArgs e) => OpenTransport("Transport Management");

    private void OpenTransport(string name) => OpenModule(AppModule.Transport, sp => new TransportManagementWindow(
        sp.GetRequiredService<VehicleService>(),
        sp.GetRequiredService<TransportExpenseService>()), name);

    private void BtnStaffManagement_Click(object sender, RoutedEventArgs e) => OpenStaff("Staff Management");
    private void BtnTeachersModule_Click(object sender, RoutedEventArgs e) => OpenStaff("Staff Management");
    private void BtnSupportStaffModule_Click(object sender, RoutedEventArgs e) => OpenStaff("Staff Management");

    private void OpenStaff(string name) => OpenModule(AppModule.Staff, sp => new StaffManagementWindow(
        sp.GetRequiredService<TeacherService>(),
        sp.GetRequiredService<ClassService>(),
        sp.GetRequiredService<StaffService>()), name);

    private void BtnFinanceManagement_Click(object sender, RoutedEventArgs e) => OpenModule(AppModule.Finance, sp => new FinanceManagementWindow(
        sp.GetRequiredService<FeePaymentService>(),
        sp.GetRequiredService<ClassService>(),
        sp.GetRequiredService<StudentService>(),
        sp.GetRequiredService<FeeStructureService>(),
        sp.GetRequiredService<ElectricityBillService>(),
        sp.GetRequiredService<OtherExpenseService>(),
        sp.GetRequiredService<TransportExpenseService>(),
        sp.GetRequiredService<TeacherService>(),
        sp.GetRequiredService<StaffService>(),
        sp.GetRequiredService<AcademicYearService>()), "Finance Management");

    private void BtnBackupRestore_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.Backup, sp => new BackupRestoreWindow(sp), "Backup & Restore");

    private void BtnSystemSettings_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.SystemSettings, sp => new SystemSettingsWindow(sp), "System Settings");

    private void BtnUserManagement_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.UserManagement, sp => new UserManagementWindow(sp.GetRequiredService<UserService>()), "User Management");

    private void BtnAuditTrail_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.AuditTrail, sp => new AuditLogWindow(sp.GetRequiredService<AuditLogService>()), "Audit Trail");

    private void BtnSchoolDocuments_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.SchoolDocuments, sp => new SchoolDocumentsWindow(
            sp.GetRequiredService<SchoolDocumentService>(),
            LoginWindow.CurrentUser?.Username ?? "admin"), "School Documents");

    private void BtnAcademicYearManagement_Click(object sender, RoutedEventArgs e) =>
        OpenModule(AppModule.AcademicYear, sp => new AcademicYearManagementWindow(sp.GetRequiredService<AcademicYearService>()), "Academic Year Management");

    private void BtnClassesModule_Click(object sender, RoutedEventArgs e) => OpenModule(AppModule.Students, sp => new ClassesManagementWindow(
        sp.GetRequiredService<ClassService>(),
        sp.GetRequiredService<TeacherService>(),
        sp.GetRequiredService<StudentService>()), "Classes Management");

    private void BtnExpensesModule_Click(object sender, RoutedEventArgs e) => OpenModule(AppModule.Finance, sp => new ExpenseManagementWindow(
        sp.GetRequiredService<ElectricityBillService>(),
        sp.GetRequiredService<OtherExpenseService>(),
        sp.GetRequiredService<TransportExpenseService>(),
        sp.GetRequiredService<FeePaymentService>(),
        sp.GetRequiredService<TeacherService>(),
        sp.GetRequiredService<StaffService>()), "Expense Management");
}
