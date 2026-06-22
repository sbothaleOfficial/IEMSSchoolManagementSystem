using System.Windows;
using System.Windows.Controls;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace IEMS.WPF;

public partial class MainWindow : Window
{
    private readonly StudentService _studentService;
    private readonly TeacherService _teacherService;
    private readonly ClassService _classService;
    private readonly StaffService _staffService;
    private readonly FeePaymentService _feePaymentService;
    private readonly FeeStructureService _feeStructureService;
    private readonly VehicleService _vehicleService;
    private readonly TransportExpenseService _transportExpenseService;
    private readonly ElectricityBillService _electricityBillService;
    private readonly OtherExpenseService _otherExpenseService;
    private readonly BulkPromotionService _bulkPromotionService;
    private readonly AcademicYearService _academicYearService;
    private readonly UserService _userService;

    public MainWindow(StudentService studentService, TeacherService teacherService, ClassService classService, StaffService staffService, FeePaymentService feePaymentService, FeeStructureService feeStructureService, VehicleService vehicleService, TransportExpenseService transportExpenseService, ElectricityBillService electricityBillService, OtherExpenseService otherExpenseService, BulkPromotionService bulkPromotionService, AcademicYearService academicYearService, UserService userService)
    {
        InitializeComponent();
        _studentService = studentService;
        _teacherService = teacherService;
        _classService = classService;
        _staffService = staffService;
        _feePaymentService = feePaymentService;
        _feeStructureService = feeStructureService;
        _vehicleService = vehicleService;
        _transportExpenseService = transportExpenseService;
        _electricityBillService = electricityBillService;
        _otherExpenseService = otherExpenseService;
        _bulkPromotionService = bulkPromotionService;
        _academicYearService = academicYearService;
        _userService = userService;

        // Update welcome message with current user
        if (LoginWindow.CurrentUser != null)
        {
            txtWelcomeUser.Text = $"Welcome, {LoginWindow.CurrentUser.FullName}";

            // Show User Management only for Admin role (case-insensitive)
            if (!string.Equals(LoginWindow.CurrentUser.Role, "Admin", System.StringComparison.OrdinalIgnoreCase))
            {
                cardUserManagement.Visibility = System.Windows.Visibility.Collapsed;
            }
        }

        lblStatus.Text = "Dashboard loaded successfully";
    }

    private void BtnLogout_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // Clear current user
            LoginWindow.CurrentUser = null;

            // Create and show login window
            var loginWindow = new LoginWindow();
            loginWindow.Show();

            // Set login window as main window before closing this one
            System.Windows.Application.Current.MainWindow = loginWindow;

            // Dispose the service scope if it exists
            if (this.Tag is IServiceScope scope)
            {
                scope.Dispose();
            }

            // Close main window (won't shut down app since MainWindow is now loginWindow)
            this.Close();
        }
    }

    // Dashboard Navigation Event Handlers

    // New Single Entry Point Handlers
    private void BtnStudentManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Direct access to general student management - no confusing popup
            var studentsWindow = new StudentsManagementWindow(_studentService, _classService, _teacherService, _feePaymentService, _feeStructureService, _bulkPromotionService, _academicYearService);
            studentsWindow.ShowDialog();
            lblStatus.Text = "Student Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Student Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Student Management";
        }
    }

    private void BtnTransportManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var transportWindow = new TransportManagementWindow(_vehicleService, _transportExpenseService);
            transportWindow.ShowDialog();
            lblStatus.Text = "Transport Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Transport Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Transport Management";
        }
    }

    private void BtnStaffManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var staffWindow = new StaffManagementWindow(_teacherService, _classService, _staffService);
            staffWindow.ShowDialog();
            lblStatus.Text = "Staff Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Staff Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Staff Management";
        }
    }

    private void BtnFinanceManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var financeWindow = new FinanceManagementWindow(_feePaymentService, _classService, _studentService, _feeStructureService, _electricityBillService, _otherExpenseService, _transportExpenseService, _teacherService, _staffService, _academicYearService);
            financeWindow.ShowDialog();
            lblStatus.Text = "Finance Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Finance Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Finance Management";
        }
    }

    private void BtnBackupRestore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var backupWindow = new BackupRestoreWindow();
            backupWindow.ShowDialog();
            lblStatus.Text = "Backup & Restore module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Backup & Restore: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Backup & Restore";
        }
    }

    private void BtnSystemSettings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var systemSettingsWindow = new SystemSettingsWindow();
            systemSettingsWindow.ShowDialog();
            lblStatus.Text = "System Settings module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening System Settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening System Settings";
        }
    }

    private void BtnUserManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var userManagementWindow = new UserManagementWindow(_userService);
            userManagementWindow.ShowDialog();
            lblStatus.Text = "User Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening User Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening User Management";
        }
    }

    private void BtnAcademicYearManagement_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var academicYearWindow = new AcademicYearManagementWindow(_academicYearService);
            academicYearWindow.ShowDialog();
            lblStatus.Text = "Academic Year Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Academic Year Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Academic Year Management";
        }
    }

    // Legacy Click Handlers (kept for backward compatibility if needed)
    private void BtnStudentsModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var studentsWindow = new StudentsManagementWindow(_studentService, _classService, _teacherService, _feePaymentService, _feeStructureService, _bulkPromotionService, _academicYearService);
            studentsWindow.ShowDialog();
            lblStatus.Text = "Students Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Students Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Students Management";
        }
    }

    private void BtnClassesModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var classesWindow = new ClassesManagementWindow(_classService, _teacherService, _studentService);
            classesWindow.ShowDialog();
            lblStatus.Text = "Classes Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Classes Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Classes Management";
        }
    }

    private void BtnTeachersModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var staffWindow = new StaffManagementWindow(_teacherService, _classService, _staffService);
            staffWindow.ShowDialog();
            lblStatus.Text = "Staff Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Staff Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Staff Management";
        }
    }

    private void BtnBusesModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var transportWindow = new TransportManagementWindow(_vehicleService, _transportExpenseService);
            transportWindow.ShowDialog();
            lblStatus.Text = "Transport Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Transport Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Transport Management";
        }
    }

    private void BtnRoutesModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var transportWindow = new TransportManagementWindow(_vehicleService, _transportExpenseService);
            transportWindow.ShowDialog();
            lblStatus.Text = "Transport Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Transport Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Transport Management";
        }
    }

    private void BtnSupportStaffModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var staffWindow = new StaffManagementWindow(_teacherService, _classService, _staffService);
            staffWindow.ShowDialog();
            lblStatus.Text = "Staff Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Staff Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Staff Management";
        }
    }

    private void BtnFeesModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var studentsWindow = new StudentsManagementWindow(_studentService, _classService, _teacherService, _feePaymentService, _feeStructureService, _bulkPromotionService, _academicYearService);
            studentsWindow.ShowDialog();
            lblStatus.Text = "Student Fees Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Student Fees Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Student Fees Management";
        }
    }

    private void BtnExpensesModule_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var expenseWindow = new ExpenseManagementWindow(_electricityBillService, _otherExpenseService, _transportExpenseService, _feePaymentService, _teacherService, _staffService);
            expenseWindow.ShowDialog();
            lblStatus.Text = "Expense Management module accessed";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening Expense Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error opening Expense Management";
        }
    }
}