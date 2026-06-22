using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.WPF.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace IEMS.WPF
{
    public partial class FinanceManagementWindow : Window
    {
        private readonly FeePaymentService _feePaymentService;
        private readonly ClassService _classService;
        private readonly StudentService _studentService;
        private readonly FeeStructureService _feeStructureService;
        private readonly ElectricityBillService _electricityBillService;
        private readonly OtherExpenseService _otherExpenseService;
        private readonly TransportExpenseService _transportExpenseService;
        private readonly TeacherService _teacherService;
        private readonly StaffService _staffService;
        private readonly AcademicYearService _academicYearService;
        private List<FeePaymentDto> _allFeePayments = new();
        private List<ClassDto> _allClasses = new();
        private string _currentAcademicYear = DateTime.Now.Year.ToString() + "-" + (DateTime.Now.Year + 1).ToString().Substring(2);


        // Expense management collections
        private ObservableCollection<ElectricityBillDto> _electricityBills = new();
        private ObservableCollection<OtherExpenseDto> _otherExpenses = new();

        public FinanceManagementWindow(FeePaymentService feePaymentService, ClassService classService, StudentService studentService, FeeStructureService feeStructureService, ElectricityBillService electricityBillService, OtherExpenseService otherExpenseService, TransportExpenseService transportExpenseService, TeacherService teacherService, StaffService staffService, AcademicYearService academicYearService)
        {
            InitializeComponent();
            _feePaymentService = feePaymentService;
            _classService = classService;
            _studentService = studentService;
            _feeStructureService = feeStructureService;
            _electricityBillService = electricityBillService;
            _otherExpenseService = otherExpenseService;
            _transportExpenseService = transportExpenseService;
            _teacherService = teacherService;
            _staffService = staffService;
            _academicYearService = academicYearService;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    // Initialize expense management data
                    await LoadElectricityBills();
                    await LoadOtherExpenses();
                    InitializeCategoryFilter();

                    dgElectricityBills.ItemsSource = _electricityBills;
                    dgOtherExpenses.ItemsSource = _otherExpenses;

                    // Initialize academic year dropdown
                    InitializeAcademicYears();

                    // Load expense dashboard
                    await RefreshExpenseDashboard();

                    lblStatus.Text = "Finance Management Ready";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Finance Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Finance Management Window Loading Error");
        }

        private async void InitializeAcademicYears()
        {
            try
            {
                // Load academic years from database
                var academicYears = await _academicYearService.GetAllAcademicYearsAsync();
                var yearsList = academicYears.OrderByDescending(ay => ay.StartDate).Select(ay => ay.Year).ToList();

                cmbAcademicYear.ItemsSource = yearsList;

                // Use centralized method to get current academic year
                var currentAcademicYear = await _academicYearService.GetCurrentAcademicYearAsync();
                if (currentAcademicYear != null)
                {
                    _currentAcademicYear = currentAcademicYear.Year;
                    cmbAcademicYear.SelectedItem = _currentAcademicYear;
                }
                else if (yearsList.Any())
                {
                    cmbAcademicYear.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading academic years: {ex.Message}\n\nUsing fallback years.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                // Fallback: Generate current and next year dynamically
                var currentYear = DateTime.Now.Year;
                var fallbackYears = new List<string>
                {
                    $"{currentYear}-{(currentYear + 1).ToString().Substring(2)}",
                    $"{currentYear + 1}-{(currentYear + 2).ToString().Substring(2)}",
                    $"{currentYear - 1}-{currentYear.ToString().Substring(2)}"
                };
                cmbAcademicYear.ItemsSource = fallbackYears;
                cmbAcademicYear.SelectedIndex = 0;
            }
        }

        private async Task LoadClasses()
        {
            try
            {
                _allClasses = (await _classService.GetAllClassesAsync()).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading classes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadFeePayments()
        {
            try
            {
                _allFeePayments = (await _feePaymentService.GetAllFeePaymentsAsync()).ToList();
                ApplyFeeFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fee payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadAnalytics()
        {
            try
            {
                var analytics = await _feePaymentService.GetFeeAnalyticsAsync(_currentAcademicYear);
                await LoadPendingFees();
                lblStatus.Text = $"Analytics updated for {_currentAcademicYear}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading analytics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadPendingFees(int? classId = null)
        {
            try
            {
                var pendingFees = await _feePaymentService.GetStudentsWithPendingFeesAsync(_currentAcademicYear, classId);
                lblStatus.Text = $"Found {pendingFees.Count} students with pending fees";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading pending fees: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFeeFilter()
        {
            // FIXED BUG #11: Method body removed as Fee Analytics tab functionality was removed
            // This method is no longer used but kept for potential future use
        }

        #region Event Handlers

        private void CmbAcademicYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                if (cmbAcademicYear.SelectedItem != null)
                {
                    _currentAcademicYear = cmbAcademicYear.SelectedItem.ToString() ?? _currentAcademicYear;
                    await LoadAnalytics();
                    // FIXED BUG #3: Refresh expense dashboard when academic year changes
                    await RefreshExpenseDashboard();
                }
            }, "Academic Year Selection Error");
        }

        private void BtnRefreshAnalytics_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                await LoadFeePayments();
                await LoadAnalytics();
            }, "Analytics Refresh Error");
        }

        private void CmbClassFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // FIXED BUG #11: Method body removed as Fee Analytics tab functionality was removed
            // This method is no longer used but kept for potential future use
        }

        private void BtnGoToStudentManagement_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Close current window
                this.Close();

                // Open Student Management window directly on Fee Payment tab
                var studentsWindow = new StudentsManagementWindow(
                    _studentService,
                    _classService,
                    _teacherService,
                    _feePaymentService,
                    _feeStructureService,
                    null, // BulkPromotionService (optional)
                    null  // IAcademicYearRepository (optional)
                );

                // Note: User will need to manually navigate to Fee Payment tab
                // as the TabControl doesn't have a public name
                studentsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Student Management: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnApplyFeeFilter_Click(object sender, RoutedEventArgs e)
        {
            ApplyFeeFilter();
        }

        private void BtnExportPendingFees_Click(object sender, RoutedEventArgs e)
        {
            // FIXED BUG #11: Method functionality removed as Fee Analytics tab was removed
            // This method is no longer used but kept for potential future use
            MessageBox.Show("Export functionality is not available. Fee Analytics tab has been removed.",
                "Feature Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Export Functionality

        private void ExportToExcel(List<StudentFeeAnalyticsDto> pendingFees)
        {
            try
            {
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                    DefaultExt = "csv",
                    FileName = $"PendingFees_{_currentAcademicYear}_{DateTime.Now:yyyyMMdd}.csv"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                    {
                        // Write header
                        writer.WriteLine("Student Name,Class,Student Number,Parent Phone,Total Pending Amount,Last Payment Date,Days Since Last Payment");

                        // Write data
                        foreach (var student in pendingFees)
                        {
                            writer.WriteLine($"\"{student.StudentName}\",\"{student.ClassName}\",\"{student.StudentNumber}\",\"{student.ParentPhone}\",{student.TotalPendingAmount:F2},{student.LastPaymentDate:yyyy-MM-dd},{student.DaysSinceLastPayment}");
                        }
                    }

                    MessageBox.Show($"Data exported successfully to {saveDialog.FileName}", "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    lblStatus.Text = $"Exported {pendingFees.Count} records to CSV";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion


        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #region Expense Management Methods

        #region Electricity Bills Tab

        private async Task LoadElectricityBills()
        {
            try
            {
                var bills = await _electricityBillService.GetAllAsync();
                _electricityBills.Clear();

                if (bills != null)
                {
                    foreach (var bill in bills)
                    {
                        if (bill != null)
                        {
                            _electricityBills.Add(bill);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading electricity bills: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = $"Error loading electricity bills: {ex.Message}";
            }
        }

        private void BtnAddElectricityBill_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    var addWindow = new AddEditElectricityBillWindow(_electricityBillService);
                    if (addWindow.ShowDialog() == true)
                    {
                        await LoadElectricityBills();
                        await RefreshExpenseDashboard();
                        lblStatus.Text = "Electricity bill added successfully";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening add electricity bill window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void BtnEditElectricityBill_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (sender is Button button && button.Tag is int billId)
                {
                    var editWindow = new AddEditElectricityBillWindow(_electricityBillService, billId);
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadElectricityBills();
                        await RefreshExpenseDashboard();
                        lblStatus.Text = "Electricity bill updated successfully";
                    }
                }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening edit electricity bill window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        private void BtnDeleteElectricityBill_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (sender is Button button && button.Tag is int billId)
                    {
                        var result = MessageBox.Show("Are you sure you want to delete this electricity bill?",
                            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            await _electricityBillService.DeleteAsync(billId);
                            await LoadElectricityBills();
                            await RefreshExpenseDashboard();
                            lblStatus.Text = "Electricity bill deleted successfully";
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting electricity bill: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Delete Electricity Bill Error");
        }

        private void DgElectricityBills_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                if (dgElectricityBills.SelectedItem is ElectricityBillDto selectedBill)
                {
                    var editWindow = new AddEditElectricityBillWindow(_electricityBillService, selectedBill.Id);
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadElectricityBills();
                        await RefreshExpenseDashboard();
                    }
                }
            }, "Electricity Bill Double Click Error");
        }

        private void BtnRefreshElectricityBills_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    await LoadElectricityBills();
                    lblStatus.Text = "Electricity bills refreshed successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error refreshing electricity bills: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error refreshing electricity bills";
                }
            });
        }

        #endregion

        #region Other Expenses Tab

        private async Task LoadOtherExpenses()
        {
            try
            {
                var expenses = await _otherExpenseService.GetAllAsync();
                _otherExpenses.Clear();
                foreach (var expense in expenses)
                {
                    _otherExpenses.Add(expense);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading other expenses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InitializeCategoryFilter()
        {
            var categories = new List<string> { "All Categories" };
            categories.AddRange(Enum.GetNames<OtherExpenseCategory>().Select(c => c.Replace("_", " ")));
            cmbCategoryFilter.ItemsSource = categories;
            cmbCategoryFilter.SelectedIndex = 0;
        }

        private void CmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (cmbCategoryFilter.SelectedItem is string selectedCategory && selectedCategory != "All Categories")
                    {
                        // FIXED BUG #10: Use TryParse for safer enum parsing
                        if (Enum.TryParse<OtherExpenseCategory>(selectedCategory.Replace(" ", "_"), out var category))
                        {
                            var filteredExpenses = await _otherExpenseService.GetByCategoryAsync(category);

                            _otherExpenses.Clear();
                            foreach (var expense in filteredExpenses)
                            {
                                _otherExpenses.Add(expense);
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Invalid category selected: {selectedCategory}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                            cmbCategoryFilter.SelectedIndex = 0; // Reset to "All Categories"
                        }
                    }
                    else
                    {
                        await LoadOtherExpenses();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error filtering expenses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Category Filter Selection Error");
        }

        private void BtnClearFilter_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                cmbCategoryFilter.SelectedIndex = 0;
                await LoadOtherExpenses();
            }, "Clear Filter Error");
        }

        private void BtnAddOtherExpense_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    var addWindow = new AddEditOtherExpenseWindow(_otherExpenseService);
                    if (addWindow.ShowDialog() == true)
                    {
                        await LoadOtherExpenses();
                        await RefreshExpenseDashboard();
                        lblStatus.Text = "Other expense added successfully";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening add other expense window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Add Other Expense Error");
        }

        private void BtnEditOtherExpense_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (dgOtherExpenses.SelectedItem is OtherExpenseDto selectedExpense)
                    {
                        var editWindow = new AddEditOtherExpenseWindow(_otherExpenseService, selectedExpense.Id);
                        if (editWindow.ShowDialog() == true)
                        {
                            await LoadOtherExpenses();
                            await RefreshExpenseDashboard();
                            lblStatus.Text = "Other expense updated successfully";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select an expense to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening edit other expense window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Edit Other Expense Error");
        }

        private void BtnDeleteOtherExpense_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (dgOtherExpenses.SelectedItem is OtherExpenseDto selectedExpense)
                    {
                        var result = MessageBox.Show($"Are you sure you want to delete the expense '{selectedExpense.Description}'?",
                            "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            await _otherExpenseService.DeleteAsync(selectedExpense.Id);
                            await LoadOtherExpenses();
                            await RefreshExpenseDashboard();
                            lblStatus.Text = "Other expense deleted successfully";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Please select an expense to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting other expense: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Delete Other Expense Error");
        }

        private void DgOtherExpenses_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                if (dgOtherExpenses.SelectedItem is OtherExpenseDto selectedExpense)
                {
                    var editWindow = new AddEditOtherExpenseWindow(_otherExpenseService, selectedExpense.Id);
                    if (editWindow.ShowDialog() == true)
                    {
                        await LoadOtherExpenses();
                        await RefreshExpenseDashboard();
                    }
                }
            }, "Other Expense Double Click Error");
        }

        private void BtnRefreshOtherExpenses_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    await LoadOtherExpenses();
                    lblStatus.Text = "Other expenses refreshed successfully";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error refreshing other expenses: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error refreshing other expenses";
                }
            });
        }

        #endregion

        #region Expense Analytics Tab

        private void ViewRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                await RefreshExpenseDashboard();
            }, "View Radio Button Error");
        }

        private void BtnRefreshDashboard_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                await RefreshExpenseDashboard();
            }, "Refresh Dashboard Error");
        }

        private async Task RefreshExpenseDashboard()
        {
            try
            {
                // Check if UI elements are loaded
                if (rbMonthly == null || rbYearly == null || rbOverall == null ||
                    txtElectricityTotal == null || txtOtherExpensesTotal == null ||
                    txtTransportTotal == null || txtSalariesTotal == null ||
                    txtTotalIncome == null || txtTotalExpenses == null ||
                    txtNetBalance == null || dgCategoryBreakdown == null ||
                    txtFeesCollected == null || txtPendingFees == null ||
                    txtAllExpensesTotal == null || dgFeeBreakdown == null)
                {
                    return; // UI not ready yet
                }

                DateTime fromDate, toDate;
                GetDateRange(out fromDate, out toDate);

                // FIXED BUG #8: Use cash basis accounting - only count PAID bills by their PaidDate
                var electricityBills = await _electricityBillService.GetAllAsync();

                var electricityTotal = electricityBills
                    .Where(b => b.IsPaid && b.PaidDate.HasValue &&
                                b.PaidDate.Value >= fromDate && b.PaidDate.Value <= toDate)
                    .Sum(b => b.Amount);

                var otherExpenses = await _otherExpenseService.GetAllAsync();
                var otherExpensesTotal = otherExpenses
                    .Where(e => e.ExpenseDate >= fromDate && e.ExpenseDate <= toDate)
                    .Sum(e => e.Amount);

                // For transport expenses - get actual data from transport service
                var transportExpenses = await _transportExpenseService.GetAllExpensesAsync();
                var transportTotal = transportExpenses
                    .Where(t => t.ExpenseDate >= fromDate && t.ExpenseDate <= toDate)
                    .Sum(t => t.Amount);

                // Staff salary expense for the selected reporting period.
                // PREVIOUS BUG: each employee's monthly salary was multiplied by their ENTIRE
                // tenure in months. Because the "Overall" range starts in year 2000, this
                // reported cumulative lifetime payroll (tens of crores) and dwarfed every other
                // figure on the dashboard. We now report payroll for the number of months the
                // selected period represents: Monthly -> 1 month, Yearly/Overall -> 12-month
                // run-rate, counting only staff already employed by the end of the period.
                var allTeachers = await _teacherService.GetAllTeachersAsync();
                var allStaff = await _staffService.GetAllStaffAsync();

                int periodMonths = (rbMonthly?.IsChecked == true) ? 1 : 12;

                decimal monthlyPayroll =
                    allTeachers.Where(t => t.JoiningDate.Date <= toDate.Date).Sum(t => t.MonthlySalary) +
                    allStaff.Where(s => s.JoiningDate.Date <= toDate.Date).Sum(s => s.MonthlySalary);

                decimal salariesTotal = monthlyPayroll * periodMonths;

                // Get income data from fee payments
                var feePayments = await _feePaymentService.GetAllFeePaymentsAsync();
                var feesCollected = feePayments
                    .Where(f => f.PaymentDate >= fromDate && f.PaymentDate <= toDate)
                    .Sum(f => f.AmountPaid);

                // FIXED BUG #5: Calculate pending fees using ABSOLUTE latest payments, not date-filtered
                // Pending fees should always reflect current balance, regardless of selected date range
                var allStudents = await _studentService.GetAllStudentsAsync();

                decimal totalPendingFees = 0;

                foreach (var student in allStudents)
                {
                    // Get ALL payments for this student (no date filtering for pending balance)
                    var studentPayments = feePayments.Where(fp => fp.StudentId == student.Id);

                    // Group by fee type and take the ABSOLUTE latest payment's RemainingBalance
                    var latestPaymentsPerFeeType = studentPayments
                        .GroupBy(fp => fp.FeeType)
                        .Select(g => g.OrderByDescending(fp => fp.PaymentDate).First());

                    // Sum up the remaining balances
                    totalPendingFees += latestPaymentsPerFeeType.Sum(fp => fp.RemainingBalance);
                }

                // Update UI - New summary cards
                txtFeesCollected.Text = $"₹{feesCollected:N2}";
                txtPendingFees.Text = $"₹{totalPendingFees:N2}";

                // Update detailed expense cards
                txtElectricityTotal.Text = $"₹{electricityTotal:N2}";
                txtOtherExpensesTotal.Text = $"₹{otherExpensesTotal:N2}";
                txtTransportTotal.Text = $"₹{transportTotal:N2}";
                txtSalariesTotal.Text = $"₹{salariesTotal:N2}";

                txtTotalIncome.Text = $"₹{feesCollected:N2}";

                var totalExpenses = electricityTotal + otherExpensesTotal + transportTotal + salariesTotal;
                txtTotalExpenses.Text = $"₹{totalExpenses:N2}";
                txtAllExpensesTotal.Text = $"₹{totalExpenses:N2}";

                var netBalance = feesCollected - totalExpenses;
                txtNetBalance.Text = $"₹{netBalance:N2}";
                txtNetBalance.Foreground = netBalance >= 0
                    ? new SolidColorBrush(Color.FromRgb(76, 175, 80)) // Green
                    : new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red

                // Update category breakdown
                var categoryData = new List<dynamic>
                {
                    new { Category = "Electricity", Amount = electricityTotal, Percentage = totalExpenses > 0 ? (electricityTotal / totalExpenses) * 100 : 0 },
                    new { Category = "Other Expenses", Amount = otherExpensesTotal, Percentage = totalExpenses > 0 ? (otherExpensesTotal / totalExpenses) * 100 : 0 },
                    new { Category = "Transport", Amount = transportTotal, Percentage = totalExpenses > 0 ? (transportTotal / totalExpenses) * 100 : 0 },
                    new { Category = "Staff Salaries", Amount = salariesTotal, Percentage = totalExpenses > 0 ? (salariesTotal / totalExpenses) * 100 : 0 }
                };

                dgCategoryBreakdown.ItemsSource = categoryData.Where(c => c.Amount > 0);

                // FIXED BUG #6: Update fee breakdown data using ABSOLUTE latest payments for pending
                var feeBreakdownData = new List<dynamic>();
                var feeTypes = Enum.GetValues<FeeType>();

                foreach (var feeType in feeTypes)
                {
                    // Amount collected for this fee type in the selected period
                    var collectedInPeriod = feePayments
                        .Where(f => f.FeeType == feeType && f.PaymentDate >= fromDate && f.PaymentDate <= toDate)
                        .Sum(f => f.AmountPaid);

                    // Calculate pending using ABSOLUTE latest payments (no date filtering)
                    var paymentsForFeeType = feePayments.Where(f => f.FeeType == feeType);

                    // Group by student and take ABSOLUTE latest payment's RemainingBalance
                    var pendingForFeeType = paymentsForFeeType
                        .GroupBy(f => f.StudentId)
                        .Select(g => g.OrderByDescending(f => f.PaymentDate).First().RemainingBalance)
                        .Sum();

                    var expectedForFeeType = collectedInPeriod + pendingForFeeType;

                    if (collectedInPeriod > 0 || pendingForFeeType > 0)
                    {
                        feeBreakdownData.Add(new {
                            FeeType = feeType.ToString().Replace("_", " "),
                            CollectedAmount = collectedInPeriod,
                            PendingAmount = pendingForFeeType,
                            ExpectedAmount = expectedForFeeType
                        });
                    }
                }

                dgFeeBreakdown.ItemsSource = feeBreakdownData;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing expense dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GetDateRange(out DateTime fromDate, out DateTime toDate)
        {
            var today = DateTime.Today;

            // toDate must cover the WHOLE last day. Payments store PaymentDate = DateTime.Now
            // (a real clock time), and expense/bill dates can carry a time too, so a midnight
            // upper bound (e.g. "June 30 00:00") excluded everything recorded on the last day
            // of the period. Use the last tick of the day instead.
            if (rbMonthly?.IsChecked == true)
            {
                fromDate = new DateTime(today.Year, today.Month, 1);
                toDate = fromDate.AddMonths(1).AddTicks(-1);
            }
            else if (rbYearly?.IsChecked == true)
            {
                fromDate = new DateTime(today.Year, 1, 1);
                toDate = new DateTime(today.Year + 1, 1, 1).AddTicks(-1);
            }
            else // Overall
            {
                // FIXED BUG #4: Use reasonable date range instead of DateTime.MinValue/MaxValue
                // to avoid SQL overflow errors and comparison issues
                fromDate = new DateTime(2000, 1, 1); // Reasonable start date (school likely started after 2000)
                toDate = DateTime.Today.AddYears(1); // One year in future to catch any future-dated entries
            }
        }

        #endregion

        #endregion
    }
}