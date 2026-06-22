using System.Windows;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Documents;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class StaffManagementWindow : Window
{
    private readonly TeacherService _teacherService;
    private readonly ClassService _classService;
    private readonly StaffService _staffService;

    private List<TeacherDto> _allTeachers = new List<TeacherDto>();
    private List<StaffDto> _allStaff = new List<StaffDto>();
    private bool _teachersLoaded = false;
    private bool _staffLoaded = false;

    public StaffManagementWindow(TeacherService teacherService, ClassService classService, StaffService staffService)
    {
        InitializeComponent();
        _teacherService = teacherService;
        _classService = classService;
        _staffService = staffService;

        SetupSearchControls();
        AsyncHelper.SafeFireAndForget(LoadAllDataAsync);
        lblStatus.Text = "Staff Management loaded successfully";
    }

    private void SetupSearchControls()
    {
        cmbStaffPosition.SelectedIndex = 0; // Select "All Positions" by default
        txtSearchTeacher.Text = string.Empty;
        txtSearchStaff.Text = string.Empty;
    }

    private async Task LoadAllDataAsync()
    {
        try
        {
            // Load teachers and staff data first (can run in parallel)
            await Task.WhenAll(LoadTeachersAsync(), LoadStaffAsync());

            // Then load dashboard data (depends on teachers and staff being loaded)
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading staff management data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Teachers Tab Methods
    private async Task LoadTeachersAsync()
    {
        try
        {
            _allTeachers = (await _teacherService.GetAllTeachersAsync()).ToList();
            _teachersLoaded = true;
            ApplyTeacherSearch();
            lblStatus.Text = $"Loaded {_allTeachers.Count} teachers";
            CheckAndInitializePayslip();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading teachers: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error loading teachers";
        }
    }

    private void ApplyTeacherSearch()
    {
        var searchText = txtSearchTeacher?.Text?.Trim().ToLower() ?? string.Empty;
        var filteredTeachers = _allTeachers.AsEnumerable();

        if (!string.IsNullOrEmpty(searchText))
        {
            filteredTeachers = filteredTeachers.Where(t =>
                t.FullName.ToLower().Contains(searchText) ||
                t.EmployeeId.ToLower().Contains(searchText) ||
                t.PhoneNumber.Contains(searchText) ||
                (t.Email?.ToLower().Contains(searchText) ?? false) ||
                (t.Address?.ToLower().Contains(searchText) ?? false) ||
                (t.BankAccountNumber?.Contains(searchText) ?? false) ||
                (t.AadharNumber?.Contains(searchText) ?? false) ||
                (t.PANNumber?.ToLower().Contains(searchText) ?? false)
            );
        }

        dgTeachers.ItemsSource = filteredTeachers.ToList();
        lblStatus.Text = $"Showing {filteredTeachers.Count()} of {_allTeachers.Count} teachers";
    }

    private void BtnAddTeacher_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                var addEditWindow = new AddEditTeacherWindow(_teacherService);
                if (addEditWindow.ShowDialog() == true)
                {
                    await LoadTeachersAsync();
                    UpdateDashboard();
                    lblStatus.Text = "Teacher added successfully";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Teacher window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    private void BtnEditTeacher_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgTeachers.SelectedItem is TeacherDto selectedTeacher)
                {
                    var addEditWindow = new AddEditTeacherWindow(_teacherService, selectedTeacher);
                    if (addEditWindow.ShowDialog() == true)
                    {
                        await LoadTeachersAsync();
                        UpdateDashboard();
                        lblStatus.Text = "Teacher updated successfully";
                    }
                }
                else
                {
                    MessageBox.Show("Please select a teacher to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Edit Teacher window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    private void BtnDeleteTeacher_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgTeachers.SelectedItem is TeacherDto selectedTeacher)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete teacher '{selectedTeacher.FullName}'?\n\nThis action cannot be undone.",
                                               "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _teacherService.DeleteTeacherAsync(selectedTeacher.Id);
                        await LoadTeachersAsync();
                        UpdateDashboard();
                        lblStatus.Text = "Teacher deleted successfully";
                        MessageBox.Show("Teacher deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a teacher to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting teacher: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error deleting teacher";
            }
        });
    }

    private void BtnRefreshTeachers_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadTeachersAsync);
    }

    // Staff Tab Methods
    private async Task LoadStaffAsync()
    {
        try
        {
            _allStaff = (await _staffService.GetAllStaffAsync()).ToList();
            _staffLoaded = true;
            PopulateStaffPositions();
            ApplyStaffSearch();
            lblStatus.Text = $"Loaded {_allStaff.Count} staff members";
            CheckAndInitializePayslip();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading staff: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error loading staff";
        }
    }

    // Populate the position filter from the actual staff positions so every real
    // position (Cook, Electrician, Mechanic, Office Assistant, ...) is filterable,
    // instead of a hard-coded list that silently omitted some.
    private void PopulateStaffPositions()
    {
        if (cmbStaffPosition == null) return;

        var current = cmbStaffPosition.SelectedItem?.ToString();

        var positions = new List<string> { "All Positions" };
        positions.AddRange(_allStaff
            .Select(s => s.Position)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p));

        cmbStaffPosition.ItemsSource = positions;
        cmbStaffPosition.SelectedItem = (current != null && positions.Contains(current)) ? current : "All Positions";
    }

    private void ApplyStaffSearch()
    {
        var searchText = txtSearchStaff?.Text?.Trim().ToLower() ?? string.Empty;
        var selectedPosition = cmbStaffPosition?.SelectedItem?.ToString();
        var filteredStaff = _allStaff.AsEnumerable();

        // Filter by position if not "All Positions"
        if (!string.IsNullOrEmpty(selectedPosition) && selectedPosition != "All Positions")
        {
            filteredStaff = filteredStaff.Where(s => s.Position.Equals(selectedPosition, StringComparison.OrdinalIgnoreCase));
        }

        // Filter by search text
        if (!string.IsNullOrEmpty(searchText))
        {
            filteredStaff = filteredStaff.Where(s =>
                s.FullName.ToLower().Contains(searchText) ||
                s.EmployeeId.ToLower().Contains(searchText) ||
                s.Position.ToLower().Contains(searchText) ||
                s.PhoneNumber.Contains(searchText) ||
                (s.Email?.ToLower().Contains(searchText) ?? false) ||
                (s.Address?.ToLower().Contains(searchText) ?? false) ||
                (s.BankAccountNumber?.Contains(searchText) ?? false) ||
                (s.AadharNumber?.Contains(searchText) ?? false) ||
                (s.PANNumber?.ToLower().Contains(searchText) ?? false)
            );
        }

        dgStaff.ItemsSource = filteredStaff.ToList();
        lblStatus.Text = $"Showing {filteredStaff.Count()} of {_allStaff.Count} staff members";
    }

    private void BtnAddStaff_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                var addEditWindow = new AddEditStaffWindow(_staffService);
                if (addEditWindow.ShowDialog() == true)
                {
                    await LoadStaffAsync();
                    UpdateDashboard();
                    lblStatus.Text = "Staff member added successfully";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Add Staff window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    private void BtnEditStaff_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStaff.SelectedItem is StaffDto selectedStaff)
                {
                    var addEditWindow = new AddEditStaffWindow(_staffService, selectedStaff);
                    if (addEditWindow.ShowDialog() == true)
                    {
                        await LoadStaffAsync();
                        UpdateDashboard();
                        lblStatus.Text = "Staff member updated successfully";
                    }
                }
                else
                {
                    MessageBox.Show("Please select a staff member to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening Edit Staff window: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        });
    }

    private void BtnDeleteStaff_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStaff.SelectedItem is StaffDto selectedStaff)
                {
                    var result = MessageBox.Show($"Are you sure you want to delete staff member '{selectedStaff.FullName}'?\n\nThis action cannot be undone.",
                                               "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _staffService.DeleteStaffAsync(selectedStaff.Id);
                        await LoadStaffAsync();
                        UpdateDashboard();
                        lblStatus.Text = "Staff member deleted successfully";
                        MessageBox.Show("Staff member deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a staff member to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting staff member: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error deleting staff member";
            }
        });
    }

    private void BtnRefreshStaff_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadStaffAsync);
    }

    // Search Event Handlers
    private void TxtSearchTeacher_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyTeacherSearch();
    }

    private void BtnClearTeacherSearch_Click(object sender, RoutedEventArgs e)
    {
        txtSearchTeacher.Text = string.Empty;
        ApplyTeacherSearch();
    }

    private void TxtSearchStaff_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyStaffSearch();
    }

    private void CmbStaffPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyStaffSearch();
    }

    private void BtnClearStaffSearch_Click(object sender, RoutedEventArgs e)
    {
        txtSearchStaff.Text = string.Empty;
        cmbStaffPosition.SelectedIndex = 0; // Reset to "All Positions"
        ApplyStaffSearch();
    }

    // Dashboard Methods
    private async Task LoadDashboardDataAsync()
    {
        try
        {
            await LoadOverallStatistics();
            await LoadPositionStatistics();
            await LoadPayrollSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadOverallStatistics()
    {
        var totalTeachers = _allTeachers.Count;
        var totalSupportStaff = _allStaff.Count;
        var totalStaff = totalTeachers + totalSupportStaff;

        var allSalaries = _allTeachers.Select(t => t.MonthlySalary)
                                     .Concat(_allStaff.Select(s => s.MonthlySalary))
                                     .Where(s => s > 0);
        var totalMonthlySalary = allSalaries.Sum();

        // Calculate total spent till today based on joining dates
        var totalSpentTillToday = 0m;
        var currentDate = DateTime.Now;

        foreach (var teacher in _allTeachers)
        {
            var monthsWorked = ((currentDate.Year - teacher.JoiningDate.Year) * 12) + currentDate.Month - teacher.JoiningDate.Month;
            if (currentDate.Day < teacher.JoiningDate.Day) monthsWorked--; // Adjust for partial month
            totalSpentTillToday += teacher.MonthlySalary * Math.Max(0, monthsWorked);
        }

        foreach (var staff in _allStaff)
        {
            var monthsWorked = ((currentDate.Year - staff.JoiningDate.Year) * 12) + currentDate.Month - staff.JoiningDate.Month;
            if (currentDate.Day < staff.JoiningDate.Day) monthsWorked--; // Adjust for partial month
            totalSpentTillToday += staff.MonthlySalary * Math.Max(0, monthsWorked);
        }

        lblTotalTeachers.Text = totalTeachers.ToString();
        lblTotalSupportStaff.Text = totalSupportStaff.ToString();
        lblTotalStaff.Text = totalStaff.ToString();
        lblTotalMonthlySalary.Text = $"₹{totalMonthlySalary:N0}";
        lblTotalSpentTillToday.Text = $"₹{totalSpentTillToday:N0}";
    }


    private async Task LoadPositionStatistics()
    {
        var allPositions = _allTeachers.Select(t => "Teacher")
                                      .Concat(_allStaff.Select(s => s.Position))
                                      .ToList();

        // Handle case when there are no employees
        if (allPositions.Count == 0)
        {
            dgPositionStats.ItemsSource = null;
            return;
        }

        var allSalariesWithPositions = _allTeachers.Select(t => new { Position = "Teacher", Salary = t.MonthlySalary })
                                                  .Concat(_allStaff.Select(s => new { Position = s.Position, Salary = s.MonthlySalary }))
                                                  .ToList();

        var positionStats = allPositions
            .GroupBy(p => p)
            .Select(g =>
            {
                var positionSalaries = allSalariesWithPositions.Where(x => x.Position == g.Key).ToList();
                var avgSalary = positionSalaries.Any() ? positionSalaries.Average(x => x.Salary) : 0;
                var totalSalary = positionSalaries.Sum(x => x.Salary);

                return new
                {
                    Position = g.Key,
                    Count = g.Count(),
                    FormattedAvgSalary = $"₹{avgSalary:N0}",
                    FormattedTotalSalary = $"₹{totalSalary:N0}",
                    Percentage = $"{(g.Count() * 100.0 / allPositions.Count):F1}%"
                };
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        dgPositionStats.ItemsSource = positionStats;
    }



    private async Task LoadPayrollSummary()
    {
        var teachersPayroll = _allTeachers.Sum(t => t.MonthlySalary);
        var supportStaffPayroll = _allStaff.Sum(s => s.MonthlySalary);
        var totalPayroll = teachersPayroll + supportStaffPayroll;

        lblTeachersPayroll.Text = $"₹{teachersPayroll:N0}";
        lblSupportStaffPayroll.Text = $"₹{supportStaffPayroll:N0}";
        lblTotalPayroll.Text = $"₹{totalPayroll:N0}";
    }

    // Update dashboard when data changes
    private void UpdateDashboard()
    {
        AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync);
    }

    // Helper method to check if both teachers and staff are loaded
    private void CheckAndInitializePayslip()
    {
        if (_teachersLoaded && _staffLoaded)
        {
            try
            {
                InitializePayslipTab();
            }
            catch (Exception ex)
            {
                // If payslip initialization fails, just log it but don't crash the app
                lblStatus.Text = $"Payslip initialization skipped: {ex.Message}";
            }
        }
    }

    // Payslip Generation Methods
    private void InitializePayslipTab()
    {
        try
        {
            LoadPayslipEmployees();
            InitializePayslipYears();
            SetDefaultPayslipDate();
            ClearPayslipForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing payslip tab: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadPayslipEmployees()
    {
        try
        {
            // Check if payslip controls exist
            if (cmbEmployee == null)
            {
                throw new InvalidOperationException("Payslip controls not yet initialized");
            }

            var allEmployees = new List<object>();

            // Add teachers as employees
            if (_allTeachers != null)
            {
                foreach (var teacher in _allTeachers)
                {
                    allEmployees.Add(new
                    {
                        Id = $"T_{teacher.Id}",
                        FullName = teacher.FullName,
                        EmployeeId = teacher.EmployeeId,
                        Position = "Teacher",
                        MonthlySalary = teacher.MonthlySalary,
                        JoiningDate = teacher.JoiningDate
                    });
                }
            }

            // Add staff as employees
            if (_allStaff != null)
            {
                foreach (var staff in _allStaff)
                {
                    allEmployees.Add(new
                    {
                        Id = $"S_{staff.Id}",
                        FullName = staff.FullName,
                        EmployeeId = staff.EmployeeId,
                        Position = staff.Position,
                        MonthlySalary = staff.MonthlySalary,
                        JoiningDate = staff.JoiningDate
                    });
                }
            }

            cmbEmployee.ItemsSource = allEmployees;

            // Update status with employee count (without sensitive salary data)
            if (lblStatus != null)
            {
                lblStatus.Text = $"Payslip ready: {allEmployees.Count} employees loaded ({_allTeachers?.Count ?? 0} teachers, {_allStaff?.Count ?? 0} staff)";
            }
        }
        catch (Exception ex)
        {
            if (lblStatus != null)
            {
                lblStatus.Text = $"Error loading payslip employees: {ex.Message}";
            }
        }
    }

    private void InitializePayslipYears()
    {
        if (cmbYear == null) return;

        var years = new List<int>();
        var currentYear = DateTime.Now.Year;

        for (int year = currentYear - 2; year <= currentYear + 1; year++)
        {
            years.Add(year);
        }

        cmbYear.ItemsSource = years;
        cmbYear.SelectedItem = currentYear;
    }

    private void SetDefaultPayslipDate()
    {
        try
        {
            if (cmbMonth == null) return;
            var currentMonth = DateTime.Now.Month;
            cmbMonth.SelectedIndex = currentMonth - 1; // Months are 0-indexed in ComboBox
        }
        catch (Exception ex)
        {
            // Safely ignore initialization issues
            if (lblStatus != null)
                lblStatus.Text = $"Payslip date initialization skipped: {ex.Message}";
        }
    }

    private void ClearPayslipForm()
    {
        try
        {
            if (txtBasicSalary != null) txtBasicSalary.Text = "₹0";
            if (txtAdvanceSalary != null) txtAdvanceSalary.Text = "0";
            if (txtLoanDeduction != null) txtLoanDeduction.Text = "0";
            if (lblNetSalary != null) lblNetSalary.Text = "₹0";
            ClearPayslipPreview();
        }
        catch (Exception ex)
        {
            // Safely ignore initialization issues
            if (lblStatus != null)
                lblStatus.Text = $"Payslip form clear skipped: {ex.Message}";
        }
    }

    private void ClearPayslipPreview()
    {
        try
        {
            // Use FindName to locate Run elements for clearing
            var empName = this.FindName("PayslipEmployeeName") as Run;
            var empId = this.FindName("PayslipEmployeeId") as Run;
            var empPosition = this.FindName("PayslipPosition") as Run;
            var empPeriod = this.FindName("PayslipPeriod") as Run;
            var empGenDate = this.FindName("PayslipGeneratedDate") as Run;
            var empJoinDate = this.FindName("PayslipJoiningDate") as Run;

            var basicSalaryElement = this.FindName("PayslipBasicSalary") as TextBlock;
            var advanceSalaryElement = this.FindName("PayslipAdvanceSalary") as TextBlock;
            var loanDeductionElement = this.FindName("PayslipLoanDeduction") as TextBlock;
            var totalDeductionsElement = this.FindName("PayslipTotalDeductions") as TextBlock;
            var netSalaryElement = this.FindName("PayslipNetSalary") as TextBlock;

            // Clear Run elements
            if (empName != null) empName.Text = "";
            if (empId != null) empId.Text = "";
            if (empPosition != null) empPosition.Text = "";
            if (empPeriod != null) empPeriod.Text = "";
            if (empGenDate != null) empGenDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            if (empJoinDate != null) empJoinDate.Text = "";

            // Clear TextBlock elements
            if (basicSalaryElement != null) basicSalaryElement.Text = "₹0";
            if (advanceSalaryElement != null) advanceSalaryElement.Text = "₹0";
            if (loanDeductionElement != null) loanDeductionElement.Text = "₹0";
            if (totalDeductionsElement != null) totalDeductionsElement.Text = "₹0";
            if (netSalaryElement != null) netSalaryElement.Text = "₹0";

            System.Diagnostics.Debug.WriteLine("ClearPayslipPreview: All elements cleared successfully");
        }
        catch (Exception ex)
        {
            // Safely ignore initialization issues
            if (lblStatus != null)
                lblStatus.Text = $"Payslip preview clear skipped: {ex.Message}";
            System.Diagnostics.Debug.WriteLine($"ClearPayslipPreview Error: {ex.Message}");
        }
    }

    private void CmbEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            // Ensure all controls are available
            if (txtBasicSalary == null || lblStatus == null)
            {
                return; // Skip if controls not initialized yet
            }

            if (cmbEmployee?.SelectedItem != null)
            {
                // Use reflection to safely access anonymous object properties
                var selectedEmployee = cmbEmployee.SelectedItem;
                var employeeType = selectedEmployee.GetType();

                // Get employee properties using reflection for safer access
                var fullNameProp = employeeType.GetProperty("FullName");
                var salaryProp = employeeType.GetProperty("MonthlySalary");
                var employeeIdProp = employeeType.GetProperty("EmployeeId");
                var positionProp = employeeType.GetProperty("Position");

                var employeeName = fullNameProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";
                var salary = (decimal)(salaryProp?.GetValue(selectedEmployee) ?? 0m);
                var employeeId = employeeIdProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";
                var position = positionProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";

                // Update basic salary field
                txtBasicSalary.Text = $"₹{salary:N0}";

                // Force recalculation
                CalculateNetSalary();

                // Enhanced status confirmation with debugging
                if (lblStatus != null)
                    lblStatus.Text = $"✓ Selected: {employeeName} (ID: {employeeId}, Position: {position}, Salary: ₹{salary:N0})";

                // Also populate other fields for convenience
                if (txtAdvanceSalary != null && string.IsNullOrEmpty(txtAdvanceSalary.Text))
                    txtAdvanceSalary.Text = "0";
                if (txtLoanDeduction != null && string.IsNullOrEmpty(txtLoanDeduction.Text))
                    txtLoanDeduction.Text = "0";

                // Auto-generate preview if month/year are selected
                if (cmbMonth?.SelectedItem != null && cmbYear?.SelectedItem != null)
                {
                    GeneratePayslipPreview();
                }
            }
            else
            {
                // Clear fields when no employee selected
                txtBasicSalary.Text = "₹0";
                if (txtAdvanceSalary != null) txtAdvanceSalary.Text = "0";
                if (txtLoanDeduction != null) txtLoanDeduction.Text = "0";
                CalculateNetSalary();
                if (lblStatus != null) lblStatus.Text = "No employee selected";
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error loading employee details: {ex.Message}";
            if (lblStatus != null) lblStatus.Text = errorMsg;
            MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CalculateNetSalary()
    {
        try
        {
            // Null checks for all UI controls
            if (txtBasicSalary == null || txtAdvanceSalary == null || txtLoanDeduction == null || lblNetSalary == null)
                return;

            if (decimal.TryParse(txtBasicSalary.Text.Replace("₹", "").Replace(",", ""), out decimal basicSalary) &&
                decimal.TryParse(txtAdvanceSalary.Text, out decimal advance) &&
                decimal.TryParse(txtLoanDeduction.Text, out decimal loan))
            {
                var netSalary = basicSalary - advance - loan;
                lblNetSalary.Text = $"₹{netSalary:N0}";
            }
        }
        catch (Exception ex)
        {
            // Safely handle errors during calculation
            if (lblNetSalary != null)
                lblNetSalary.Text = "₹0";
        }
    }

    private void BtnGeneratePayslip_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbEmployee.SelectedItem == null)
            {
                MessageBox.Show("Please select an employee.", "No Employee Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbMonth.SelectedItem == null || cmbYear.SelectedItem == null)
            {
                MessageBox.Show("Please select month and year.", "Incomplete Date", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate the salary maths before producing an official payslip.
            decimal.TryParse(txtBasicSalary.Text.Replace("₹", "").Replace(",", ""), out var basic);
            decimal.TryParse(txtAdvanceSalary.Text, out var advanceAmt);
            decimal.TryParse(txtLoanDeduction.Text, out var loanAmt);

            if (advanceAmt < 0 || loanAmt < 0)
            {
                MessageBox.Show("Advance and loan deductions cannot be negative.", "Invalid Deductions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (advanceAmt + loanAmt > basic)
            {
                MessageBox.Show($"Total deductions (₹{advanceAmt + loanAmt:N0}) exceed the basic salary (₹{basic:N0}). " +
                                "Net salary cannot be negative on a payslip.", "Invalid Deductions", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            GeneratePayslipPreview();
            lblStatus.Text = "Payslip generated successfully";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating payslip: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GeneratePayslipPreview()
    {
        try
        {
            // Use reflection to safely access anonymous object properties
            var selectedEmployee = cmbEmployee.SelectedItem;
            var employeeType = selectedEmployee.GetType();

            // Get employee properties using reflection
            var fullNameProp = employeeType.GetProperty("FullName");
            var employeeIdProp = employeeType.GetProperty("EmployeeId");
            var positionProp = employeeType.GetProperty("Position");
            var joiningDateProp = employeeType.GetProperty("JoiningDate");

            var employeeName = fullNameProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";
            var employeeId = employeeIdProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";
            var position = positionProp?.GetValue(selectedEmployee)?.ToString() ?? "Unknown";
            var joiningDate = (DateTime)(joiningDateProp?.GetValue(selectedEmployee) ?? DateTime.Now);
            var selectedMonth = ((ComboBoxItem)cmbMonth.SelectedItem).Content.ToString();
            var selectedYear = cmbYear.SelectedItem.ToString();

            // Parse salary values
            decimal.TryParse(txtBasicSalary.Text.Replace("₹", "").Replace(",", ""), out decimal basicSalary);
            decimal.TryParse(txtAdvanceSalary.Text, out decimal advance);
            decimal.TryParse(txtLoanDeduction.Text, out decimal loan);

            var totalDeductions = advance + loan;
            var netSalary = basicSalary - totalDeductions;

            // Update payslip preview using FindName method
            try
            {
                // Use FindName to locate Run elements
                var empName = this.FindName("PayslipEmployeeName") as Run;
                var empId = this.FindName("PayslipEmployeeId") as Run;
                var empPosition = this.FindName("PayslipPosition") as Run;
                var empPeriod = this.FindName("PayslipPeriod") as Run;
                var empGenDate = this.FindName("PayslipGeneratedDate") as Run;
                var empJoinDate = this.FindName("PayslipJoiningDate") as Run;

                var basicSalaryElement = this.FindName("PayslipBasicSalary") as TextBlock;
                var advanceSalaryElement = this.FindName("PayslipAdvanceSalary") as TextBlock;
                var loanDeductionElement = this.FindName("PayslipLoanDeduction") as TextBlock;
                var totalDeductionsElement = this.FindName("PayslipTotalDeductions") as TextBlock;
                var netSalaryElement = this.FindName("PayslipNetSalary") as TextBlock;

                // Update Run elements
                if (empName != null)
                {
                    empName.Text = employeeName;
                    System.Diagnostics.Debug.WriteLine($"Set Employee Name via FindName: {employeeName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Employee Name Run element not found via FindName!");
                }

                if (empId != null)
                {
                    empId.Text = employeeId;
                    System.Diagnostics.Debug.WriteLine($"Set Employee ID via FindName: {employeeId}");
                }

                if (empPosition != null)
                {
                    empPosition.Text = position;
                    System.Diagnostics.Debug.WriteLine($"Set Position via FindName: {position}");
                }

                if (empPeriod != null) empPeriod.Text = $"{selectedMonth} {selectedYear}";
                if (empGenDate != null) empGenDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
                if (empJoinDate != null) empJoinDate.Text = joiningDate.ToString("dd/MM/yyyy");

                // Update TextBlock elements (salary breakdown)
                if (basicSalaryElement != null) basicSalaryElement.Text = $"₹{basicSalary:N0}";
                if (advanceSalaryElement != null) advanceSalaryElement.Text = advance > 0 ? $"₹{advance:N0}" : "₹0";
                if (loanDeductionElement != null) loanDeductionElement.Text = loan > 0 ? $"₹{loan:N0}" : "₹0";
                if (totalDeductionsElement != null) totalDeductionsElement.Text = $"₹{totalDeductions:N0}";
                if (netSalaryElement != null) netSalaryElement.Text = $"₹{netSalary:N0}";

                // Force UI update
                PayslipBorder.UpdateLayout();

                System.Diagnostics.Debug.WriteLine($"Payslip updated successfully for {employeeName}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating payslip elements: {ex.Message}");
                MessageBox.Show($"Debug: Error updating payslip elements: {ex.Message}", "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating payslip preview: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPrintPayslip_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Check if payslip has been generated using FindName
            var empName = this.FindName("PayslipEmployeeName") as Run;
            if (empName == null || string.IsNullOrEmpty(empName.Text))
            {
                MessageBox.Show("Please generate a payslip first.", "No Payslip", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                // Force layout update to ensure all content is rendered
                PayslipBorder.UpdateLayout();
                PayslipBorder.Measure(new Size(794, double.PositiveInfinity));
                PayslipBorder.Arrange(new Rect(0, 0, 794, PayslipBorder.DesiredSize.Height));

                // Create a visual for printing
                var visual = PayslipBorder;
                var transform = visual.LayoutTransform;

                // Configure for A4 paper size (8.27 × 11.69 inches at standard DPI)
                double pageWidth = printDialog.PrintableAreaWidth;
                double pageHeight = printDialog.PrintableAreaHeight;
                double visualWidth = PayslipBorder.ActualWidth > 0 ? PayslipBorder.ActualWidth : 794;
                double visualHeight = PayslipBorder.ActualHeight > 0 ? PayslipBorder.ActualHeight : PayslipBorder.DesiredSize.Height;

                // Calculate scale to fit A4 page
                var scaleX = pageWidth / visualWidth;
                var scaleY = pageHeight / visualHeight;
                var scale = Math.Min(scaleX, scaleY);

                // Apply scale transform
                visual.LayoutTransform = new ScaleTransform(scale, scale);

                // Print the visual
                printDialog.PrintVisual(visual, "Employee Payslip");

                // Restore original transform
                visual.LayoutTransform = transform;

                lblStatus.Text = "Payslip printed successfully";
                MessageBox.Show("Payslip printed successfully!", "Print Complete", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing payslip: {ex.Message}", "Print Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnClearPayslip_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbEmployee != null) cmbEmployee.SelectedItem = null;
            ClearPayslipForm();
            if (lblStatus != null) lblStatus.Text = "Payslip form cleared";
        }
        catch (Exception ex)
        {
            if (lblStatus != null)
                lblStatus.Text = $"Error clearing payslip: {ex.Message}";
        }
    }

    // Add event handlers for real-time calculation
    private void TxtBasicSalary_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            CalculateNetSalary();
        }
        catch (Exception ex)
        {
            // Safely ignore calculation errors during initialization
        }
    }

    private void TxtAdvanceSalary_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            CalculateNetSalary();
        }
        catch (Exception ex)
        {
            // Safely ignore calculation errors during initialization
        }
    }

    private void TxtLoanDeduction_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            CalculateNetSalary();
        }
        catch (Exception ex)
        {
            // Safely ignore calculation errors during initialization
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}