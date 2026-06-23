using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Printing;
using Microsoft.Win32;
using System.IO;
using System.ComponentModel;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.WPF.Controls;
using IEMS.WPF.Helpers;
using IEMS.Core.Interfaces;
using ClosedXML.Excel;

namespace IEMS.WPF;

public partial class StudentsManagementWindow : Window
{
    private readonly StudentService _studentService;
    private readonly ClassService _classService;
    private readonly TeacherService _teacherService;
    private readonly FeePaymentService _feePaymentService;
    private readonly FeeStructureService _feeStructureService;
    private readonly BulkPromotionService? _bulkPromotionService;
    private readonly AcademicYearService? _academicYearService;
    private List<StudentDto> _allStudents = new List<StudentDto>();
    private List<FeePaymentDto> _allFeePayments = new List<FeePaymentDto>();
    private List<ClassDto> _allClasses = new List<ClassDto>();
    private List<FeeStructureDto> _allFeeStructures = new List<FeeStructureDto>();

    public StudentsManagementWindow(StudentService studentService, ClassService classService, TeacherService teacherService, FeePaymentService feePaymentService, FeeStructureService feeStructureService, BulkPromotionService? bulkPromotionService = null, AcademicYearService? academicYearService = null)
    {
        InitializeComponent();
        _studentService = studentService;
        _classService = classService;
        _teacherService = teacherService;
        _feePaymentService = feePaymentService;
        _feeStructureService = feeStructureService;
        _bulkPromotionService = bulkPromotionService;
        _academicYearService = academicYearService;
        AsyncHelper.SafeFireAndForget(LoadStudentsAsync);
        AsyncHelper.SafeFireAndForget(LoadClassesAsync);
        AsyncHelper.SafeFireAndForget(LoadFeePaymentsAsync);
        AsyncHelper.SafeFireAndForget(LoadFeeStructuresAsync);
        AsyncHelper.SafeFireAndForget(LoadBonafideStudentsAsync);
        AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync);
        AsyncHelper.SafeFireAndForget(LoadPromotionDataAsync);
    }

    private async Task LoadStudentsAsync()
    {
        try
        {
            loadingOverlay.IsLoading = true;
            loadingOverlay.LoadingMessage = "Loading students...";
            lblStatus.Text = "Loading students...";

            var students = await _studentService.GetAllStudentsAsync();
            _allStudents = students.ToList();

            // Load outstanding fees for each student
            loadingOverlay.LoadingMessage = "Calculating outstanding fees...";
            await LoadOutstandingFeesAsync();

            // Apply class filter if one is selected
            ApplyStudentFilters();

            lblStatus.Text = $"Loaded {students.Count()} students";

            toastNotification.Message = $"Successfully loaded {students.Count()} students";
            toastNotification.ToastType = ToastType.Success;
            toastNotification.Show();

            // Refresh dashboard after loading students
            await LoadDashboardDataAsync();

            // Load leaving certificate students dropdown after students data is loaded
            await LoadLeavingCertificateStudentsAsync();
        }
        catch (Exception ex)
        {
            lblStatus.Text = "Error loading students";

            toastNotification.Message = $"Error loading students: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
        finally
        {
            loadingOverlay.IsLoading = false;
        }
    }

    private async Task LoadClassesAsync()
    {
        try
        {
            lblStatus.Text = "Loading classes...";
            var classes = await _classService.GetAllClassesAsync();
            _allClasses = classes.ToList();
            dgClasses.ItemsSource = _allClasses;
            lblStatus.Text = $"Loaded {classes.Count()} classes";

            // Populate class filter dropdown
            PopulateClassFilter();

            // Refresh dashboard after loading classes
            await LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading classes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error loading classes";
        }
    }

    private void BtnBack_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    // Student Management Events
    private void BtnAddStudent_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddEditStudentWindow(_studentService, _classService);
        if (addWindow.ShowDialog() == true)
        {
            var currentSearch = txtSearchStudents.Text;
            AsyncHelper.SafeFireAndForget(LoadStudentsAsync);
            AsyncHelper.SafeFireAndForget(LoadClassesAsync); // Refresh to update student counts in classes
            txtSearchStudents.Text = currentSearch; // Restore search after refresh
            FilterStudents();
            AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
        }
    }

    private void BtnEditStudent_Click(object sender, RoutedEventArgs e)
    {
        if (dgStudents.SelectedItem is StudentDto selectedStudent)
        {
            var editWindow = new AddEditStudentWindow(_studentService, _classService, selectedStudent);
            if (editWindow.ShowDialog() == true)
            {
                var currentSearch = txtSearchStudents.Text;
                AsyncHelper.SafeFireAndForget(LoadStudentsAsync);
                AsyncHelper.SafeFireAndForget(LoadClassesAsync); // Refresh to update student counts in classes
                txtSearchStudents.Text = currentSearch; // Restore search after refresh
                FilterStudents();
                AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
            }
        }
        else
        {
            MessageBox.Show("Please select a student to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnStudentIdCard_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgStudents.SelectedItems.OfType<StudentDto>().ToList();
        AsyncHelper.SafeFireAndForget(
            () => GenerateIdCardsForAsync(selected,
                "Select one or more students (Ctrl/Shift-click) to print ID cards.", suggestedName: null),
            "ID Card Error");
    }

    private void BtnDeleteStudent_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (dgStudents.SelectedItem is StudentDto selectedStudent)
            {
                var result = MessageBox.Show($"Are you sure you want to delete student {selectedStudent.FullName}?",
                                           "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        loadingOverlay.IsLoading = true;
                        loadingOverlay.LoadingMessage = "Deleting student...";

                        await _studentService.DeleteStudentAsync(selectedStudent.Id);
                        var currentSearch = txtSearchStudents.Text;
                        AsyncHelper.SafeFireAndForget(LoadStudentsAsync);
                        AsyncHelper.SafeFireAndForget(LoadClassesAsync); // Refresh to update student counts in classes
                        txtSearchStudents.Text = currentSearch; // Restore search after refresh

                        toastNotification.Message = $"Student {selectedStudent.FullName} deleted successfully";
                        toastNotification.ToastType = ToastType.Success;
                        toastNotification.Show();
                        FilterStudents();
                        AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
                        lblStatus.Text = "Student deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        toastNotification.Message = $"Error deleting student: {ex.Message}";
                        toastNotification.ToastType = ToastType.Error;
                        toastNotification.Show();
                        lblStatus.Text = "Error deleting student";
                    }
                    finally
                    {
                        loadingOverlay.IsLoading = false;
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a student to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }, "Student Delete Error");
    }

    private void BtnRefreshStudents_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadStudentsAsync);
        txtSearchStudents.Text = ""; // Clear search when refreshing
        AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
    }

    private void BtnExportToExcel_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Get students to export (filtered list if search is active)
            var studentsToExport = dgStudents.ItemsSource as System.Collections.IEnumerable;
            if (studentsToExport == null || !studentsToExport.Cast<StudentDto>().Any())
            {
                MessageBox.Show("No students to export.", "Export Failed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Show column picker dialog
            var columnPicker = new ColumnPickerDialog();
            columnPicker.Owner = this;
            if (columnPicker.ShowDialog() != true)
            {
                return; // User cancelled column selection
            }

            var selectedColumns = columnPicker.SelectedColumns;

            // Show save file dialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = $"Students_List_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.xlsx",
                Title = "Export Students to Excel"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                // Create Excel workbook
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Students");

                    // Add headers for selected columns only
                    int col = 1;
                    var columnMapping = new Dictionary<string, int>(); // Maps column name to Excel column number

                    if (selectedColumns["SerialNo"]) { worksheet.Cell(1, col).Value = "Serial No"; columnMapping["SerialNo"] = col++; }
                    if (selectedColumns["StudentNumber"]) { worksheet.Cell(1, col).Value = "Student Number"; columnMapping["StudentNumber"] = col++; }
                    if (selectedColumns["FirstName"]) { worksheet.Cell(1, col).Value = "First Name"; columnMapping["FirstName"] = col++; }
                    if (selectedColumns["FatherName"]) { worksheet.Cell(1, col).Value = "Father's Name"; columnMapping["FatherName"] = col++; }
                    if (selectedColumns["Surname"]) { worksheet.Cell(1, col).Value = "Surname"; columnMapping["Surname"] = col++; }
                    if (selectedColumns["MotherName"]) { worksheet.Cell(1, col).Value = "Mother's Name"; columnMapping["MotherName"] = col++; }
                    if (selectedColumns["DateOfBirth"]) { worksheet.Cell(1, col).Value = "Date of Birth"; columnMapping["DateOfBirth"] = col++; }
                    if (selectedColumns["Gender"]) { worksheet.Cell(1, col).Value = "Gender"; columnMapping["Gender"] = col++; }
                    if (selectedColumns["Standard"]) { worksheet.Cell(1, col).Value = "Standard"; columnMapping["Standard"] = col++; }
                    if (selectedColumns["Division"]) { worksheet.Cell(1, col).Value = "Division"; columnMapping["Division"] = col++; }
                    if (selectedColumns["AdmissionDate"]) { worksheet.Cell(1, col).Value = "Admission Date"; columnMapping["AdmissionDate"] = col++; }
                    if (selectedColumns["CasteCategory"]) { worksheet.Cell(1, col).Value = "Caste Category"; columnMapping["CasteCategory"] = col++; }
                    if (selectedColumns["Religion"]) { worksheet.Cell(1, col).Value = "Religion"; columnMapping["Religion"] = col++; }
                    if (selectedColumns["BPL"]) { worksheet.Cell(1, col).Value = "BPL"; columnMapping["BPL"] = col++; }
                    if (selectedColumns["SemiEnglish"]) { worksheet.Cell(1, col).Value = "Semi-English"; columnMapping["SemiEnglish"] = col++; }
                    if (selectedColumns["Address"]) { worksheet.Cell(1, col).Value = "Address"; columnMapping["Address"] = col++; }
                    if (selectedColumns["CityVillage"]) { worksheet.Cell(1, col).Value = "City/Village"; columnMapping["CityVillage"] = col++; }
                    if (selectedColumns["ParentMobile"]) { worksheet.Cell(1, col).Value = "Parent Mobile"; columnMapping["ParentMobile"] = col++; }
                    if (selectedColumns["AadhaarNumber"]) { worksheet.Cell(1, col).Value = "Aadhaar Number"; columnMapping["AadhaarNumber"] = col++; }
                    if (selectedColumns["OutstandingFees"]) { worksheet.Cell(1, col).Value = "Outstanding Fees"; columnMapping["OutstandingFees"] = col++; }

                    // Style header row
                    var headerRow = worksheet.Range(1, 1, 1, col - 1);
                    headerRow.Style.Font.Bold = true;
                    headerRow.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    // Add data for selected columns only
                    int row = 2;
                    foreach (StudentDto student in studentsToExport)
                    {
                        if (columnMapping.ContainsKey("SerialNo")) worksheet.Cell(row, columnMapping["SerialNo"]).Value = student.SerialNo;
                        if (columnMapping.ContainsKey("StudentNumber")) worksheet.Cell(row, columnMapping["StudentNumber"]).Value = student.StudentNumber;
                        if (columnMapping.ContainsKey("FirstName")) worksheet.Cell(row, columnMapping["FirstName"]).Value = student.FirstName;
                        if (columnMapping.ContainsKey("FatherName")) worksheet.Cell(row, columnMapping["FatherName"]).Value = student.FatherName;
                        if (columnMapping.ContainsKey("Surname")) worksheet.Cell(row, columnMapping["Surname"]).Value = student.Surname;
                        if (columnMapping.ContainsKey("MotherName")) worksheet.Cell(row, columnMapping["MotherName"]).Value = student.MotherName;
                        if (columnMapping.ContainsKey("DateOfBirth")) worksheet.Cell(row, columnMapping["DateOfBirth"]).Value = student.FormattedDateOfBirth;
                        if (columnMapping.ContainsKey("Gender")) worksheet.Cell(row, columnMapping["Gender"]).Value = student.Gender;
                        if (columnMapping.ContainsKey("Standard")) worksheet.Cell(row, columnMapping["Standard"]).Value = student.Standard;
                        if (columnMapping.ContainsKey("Division")) worksheet.Cell(row, columnMapping["Division"]).Value = student.ClassDivision;
                        if (columnMapping.ContainsKey("AdmissionDate")) worksheet.Cell(row, columnMapping["AdmissionDate"]).Value = student.FormattedAdmissionDate;
                        if (columnMapping.ContainsKey("CasteCategory")) worksheet.Cell(row, columnMapping["CasteCategory"]).Value = student.CasteCategory;
                        if (columnMapping.ContainsKey("Religion")) worksheet.Cell(row, columnMapping["Religion"]).Value = student.Religion;
                        if (columnMapping.ContainsKey("BPL")) worksheet.Cell(row, columnMapping["BPL"]).Value = student.IsBPL ? "Yes" : "No";
                        if (columnMapping.ContainsKey("SemiEnglish")) worksheet.Cell(row, columnMapping["SemiEnglish"]).Value = student.IsSemiEnglish ? "Yes" : "No";
                        if (columnMapping.ContainsKey("Address")) worksheet.Cell(row, columnMapping["Address"]).Value = student.Address;
                        if (columnMapping.ContainsKey("CityVillage")) worksheet.Cell(row, columnMapping["CityVillage"]).Value = student.CityVillage;
                        if (columnMapping.ContainsKey("ParentMobile")) worksheet.Cell(row, columnMapping["ParentMobile"]).Value = student.ParentMobileNumber;
                        if (columnMapping.ContainsKey("AadhaarNumber")) worksheet.Cell(row, columnMapping["AadhaarNumber"]).Value = student.AadhaarNumber ?? string.Empty;
                        if (columnMapping.ContainsKey("OutstandingFees")) worksheet.Cell(row, columnMapping["OutstandingFees"]).Value = student.FormattedOutstandingFees;
                        row++;
                    }

                    // Auto-fit columns
                    worksheet.Columns().AdjustToContents();

                    // Save the file
                    workbook.SaveAs(saveFileDialog.FileName);
                }

                toastNotification.Message = $"Successfully exported {studentsToExport.Cast<StudentDto>().Count()} students to Excel!";
                toastNotification.ToastType = ToastType.Success;
                toastNotification.Show();

                lblStatus.Text = "Students exported to Excel successfully";

                // Ask if user wants to open the file
                var result = MessageBox.Show(
                    "Export completed successfully. Do you want to open the file?",
                    "Export Complete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error exporting to Excel: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
            lblStatus.Text = "Error exporting students";
        }
    }

    // Class Management Events
    private void BtnAddClass_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddEditClassWindow(_classService, _teacherService);
        if (addWindow.ShowDialog() == true)
        {
            AsyncHelper.SafeFireAndForget(LoadClassesAsync);
            AsyncHelper.SafeFireAndForget(LoadPromotionDataAsync); // Refresh promotion dropdowns
            AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
        }
    }

    private void BtnEditClass_Click(object sender, RoutedEventArgs e)
    {
        if (dgClasses.SelectedItem is ClassDto selectedClass)
        {
            var editWindow = new AddEditClassWindow(_classService, _teacherService, selectedClass);
            if (editWindow.ShowDialog() == true)
            {
                AsyncHelper.SafeFireAndForget(LoadClassesAsync);
                AsyncHelper.SafeFireAndForget(LoadPromotionDataAsync); // Refresh promotion dropdowns
                AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
            }
        }
        else
        {
            MessageBox.Show("Please select a class to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void BtnDeleteClass_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (dgClasses.SelectedItem is ClassDto selectedClass)
            {
                var result = MessageBox.Show($"Are you sure you want to delete class {selectedClass.DisplayName}?",
                                           "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await _classService.DeleteClassAsync(selectedClass.Id);
                        await LoadClassesAsync();
                        await LoadDashboardDataAsync(); // Refresh dashboard
                        lblStatus.Text = "Class deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting class: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a class to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }

    private void BtnRefreshClasses_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadClassesAsync);
        AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync); // Refresh dashboard
    }

    private void BtnViewClassStudents_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (dgClasses.SelectedItem is ClassDto selectedClass)
            {
                try
                {
                    var students = await _studentService.GetAllStudentsAsync();
                    var classStudents = students.Where(s => s.ClassId == selectedClass.Id).ToList();

                    var message = classStudents.Any()
                        ? $"Students in {selectedClass.DisplayName}:\n\n" +
                          string.Join("\n", classStudents.Select(s => $"• {s.StudentNumber} - {s.FullName}"))
                        : $"No students enrolled in {selectedClass.DisplayName}";

                    MessageBox.Show(message, $"Students in {selectedClass.DisplayName}", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading students: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Please select a class to view students.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }

    // Search Functionality
    private void TxtSearchStudents_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        FilterStudents();
    }

    private void CmbStudentClassFilter_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        ApplyStudentFilters();
    }

    private void PopulateClassFilter()
    {
        if (cmbStudentClassFilter == null) return;

        var filterOptions = new List<ClassDto>();

        // Add "All Classes" option
        filterOptions.Add(new ClassDto { Id = 0, Name = "All Classes", Section = "" });

        // Add all actual classes
        if (_allClasses != null)
        {
            filterOptions.AddRange(_allClasses.OrderBy(c => GetClassOrder(c.Name)));
        }

        cmbStudentClassFilter.ItemsSource = null; // Force refresh
        cmbStudentClassFilter.ItemsSource = filterOptions;
        cmbStudentClassFilter.SelectedValue = 0; // Default to "All Classes"

        // Trigger initial filter application
        ApplyStudentFilters();
    }

    private void FilterStudents()
    {
        ApplyStudentFilters();
    }

    private void ApplyStudentFilters()
    {
        if (_allStudents == null || !_allStudents.Any())
            return;

        var searchText = txtSearchStudents?.Text?.Trim()?.ToLower() ?? "";
        var selectedClassId = cmbStudentClassFilter?.SelectedValue as int?;

        // Start with all students
        var filteredStudents = _allStudents.AsEnumerable();

        // Apply class filter if selected (and not "All Classes")
        if (selectedClassId.HasValue && selectedClassId.Value > 0)
        {
            filteredStudents = filteredStudents.Where(s => s.ClassId == selectedClassId.Value);
        }

        // Apply text search filter. Use a null-safe local matcher so an optional field that
        // happens to be null (e.g. Address/CityVillage/ParentMobile) can't crash the search.
        if (!string.IsNullOrEmpty(searchText))
        {
            bool Contains(string? field) => (field ?? string.Empty).ToLower().Contains(searchText);

            filteredStudents = filteredStudents.Where(student =>
                Contains(student.FirstName) ||
                Contains(student.Surname) ||
                Contains(student.FullName) ||
                Contains(student.FatherName) ||
                Contains(student.MotherName) ||
                Contains(student.StudentNumber) ||
                Contains(student.ParentMobileNumber) ||
                Contains(student.Standard) ||
                Contains(student.ClassDivision) ||
                Contains(student.Address) ||
                Contains(student.CityVillage)
            );
        }

        var resultList = filteredStudents.ToList();
        dgStudents.ItemsSource = null; // Force refresh
        dgStudents.ItemsSource = resultList;
        dgStudents.Items.Refresh(); // Explicitly refresh the view

        // Update status message
        var statusMessage = $"Showing {resultList.Count} students";
        if (selectedClassId.HasValue && selectedClassId.Value > 0)
        {
            var className = _allClasses?.FirstOrDefault(c => c.Id == selectedClassId.Value)?.DisplayName ?? "Unknown";
            statusMessage += $" from {className}";
        }
        if (!string.IsNullOrEmpty(searchText))
        {
            statusMessage += $" matching '{searchText}'";
        }
        lblStatus.Text = statusMessage;
    }

    // Fee Payment Management
    private async Task LoadFeePaymentsAsync()
    {
        try
        {
            lblStatus.Text = "Loading fee payments...";
            var feePayments = await _feePaymentService.GetAllFeePaymentsAsync();
            _allFeePayments = feePayments.ToList();
            dgFeePayments.ItemsSource = _allFeePayments;
            lblStatus.Text = $"Loaded {feePayments.Count()} fee payments";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading fee payments: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error loading fee payments";
        }
    }

    private async Task LoadFeeStructuresAsync()
    {
        try
        {
            lblStatus.Text = "Loading fee structures...";
            var feeStructures = await _feeStructureService.GetAllFeeStructuresAsync();
            _allFeeStructures = feeStructures.ToList();
            dgStudentFeeStructures.ItemsSource = _allFeeStructures;

            // Initialize the filters after loading the data
            await InitializeStudentFeeStructureFilters();

            lblStatus.Text = $"Loaded {feeStructures.Count()} fee structures";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading fee structures: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error loading fee structures";
        }
    }

    // Fee Payment Events
    private void BtnAddFeePayment_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Debug: Check if services are null
            if (_feePaymentService == null)
            {
                MessageBox.Show("Fee Payment Service is not initialized.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_feeStructureService == null)
            {
                MessageBox.Show("Fee Structure Service is not initialized.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (_studentService == null)
            {
                MessageBox.Show("Student Service is not initialized.", "Service Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var addFeePaymentWindow = new AddEditFeePaymentWindow(_feePaymentService, _feeStructureService, _studentService, _academicYearService);
            if (addFeePaymentWindow.ShowDialog() == true)
            {
                AsyncHelper.SafeFireAndForget(LoadFeePaymentsAsync); // Refresh fee payments list
                lblStatus.Text = "Fee payment recorded successfully";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening fee payment window: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnViewReceipt_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (dgFeePayments.SelectedItem is FeePaymentDto selectedPayment)
            {
                try
                {
                    lblStatus.Text = "Generating receipt...";
                    var receipt = await _feePaymentService.GenerateReceiptAsync(selectedPayment.Id);
                    var receiptWindow = new FeeReceiptWindow(receipt, _feePaymentService);
                    receiptWindow.ShowDialog();
                    lblStatus.Text = "Receipt displayed";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generating receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error generating receipt";
                }
            }
            else
            {
                MessageBox.Show("Please select a fee payment to view receipt.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }

    private void BtnPrintReceipt_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (dgFeePayments.SelectedItem is FeePaymentDto selectedPayment)
            {
                try
                {
                    lblStatus.Text = "Preparing receipt for printing...";
                    var receipt = await _feePaymentService.GenerateReceiptAsync(selectedPayment.Id);
                    var receiptWindow = new FeeReceiptWindow(receipt, _feePaymentService);
                    receiptWindow.Show();

                    // Automatically trigger print dialog
                    var printButton = receiptWindow.FindName("btnPrint") as System.Windows.Controls.Button;
                    printButton?.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));

                    lblStatus.Text = "Receipt ready for printing";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error printing receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error printing receipt";
                }
            }
            else
            {
                MessageBox.Show("Please select a fee payment to print receipt.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        });
    }

    private void BtnRefreshFeePayments_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadFeePaymentsAsync);
        txtSearchFeePayments.Text = ""; // Clear search when refreshing
    }

    // Fee Payment Search Functionality
    private void TxtSearchFeePayments_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        FilterFeePayments();
    }

    private void FilterFeePayments()
    {
        if (_allFeePayments == null || !_allFeePayments.Any())
            return;

        var searchText = txtSearchFeePayments.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(searchText))
        {
            // Show all fee payments if search is empty
            dgFeePayments.ItemsSource = _allFeePayments;
            lblStatus.Text = $"Showing all {_allFeePayments.Count} fee payments";
            return;
        }

        // Filter fee payments based on multiple criteria
        var filteredPayments = _allFeePayments.Where(payment =>
            payment.ReceiptNumber.ToLower().Contains(searchText) ||
            payment.StudentName.ToLower().Contains(searchText) ||
            payment.FeeTypeDisplay.ToLower().Contains(searchText) ||
            payment.PaymentMethodDisplay.ToLower().Contains(searchText) ||
            payment.AcademicYear.ToLower().Contains(searchText)
        ).ToList();

        dgFeePayments.ItemsSource = filteredPayments;
        lblStatus.Text = $"Found {filteredPayments.Count} fee payments matching '{txtSearchFeePayments.Text}'";
    }

    // Bonafide Certificate Event Handlers
    private void BtnGenerateBonafide_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgBonafideStudents.SelectedItem is StudentDto selectedStudentDto)
                {
                    // Get the full student entity from the service
                    var student = await _studentService.GetStudentEntityByIdAsync(selectedStudentDto.Id);
                    if (student != null)
                    {
                        var bonafideWindow = new BonafideCertificateWindow(student);
                        bonafideWindow.ShowDialog();
                    }
                    else
                    {
                        toastNotification.Message = "Student not found!";
                        toastNotification.ToastType = ToastType.Error;
                        toastNotification.Show();
                    }
                }
                else
                {
                    toastNotification.Message = "Please select a student to generate Bonafide certificate.";
                    toastNotification.ToastType = ToastType.Warning;
                    toastNotification.Show();
                }
            }
            catch (Exception ex)
            {
                toastNotification.Message = $"Error generating certificate: {ex.Message}";
                toastNotification.ToastType = ToastType.Error;
                toastNotification.Show();
            }
        });
    }

    private void BtnGenerateIdCard_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgBonafideStudents.SelectedItems.OfType<StudentDto>().ToList();
        AsyncHelper.SafeFireAndForget(
            () => GenerateIdCardsForAsync(selected,
                "Select one or more students (Ctrl/Shift-click) to print ID cards.", suggestedName: null),
            "ID Card Error");
    }

    private void BtnClassIdCards_Click(object sender, RoutedEventArgs e)
    {
        var className = cmbIdCardClass.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(className) || className == "(Select class)")
        {
            toastNotification.Message = "Pick a class to print all its ID cards.";
            toastNotification.ToastType = ToastType.Warning;
            toastNotification.Show();
            return;
        }

        var classStudents = _allStudents.Where(s => s.ClassWithDivision == className).ToList();
        var safeName = className.Replace(" ", "_").Replace("(", "").Replace(")", "");
        AsyncHelper.SafeFireAndForget(
            () => GenerateIdCardsForAsync(classStudents,
                $"No students found in {className}.", suggestedName: $"IDCards_Class_{safeName}"),
            "Class ID Card Error");
    }

    /// <summary>Builds and exports an A4 ID-card sheet for the given students (loads photos by id).</summary>
    private async Task GenerateIdCardsForAsync(IReadOnlyList<StudentDto> students, string warnIfEmpty, string? suggestedName,
        IEMS.WPF.Pdf.IdCardSize? size = null, bool includeBack = false)
    {
        try
        {
            if (students.Count == 0)
            {
                toastNotification.Message = warnIfEmpty;
                toastNotification.ToastType = ToastType.Warning;
                toastNotification.Show();
                return;
            }

            // Load the full entities (with photo BLOB) on this background thread.
            var raw = new List<IEMS.WPF.Pdf.IdCardData>();
            foreach (var dto in students.OrderBy(s => s.SerialNo))
            {
                var student = await _studentService.GetStudentEntityByIdAsync(dto.Id);
                if (student == null) continue;

                raw.Add(new IEMS.WPF.Pdf.IdCardData
                {
                    StudentName = student.FullName,
                    FatherName = student.FatherName,
                    ClassName = student.ClassWithDivision,
                    StudentNumber = string.IsNullOrWhiteSpace(student.StudentNumber) ? "-" : student.StudentNumber,
                    DateOfBirth = student.DateOfBirth.ToString("dd MMM yyyy"),
                    BloodGroup = student.BloodGroup ?? string.Empty,
                    ParentMobile = student.ParentMobileNumber,
                    Address = string.IsNullOrWhiteSpace(student.Address) ? student.CityVillage : student.Address,
                    Photo = student.Photo
                });
            }

            if (raw.Count == 0)
            {
                toastNotification.Message = "Student(s) not found.";
                toastNotification.ToastType = ToastType.Error;
                toastNotification.Show();
                return;
            }

            var theSize = size ?? IEMS.WPF.Pdf.IdCardSize.StandardCr80;
            var suggested = suggestedName
                ?? (raw.Count == 1
                    ? $"IDCard_{raw[0].StudentName.Replace(' ', '_')}"
                    : $"IDCards_{raw.Count}_students");

            // All image rendering (WPF) and PDF generation must happen on the UI thread.
            Dispatcher.Invoke(() =>
            {
                var frontBg = IEMS.WPF.Pdf.CardArt.RenderFront(theSize.WidthMm, theSize.HeightMm);
                var backBg = includeBack ? IEMS.WPF.Pdf.CardArt.RenderBack(theSize.WidthMm, theSize.HeightMm) : null;
                var logo = BonafideCertificateWindow.LoadSchoolLogoBytes();
                var school = BuildSchoolInfo();

                var cards = new List<IEMS.WPF.Pdf.IdCardData>();
                foreach (var r in raw)
                {
                    byte[]? norm = null, rounded = null;
                    try
                    {
                        if (r.Photo != null && r.Photo.Length > 0)
                        {
                            norm = IEMS.WPF.Helpers.PhotoHelper.NormalizeForCard(r.Photo);
                            rounded = IEMS.WPF.Pdf.CardArt.RoundPhoto(norm);
                        }
                    }
                    catch { /* a bad photo shouldn't block the card */ }

                    var barcode = IEMS.WPF.Pdf.Barcode128.RenderPng(r.StudentNumber, 600, 120);
                    cards.Add(r with { Photo = norm, PhotoRounded = rounded, Barcode = barcode });
                }

                var document = new IEMS.WPF.Pdf.StudentIdCardDocument(cards, school, logo, frontBg, backBg, includeBack, theSize);
                IEMS.WPF.Pdf.PdfExporter.SaveAndOpen(document, suggested);
            });
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error generating ID card(s): {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    /// <summary>School details for the ID card. (Matches the seeded School.* settings.)</summary>
    private static IEMS.WPF.Pdf.SchoolInfo BuildSchoolInfo() => new()
    {
        Name = "Inspire English Medium School, Mardi",
        Tagline = "Excellence in Education • Inspiring Future Leaders",
        Address = "Tah. Maregaon, Dist. Yavatmal (MH) – 445303",
        Phone = "8483949981",
        Email = "inspire.mardi@gmail.com",
        Website = "",
        Terms = new[]
        {
            "This ID card is the property of the school.",
            "This card is non-transferable.",
            "If found, please return it to the school office.",
            "The holder must carry this card in school every day."
        }
    };

    // ---------- ID Cards tab ----------

    // Full DTO (incl. photo + blood group) of the student currently shown in the photo panel.
    private StudentDto? _idCardSelected;
    private byte[]? _idCardPhotoBytes;

    private void RefreshIdCardTab()
    {
        // Card-size options (CR80 default) + a "Custom size…" entry. Populate once.
        if (cmbIdCardSize.Items.Count == 0)
        {
            foreach (var sz in IEMS.WPF.Pdf.IdCardSize.Presets)
                cmbIdCardSize.Items.Add(sz);
            cmbIdCardSize.Items.Add(CustomSizeLabel);
            cmbIdCardSize.SelectedIndex = 0; // Standard CR80
            _lastIdCardSize = IEMS.WPF.Pdf.IdCardSize.StandardCr80;
        }

        // Class filter
        var selectedClass = cmbIdCardTabClass.SelectedItem as string;
        cmbIdCardTabClass.Items.Clear();
        cmbIdCardTabClass.Items.Add("(All classes)");
        foreach (var c in _allStudents.Select(s => s.ClassWithDivision)
                     .Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c))
            cmbIdCardTabClass.Items.Add(c);
        cmbIdCardTabClass.SelectedItem = selectedClass != null && cmbIdCardTabClass.Items.Contains(selectedClass)
            ? selectedClass : "(All classes)";

        FilterIdCardStudents();
    }

    private void FilterIdCardStudents()
    {
        if (dgIdCardStudents == null) return;

        IEnumerable<StudentDto> q = _allStudents;

        var cls = cmbIdCardTabClass.SelectedItem as string;
        if (!string.IsNullOrEmpty(cls) && cls != "(All classes)")
            q = q.Where(s => s.ClassWithDivision == cls);

        var search = txtSearchIdCard?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            q = q.Where(x => x.FullName.ToLowerInvariant().Contains(s)
                          || (x.StudentNumber ?? "").ToLowerInvariant().Contains(s)
                          || (x.ClassWithDivision ?? "").ToLowerInvariant().Contains(s));
        }

        dgIdCardStudents.ItemsSource = q.OrderBy(s => s.SerialNo).ToList();
    }

    private void TxtSearchIdCard_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => FilterIdCardStudents();

    private void CmbIdCardTabClass_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => FilterIdCardStudents();

    private void DgIdCardStudents_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        // The photo panel edits a single student; use the most-recently focused row.
        var dto = dgIdCardStudents.SelectedItem as StudentDto;
        if (dto == null)
        {
            _idCardSelected = null;
            _idCardPhotoBytes = null;
            lblIdCardStudentName.Text = "Select a student";
            lblIdCardStudentClass.Text = string.Empty;
            imgIdCardPhoto.Source = null;
            btnIdCardChoosePhoto.IsEnabled = false;
            btnIdCardScanPhoto.IsEnabled = false;
            btnIdCardRemovePhoto.IsEnabled = false;
            return;
        }

        AsyncHelper.SafeFireAndForget(async () =>
        {
            // Load the full record so saving the photo never wipes other fields (e.g. blood group).
            var full = await _studentService.GetStudentByIdAsync(dto.Id);
            if (full == null) return;
            _idCardSelected = full;
            _idCardPhotoBytes = full.Photo;
            lblIdCardStudentName.Text = full.FullName;
            lblIdCardStudentClass.Text = $"Class {full.ClassWithDivision}   •   Roll No {(string.IsNullOrWhiteSpace(full.StudentNumber) ? "-" : full.StudentNumber)}";
            imgIdCardPhoto.Source = PhotoHelper.Decode(full.Photo);
            btnIdCardChoosePhoto.IsEnabled = true;
            btnIdCardScanPhoto.IsEnabled = true;
            btnIdCardRemovePhoto.IsEnabled = full.Photo != null && full.Photo.Length > 0;
        }, "Load Student Error");
    }

    private void BtnIdCardChoosePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idCardSelected == null) return;
        try
        {
            var bytes = PhotoHelper.Pick();
            if (bytes == null) return; // cancelled
            // Crop/resize to the card's passport aspect so it fills the box (no grey margins).
            SaveIdCardPhoto(PhotoHelper.NormalizeForCard(bytes));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Invalid Image", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnIdCardScanPhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idCardSelected == null) return;
        try
        {
            var bytes = PhotoHelper.ScanFromScanner();
            if (bytes == null) return; // user cancelled the scan dialog
            // Auto-crop the scanned page to passport size for the card.
            SaveIdCardPhoto(PhotoHelper.NormalizeForCard(bytes));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Scanner", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private const string CustomSizeLabel = "Custom size…";
    private IEMS.WPF.Pdf.IdCardSize _lastIdCardSize = IEMS.WPF.Pdf.IdCardSize.StandardCr80;
    private IEMS.WPF.Pdf.IdCardSize? _customIdCardSize;
    private bool _suppressSizeChange;

    private IEMS.WPF.Pdf.IdCardSize GetSelectedIdCardSize()
        => cmbIdCardSize.SelectedItem as IEMS.WPF.Pdf.IdCardSize ?? IEMS.WPF.Pdf.IdCardSize.StandardCr80;

    private void CmbIdCardSize_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (_suppressSizeChange) return;

        if (cmbIdCardSize.SelectedItem is IEMS.WPF.Pdf.IdCardSize sz)
        {
            _lastIdCardSize = sz;
            return;
        }

        // The "Custom size…" entry: ask for dimensions, then add the result as a selectable item.
        if (cmbIdCardSize.SelectedItem as string == CustomSizeLabel)
        {
            var dlg = new CustomCardSizeWindow(_lastIdCardSize.WidthMm, _lastIdCardSize.HeightMm) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                var custom = new IEMS.WPF.Pdf.IdCardSize(
                    $"Custom — {dlg.WidthMm:0.#} × {dlg.HeightMm:0.#} mm", (float)dlg.WidthMm, (float)dlg.HeightMm);

                _suppressSizeChange = true;
                if (_customIdCardSize != null) cmbIdCardSize.Items.Remove(_customIdCardSize);
                _customIdCardSize = custom;
                cmbIdCardSize.Items.Insert(cmbIdCardSize.Items.IndexOf(CustomSizeLabel), custom);
                cmbIdCardSize.SelectedItem = custom;
                _lastIdCardSize = custom;
                _suppressSizeChange = false;
            }
            else
            {
                // Cancelled: revert to the previous real size.
                _suppressSizeChange = true;
                cmbIdCardSize.SelectedItem = _lastIdCardSize;
                _suppressSizeChange = false;
            }
        }
    }

    private void BtnIdCardRemovePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idCardSelected == null || _idCardPhotoBytes == null) return;
        SaveIdCardPhoto(null);
    }

    private void SaveIdCardPhoto(byte[]? bytes)
    {
        var target = _idCardSelected;
        if (target == null) return;

        AsyncHelper.SafeFireAndForget(async () =>
        {
            target.Photo = bytes;
            await _studentService.UpdateStudentAsync(target);
            _idCardPhotoBytes = bytes;
            imgIdCardPhoto.Source = PhotoHelper.Decode(bytes);
            btnIdCardRemovePhoto.IsEnabled = bytes != null && bytes.Length > 0;

            toastNotification.Message = bytes == null
                ? $"Photo removed for {target.FullName}."
                : $"Photo saved for {target.FullName}.";
            toastNotification.ToastType = ToastType.Success;
            toastNotification.Show();
        }, "Save Photo Error");
    }

    private void BtnIdCardGenerateSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgIdCardStudents.SelectedItems.OfType<StudentDto>().ToList();
        var size = GetSelectedIdCardSize();
        bool includeBack = chkIncludeBack.IsChecked == true;
        AsyncHelper.SafeFireAndForget(
            () => GenerateIdCardsForAsync(selected,
                "Select one or more students (Ctrl/Shift-click) to print ID cards.", suggestedName: null, size, includeBack),
            "ID Card Error");
    }

    private void BtnIdCardGenerateClass_Click(object sender, RoutedEventArgs e)
    {
        var className = cmbIdCardTabClass.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(className) || className == "(All classes)")
        {
            toastNotification.Message = "Pick a specific class above to print the whole class.";
            toastNotification.ToastType = ToastType.Warning;
            toastNotification.Show();
            return;
        }

        var classStudents = _allStudents.Where(s => s.ClassWithDivision == className).ToList();
        var safeName = className.Replace(" ", "_").Replace("(", "").Replace(")", "");
        var size = GetSelectedIdCardSize();
        bool includeBack = chkIncludeBack.IsChecked == true;
        AsyncHelper.SafeFireAndForget(
            () => GenerateIdCardsForAsync(classStudents,
                $"No students found in {className}.", suggestedName: $"IDCards_Class_{safeName}", size, includeBack),
            "Class ID Card Error");
    }

    private void BtnRefreshBonafide_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadBonafideStudentsAsync);
    }

    private async Task LoadBonafideStudentsAsync()
    {
        try
        {
            // Use the same student data as the main students tab
            dgBonafideStudents.ItemsSource = _allStudents;
            lblStatus.Text = $"Loaded {_allStudents.Count} students for Bonafide certificates";

            // Populate the class picker for "print all ID cards in a class".
            var selectedClass = cmbIdCardClass.SelectedItem as string;
            cmbIdCardClass.Items.Clear();
            cmbIdCardClass.Items.Add("(Select class)");
            foreach (var c in _allStudents.Select(s => s.ClassWithDivision)
                         .Where(c => !string.IsNullOrWhiteSpace(c))
                         .Distinct().OrderBy(c => c))
                cmbIdCardClass.Items.Add(c);
            cmbIdCardClass.SelectedItem = selectedClass != null && cmbIdCardClass.Items.Contains(selectedClass)
                ? selectedClass : "(Select class)";

            RefreshIdCardTab();
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error loading students: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    // Bonafide Search Functionality
    private void TxtSearchBonafide_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        FilterBonafideStudents();
    }

    private void FilterBonafideStudents()
    {
        if (_allStudents == null || !_allStudents.Any())
            return;

        var searchText = txtSearchBonafide.Text.Trim().ToLower();

        if (string.IsNullOrEmpty(searchText))
        {
            // Show all students if search is empty
            dgBonafideStudents.ItemsSource = _allStudents;
            lblStatus.Text = $"Showing all {_allStudents.Count} students";
            return;
        }

        // Filter students based on multiple criteria
        var filteredStudents = _allStudents.Where(student =>
            student.FirstName.ToLower().Contains(searchText) ||
            student.Surname.ToLower().Contains(searchText) ||
            student.FatherName.ToLower().Contains(searchText) ||
            student.MotherName.ToLower().Contains(searchText) ||
            student.StudentNumber.ToLower().Contains(searchText) ||
            student.Standard.ToLower().Contains(searchText) ||
            student.ClassDivision.ToLower().Contains(searchText)
        ).ToList();

        dgBonafideStudents.ItemsSource = filteredStudents;
        lblStatus.Text = $"Found {filteredStudents.Count} students matching '{txtSearchBonafide.Text}'";
    }

    // Dashboard Data Loading and Statistics Methods
    private async Task LoadDashboardDataAsync()
    {
        if (_allStudents == null || !_allStudents.Any() || _allClasses == null || !_allClasses.Any())
            return;

        // Snapshot the in-memory collections so the background thread reads stable lists.
        var students = _allStudents.ToList();
        var classes = _allClasses.ToList();

        try
        {
            // Compute all statistics OFF the UI thread so the dashboard stays responsive even
            // for large schools (thousands of students). Only the final assignment to controls
            // runs on the UI thread — the awaited continuation resumes on the captured UI context.
            var stats = await Task.Run(() => ComputeDashboardStats(students, classes));

            lblTotalStudents.Text = stats.Total.ToString();
            lblMaleStudents.Text = stats.Male.ToString();
            lblFemaleStudents.Text = stats.Female.ToString();
            lblTotalClasses.Text = stats.Classes.ToString();
            lblBPLStudents.Text = stats.Bpl.ToString();
            lblSemiEnglishStudents.Text = stats.SemiEnglish.ToString();

            dgClassStats.ItemsSource = stats.ClassStats;
            dgReligionStats.ItemsSource = stats.ReligionStats;
            dgCasteStats.ItemsSource = stats.CasteStats;
            dgCityVillageStats.ItemsSource = stats.CityStats;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Dashboard Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Pure, UI-free aggregation so it can run on a background thread. Returns plain data;
    // the caller assigns it to controls on the UI thread. Gender comparisons are null-safe
    // (string.Equals handles a null left operand instead of throwing).
    private static DashboardStats ComputeDashboardStats(List<StudentDto> students, List<ClassDto> classes)
    {
        bool IsMale(StudentDto s) => string.Equals(s.Gender, "Male", StringComparison.OrdinalIgnoreCase);
        bool IsFemale(StudentDto s) => string.Equals(s.Gender, "Female", StringComparison.OrdinalIgnoreCase);

        var classStats = classes
            .OrderBy(c => c.Name)
            .Select(c =>
            {
                var inClass = students.Where(s => s.ClassId == c.Id).ToList();
                return new
                {
                    ClassName = c.DisplayName,
                    TotalStudents = inClass.Count,
                    MaleStudents = inClass.Count(IsMale),
                    FemaleStudents = inClass.Count(IsFemale)
                };
            })
            .ToList();

        var religionStats = students
            .GroupBy(s => s.Religion)
            .Select(g => new
            {
                Religion = string.IsNullOrEmpty(g.Key) ? "Not Specified" : g.Key,
                Count = g.Count(),
                MaleCount = g.Count(IsMale),
                FemaleCount = g.Count(IsFemale)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var casteStats = students
            .GroupBy(s => s.CasteCategory)
            .Select(g => new
            {
                Category = string.IsNullOrEmpty(g.Key) ? "Not Specified" : g.Key,
                Count = g.Count(),
                MaleCount = g.Count(IsMale),
                FemaleCount = g.Count(IsFemale)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var cityStats = students
            .GroupBy(s => s.CityVillage)
            .Select(g => new
            {
                CityVillage = string.IsNullOrEmpty(g.Key) ? "Not Specified" : g.Key,
                Count = g.Count(),
                MaleCount = g.Count(IsMale),
                FemaleCount = g.Count(IsFemale)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new DashboardStats
        {
            Total = students.Count,
            Male = students.Count(IsMale),
            Female = students.Count(IsFemale),
            Classes = classes.Count,
            Bpl = students.Count(s => s.IsBPL),
            SemiEnglish = students.Count(s => s.IsSemiEnglish),
            ClassStats = classStats,
            ReligionStats = religionStats,
            CasteStats = casteStats,
            CityStats = cityStats
        };
    }

    private sealed class DashboardStats
    {
        public int Total { get; set; }
        public int Male { get; set; }
        public int Female { get; set; }
        public int Classes { get; set; }
        public int Bpl { get; set; }
        public int SemiEnglish { get; set; }
        public System.Collections.IEnumerable ClassStats { get; set; } = System.Array.Empty<object>();
        public System.Collections.IEnumerable ReligionStats { get; set; } = System.Array.Empty<object>();
        public System.Collections.IEnumerable CasteStats { get; set; } = System.Array.Empty<object>();
        public System.Collections.IEnumerable CityStats { get; set; } = System.Array.Empty<object>();
    }

    // Leaving Certificate Event Handlers
    private async Task LoadLeavingCertificateStudentsAsync()
    {
        try
        {
            // Use the same student data as the main students tab
            if (cmbLeavingCertStudent != null)
                cmbLeavingCertStudent.ItemsSource = _allStudents;

            if (lblStatus != null)
                lblStatus.Text = $"Loaded {_allStudents.Count} students for Leaving certificates";

            // Populate the reason dropdown
            var leavingReasons = new List<string>
            {
                "Completion of course",
                "Transfer to another school",
                "Family relocation",
                "Financial reasons",
                "Health reasons",
                "Migration",
                "Other"
            };
            if (cmbLeavingReason != null)
                cmbLeavingReason.ItemsSource = leavingReasons;

            // Populate character/conduct dropdown
            var characterOptions = new List<string>
            {
                "Excellent",
                "Very Good",
                "Good",
                "Satisfactory"
            };
            if (cmbCharacterConduct != null)
            {
                cmbCharacterConduct.ItemsSource = characterOptions;
                cmbCharacterConduct.SelectedIndex = 0; // Default to "Excellent"
            }

            // Populate progress in studies dropdown
            if (cmbProgressInStudies != null)
            {
                cmbProgressInStudies.ItemsSource = characterOptions; // Same options as character conduct
                cmbProgressInStudies.SelectedIndex = 0; // Default to "Excellent"
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error loading students: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void CmbLeavingCertStudent_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            if (cmbLeavingCertStudent.SelectedItem is StudentDto selectedStudent)
            {
                try
                {
                    // Show the student details section
                    gridLeavingCertDetails.Visibility = System.Windows.Visibility.Visible;

                    // Populate the left panel student details
                    if (lblLeavingStudentName != null) lblLeavingStudentName.Text = selectedStudent.FullName;
                    if (lblLeavingMotherName != null) lblLeavingMotherName.Text = selectedStudent.MotherName;
                    if (lblLeavingFatherName != null) lblLeavingFatherName.Text = selectedStudent.FatherName;
                    if (lblLeavingAdmissionNo != null) lblLeavingAdmissionNo.Text = selectedStudent.StudentNumber;
                    if (lblLeavingStudentId != null) lblLeavingStudentId.Text = selectedStudent.Id.ToString();
                    if (lblLeavingClass != null) lblLeavingClass.Text = $"{selectedStudent.Standard} ({selectedStudent.ClassDivision})";
                    if (lblLeavingDOB != null) lblLeavingDOB.Text = selectedStudent.DateOfBirth.ToString("dd/MM/yyyy");
                    if (lblLeavingAdmissionDate != null) lblLeavingAdmissionDate.Text = selectedStudent.AdmissionDate.ToString("dd/MM/yyyy");
                    if (lblLeavingReligion != null) lblLeavingReligion.Text = selectedStudent.Religion;
                    if (lblLeavingCaste != null) lblLeavingCaste.Text = selectedStudent.CasteCategory;
                    if (lblLeavingMotherTongue != null) lblLeavingMotherTongue.Text = "Marathi"; // Default or could be added to StudentDto

                    // Set default value for last class
                    if (txtLastClass != null) txtLastClass.Text = selectedStudent.Standard;

                    // Populate Certificate Details fields with student data (auto-fill but allow override)
                    if (txtBirthPlace != null) txtBirthPlace.Text = selectedStudent.CityVillage ?? "";
                    if (txtSerialNoOverride != null) txtSerialNoOverride.Text = selectedStudent.SerialNo.ToString();
                    if (txtStudentId != null) txtStudentId.Text = selectedStudent.Id.ToString();
                    if (txtGeneralRegisterNumber != null) txtGeneralRegisterNumber.Text = selectedStudent.StudentNumber;
                    if (txtPhoneNumber != null) txtPhoneNumber.Text = selectedStudent.ParentMobileNumber;

                    // Auto-populate Place of Birth fields from student data
                    // For now, we'll use defaults since we don't have specific fields in the database
                    // Users can override these values manually
                    if (txtTaluka != null) txtTaluka.Text = ""; // Empty by default, user can fill
                    if (txtDistrict != null) txtDistrict.Text = ""; // Empty by default, user can fill
                    if (txtBirthState != null) txtBirthState.Text = "Maharashtra"; // Default state

                    // Auto-populate additional fields
                    if (txtAadhaarNumber != null) txtAadhaarNumber.Text = selectedStudent.AadhaarNumber ?? ""; // Auto-populate from student data
                    if (txtEmail != null) txtEmail.Text = ""; // Empty by default, user can fill

                    // Set default Academic Session based on current date
                    if (txtAcademicSession != null)
                    {
                        var currentYear = DateTime.Now.Year;
                        var nextYear = currentYear + 1;
                        var defaultAcademicSession = $"{currentYear}-{nextYear.ToString().Substring(2)}";
                        txtAcademicSession.Text = defaultAcademicSession;
                    }
                    // txtSubcaste, txtAadhaarNumber, txtEmail remain empty for manual entry
                    // txtState already defaults to "Maharashtra"

                    // Populate the certificate preview with student data
                    PopulateLeavingCertificatePreview(selectedStudent);
                }
                catch (Exception labelEx)
                {
                    toastNotification.Message = $"Error populating student details: {labelEx.Message}";
                    toastNotification.ToastType = ToastType.Error;
                    toastNotification.Show();
                }
            }
            else
            {
                // Hide the student details section if no student is selected
                gridLeavingCertDetails.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error selecting student: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void PopulateLeavingCertificatePreview(StudentDto student)
    {
        try
        {
            // Find and populate the TextBlock elements in the certificate (not Run elements)
            if (FindName("CertStudentName") is TextBlock certStudentName)
                certStudentName.Text = student.FullName.ToUpper();

            if (FindName("CertMotherName") is TextBlock certMotherName)
                certMotherName.Text = student.MotherName.ToUpper();

            if (FindName("CertDateOfBirth") is TextBlock certDateOfBirth)
                certDateOfBirth.Text = student.DateOfBirth.ToString("dd/MM/yyyy");

            if (FindName("CertDateOfBirthWords") is TextBlock certDateOfBirthWords)
                certDateOfBirthWords.Text = ConvertDateToWords(student.DateOfBirth);

            if (FindName("CertDateOfAdmission") is TextBlock certDateOfAdmission)
                certDateOfAdmission.Text = student.AdmissionDate.ToString("dd/MM/yyyy");

            if (FindName("CertAdmissionClass") is TextBlock certAdmissionClass)
                certAdmissionClass.Text = student.Standard;

            if (FindName("CertStandard") is TextBlock certStandard)
            {
                // Enhanced Grade field with default academic session format (both numeric and words)
                var currentYear = DateTime.Now.Year;
                var nextYear = currentYear + 1;
                var academicSession = $"{currentYear}–{nextYear.ToString().Substring(2)}";
                var standardWords = ConvertStandardToWords(student.Standard);

                // Create both numeric and words format
                var numericFormat = $"{student.Standard}th — Academic Session {academicSession}";
                var wordsFormat = $"Class {standardWords} — Academic Session {ConvertAcademicSessionToWords(academicSession)}";

                // Combine both formats
                var combinedText = $"{numericFormat}, {wordsFormat}";
                certStandard.Text = combinedText;
            }

            if (FindName("CertReligion") is TextBlock certReligion)
                certReligion.Text = student.Religion;

            if (FindName("CertCaste") is TextBlock certCaste)
                certCaste.Text = student.CasteCategory;

            if (FindName("CertSubcaste") is TextBlock certSubcaste)
                certSubcaste.Text = ""; // Not available in current StudentDto

            // Mother Tongue will be populated when certificate is generated based on dropdown selection

            if (FindName("CertGeneralRegisterNumber") is TextBlock certGeneralRegisterNumber)
                certGeneralRegisterNumber.Text = student.StudentNumber;

            if (FindName("CertAadhaarNumber") is TextBlock certAadhaarNumber)
                certAadhaarNumber.Text = student.AadhaarNumber ?? "";

            // Set default leaving date to today
            dpLeavingDate.SelectedDate = DateTime.Now;
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error populating certificate: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void LeavingCertField_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        // Update the preview when Student ID or Aadhaar number is changed manually
        try
        {
            if (FindName("CertStudentId") is TextBlock certStudentId && txtStudentId != null)
                certStudentId.Text = txtStudentId.Text;

            if (FindName("CertAadhaarNumber") is TextBlock certAadhaarNumber && txtAadhaarNumber != null)
                certAadhaarNumber.Text = txtAadhaarNumber.Text;
        }
        catch
        {
            // Silently ignore errors during preview update
        }
    }

    private string ConvertDateToWords(DateTime date)
    {
        try
        {
            var months = new[] { "", "January", "February", "March", "April", "May", "June",
                               "July", "August", "September", "October", "November", "December" };

            var day = ConvertNumberToWords(date.Day);
            var month = months[date.Month];
            var year = ConvertNumberToWords(date.Year);

            return $"{day} {month} {year}";
        }
        catch
        {
            return "";
        }
    }

    private string ConvertNumberToWords(int number)
    {
        if (number == 0) return "Zero";

        var ones = new[] { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                          "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                          "Seventeen", "Eighteen", "Nineteen" };

        var tens = new[] { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

        var result = "";

        if (number >= 1000)
        {
            result += ones[number / 1000] + " Thousand ";
            number %= 1000;
        }

        if (number >= 100)
        {
            result += ones[number / 100] + " Hundred ";
            number %= 100;
        }

        if (number >= 20)
        {
            result += tens[number / 10] + " ";
            number %= 10;
        }

        if (number > 0)
        {
            result += ones[number];
        }

        return result.Trim();
    }

    private void BtnGenerateLeavingCert_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbLeavingCertStudent.SelectedItem is StudentDto selectedStudent)
            {
                // Validate required fields
                if (dpLeavingDate.SelectedDate == null)
                {
                    toastNotification.Message = "Please select a leaving date.";
                    toastNotification.ToastType = ToastType.Warning;
                    toastNotification.Show();
                    return;
                }

                if (cmbLeavingReason.SelectedItem == null)
                {
                    toastNotification.Message = "Please select a reason for leaving.";
                    toastNotification.ToastType = ToastType.Warning;
                    toastNotification.Show();
                    return;
                }

                if (cmbCharacterConduct.SelectedItem == null)
                {
                    toastNotification.Message = "Please select character/conduct rating.";
                    toastNotification.ToastType = ToastType.Warning;
                    toastNotification.Show();
                    return;
                }

                // Populate additional certificate fields
                if (FindName("CertLeavingDate") is TextBlock certLeavingDate)
                    certLeavingDate.Text = dpLeavingDate.SelectedDate.Value.ToString("dd/MM/yyyy");

                // Populate Medium field
                if (FindName("CertMedium") is TextBlock certMedium)
                {
                    var medium = (cmbMedium?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "English";
                    certMedium.Text = medium;
                }

                // Populate Mother Tongue field
                if (FindName("CertMotherTongue") is TextBlock certMotherTongue)
                {
                    var motherTongueSelection = (cmbMotherTongue?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Marathi";
                    if (motherTongueSelection == "Other" && !string.IsNullOrWhiteSpace(txtCustomMotherTongue.Text))
                    {
                        certMotherTongue.Text = txtCustomMotherTongue.Text;
                    }
                    else if (motherTongueSelection != "Other")
                    {
                        certMotherTongue.Text = motherTongueSelection;
                    }
                    else
                    {
                        certMotherTongue.Text = "Marathi"; // Default if Other is selected but no custom value entered
                    }
                }

                if (FindName("CertLeavingReason") is TextBlock certLeavingReason)
                {
                    var reason = (cmbLeavingReason.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "";
                    certLeavingReason.Text = reason;
                }

                // For field 13 - Progress in Studies
                if (FindName("CertProgressInStudies") is TextBlock certProgressInStudies)
                {
                    var progress = (cmbProgressInStudies?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Good";
                    certProgressInStudies.Text = progress;
                }

                // For field 13 - Behavior
                if (FindName("CertCharacterConduct") is TextBlock certCharacterConduct)
                {
                    var conduct = (cmbCharacterConduct.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Good";
                    certCharacterConduct.Text = conduct;
                }

                if (FindName("CertRemarks") is TextBlock certRemarks)
                    certRemarks.Text = !string.IsNullOrWhiteSpace(txtLeavingRemarks.Text) ? txtLeavingRemarks.Text : "NIL";

                if (FindName("CertIssueDate") is TextBlock certIssueDate)
                    certIssueDate.Text = DateTime.Now.ToString("dd/MM/yyyy");

                // Populate new fields from Certificate Details textboxes
                // Place of Birth fields (Field 8)
                if (FindName("CertBirthPlace") is TextBlock certBirthPlace)
                    certBirthPlace.Text = !string.IsNullOrWhiteSpace(txtBirthPlace?.Text) ? txtBirthPlace.Text : "";

                if (FindName("CertTaluka") is TextBlock certTaluka)
                    certTaluka.Text = !string.IsNullOrWhiteSpace(txtTaluka?.Text) ? txtTaluka.Text : "";

                if (FindName("CertDistrict") is TextBlock certDistrict)
                    certDistrict.Text = !string.IsNullOrWhiteSpace(txtDistrict?.Text) ? txtDistrict.Text : "";

                if (FindName("CertBirthStateDisplay") is TextBlock certBirthStateDisplay)
                    certBirthStateDisplay.Text = !string.IsNullOrWhiteSpace(txtBirthState?.Text) ? txtBirthState.Text : "Maharashtra";

                // Populate Header fields
                if (FindName("CertHeaderPhone") is TextBlock certHeaderPhone)
                    certHeaderPhone.Text = !string.IsNullOrWhiteSpace(txtPhoneNumber?.Text) ? txtPhoneNumber.Text : "";

                if (FindName("CertHeaderEmail") is TextBlock certHeaderEmail)
                    certHeaderEmail.Text = !string.IsNullOrWhiteSpace(txtEmail?.Text) ? txtEmail.Text : "";

                if (FindName("CertHeaderSerialNo") is TextBlock certHeaderSerialNo)
                {
                    var serialNo = !string.IsNullOrWhiteSpace(txtSerialNoOverride?.Text) ? txtSerialNoOverride.Text : selectedStudent.SerialNo.ToString();
                    certHeaderSerialNo.Text = serialNo;
                }

                if (FindName("CertHeaderGeneralRegister") is TextBlock certHeaderGeneralRegister)
                    certHeaderGeneralRegister.Text = !string.IsNullOrWhiteSpace(txtGeneralRegisterNumber?.Text) ? txtGeneralRegisterNumber.Text : selectedStudent.StudentNumber;

                if (FindName("CertSubcaste") is TextBlock certSubcaste)
                    certSubcaste.Text = !string.IsNullOrWhiteSpace(txtSubcaste?.Text) ? txtSubcaste.Text : "";

                if (FindName("CertSerialNo") is TextBlock certSerialNo)
                {
                    // Use override if provided, otherwise use student's serial number
                    var serialNo = !string.IsNullOrWhiteSpace(txtSerialNoOverride?.Text) ? txtSerialNoOverride.Text : selectedStudent.SerialNo.ToString();
                    certSerialNo.Text = serialNo;
                }

                if (FindName("CertStudentId") is TextBlock certStudentId)
                    certStudentId.Text = !string.IsNullOrWhiteSpace(txtStudentId?.Text) ? txtStudentId.Text : selectedStudent.Id.ToString();

                if (FindName("CertAadhaarNumber") is TextBlock certAadhaarNumber)
                    certAadhaarNumber.Text = !string.IsNullOrWhiteSpace(txtAadhaarNumber?.Text) ? txtAadhaarNumber.Text : "";

                if (FindName("CertPhoneNumber") is TextBlock certPhoneNumber)
                    certPhoneNumber.Text = !string.IsNullOrWhiteSpace(txtPhoneNumber?.Text) ? txtPhoneNumber.Text : "";

                if (FindName("CertEmail") is TextBlock certEmail)
                    certEmail.Text = !string.IsNullOrWhiteSpace(txtEmail?.Text) ? txtEmail.Text : "";

                if (FindName("CertGeneralRegisterNumber") is TextBlock certGeneralRegisterNumber)
                    certGeneralRegisterNumber.Text = !string.IsNullOrWhiteSpace(txtGeneralRegisterNumber?.Text) ? txtGeneralRegisterNumber.Text : selectedStudent.StudentNumber;

                if (FindName("CertState") is TextBlock certState)
                    certState.Text = !string.IsNullOrWhiteSpace(txtState?.Text) ? txtState.Text : "Maharashtra";

                // Fix for Issue 4: Previous School and Grade
                if (FindName("CertPreviousSchool") is TextBlock certPreviousSchool)
                    certPreviousSchool.Text = !string.IsNullOrWhiteSpace(txtPreviousSchool?.Text) ? txtPreviousSchool.Text : "";

                // Update Grade field with user-specified Academic Session (both numeric and words format)
                if (FindName("CertStandard") is TextBlock certStandard)
                {
                    var userAcademicSession = !string.IsNullOrWhiteSpace(txtAcademicSession?.Text) ? txtAcademicSession.Text : "";
                    var standardWords = ConvertStandardToWords(selectedStudent.Standard);

                    string academicSessionToUse;
                    if (!string.IsNullOrWhiteSpace(userAcademicSession))
                    {
                        academicSessionToUse = userAcademicSession;
                    }
                    else
                    {
                        // Fallback to current year if no academic session is specified
                        var currentYear = DateTime.Now.Year;
                        var nextYear = currentYear + 1;
                        academicSessionToUse = $"{currentYear}–{nextYear.ToString().Substring(2)}";
                    }

                    // Standard values are already display-formatted in the data
                    // ("Nursery", "KG1", "1st", "2nd", "10th"), so use them as-is. The old
                    // code blindly appended "th", producing "Nurseryth", "1stth", "2ndth".
                    var standardNumeric = selectedStudent.Standard;
                    var numericFormat = $"{standardNumeric} — Academic Session {academicSessionToUse}";
                    var wordsFormat = $"Class {standardWords} — Academic Session {ConvertAcademicSessionToWords(academicSessionToUse)}";

                    // Combine both formats with proper line break
                    var combinedText = $"{numericFormat}\n{wordsFormat}";
                    certStandard.Text = combinedText;
                }

                toastNotification.Message = "Leaving Certificate generated successfully!";
                toastNotification.ToastType = ToastType.Success;
                toastNotification.Show();

                lblStatus.Text = "Leaving Certificate generated";
            }
            else
            {
                toastNotification.Message = "Please select a student to generate Leaving certificate.";
                toastNotification.ToastType = ToastType.Warning;
                toastNotification.Show();
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error generating certificate: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void BtnPrintLeavingCert_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbLeavingCertStudent.SelectedItem is StudentDto selectedStudent)
            {
                // First generate the certificate if not already done
                BtnGenerateLeavingCert_Click(sender, e);

                // Print the certificate
                var printDialog = new System.Windows.Controls.PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Create a copy of the certificate border for printing
                    var certificateToPrint = LeavingCertificateBorder;

                    // Get the printable area
                    var printableArea = printDialog.PrintableAreaWidth;
                    var printableHeight = printDialog.PrintableAreaHeight;

                    // Ensure the certificate fits the printable area
                    var originalWidth = certificateToPrint.ActualWidth;
                    var originalHeight = certificateToPrint.ActualHeight;

                    // Calculate scale factor to fit within printable area while maintaining aspect ratio
                    var scaleX = printableArea / originalWidth;
                    var scaleY = printableHeight / originalHeight;
                    var scale = Math.Min(scaleX, scaleY);

                    // Create a transform group for scaling
                    var transformGroup = new TransformGroup();
                    transformGroup.Children.Add(new ScaleTransform(scale, scale));

                    // Apply transform to ensure it fits
                    certificateToPrint.RenderTransform = transformGroup;

                    try
                    {
                        // Print with proper scaling
                        printDialog.PrintVisual(certificateToPrint, $"Leaving Certificate - {selectedStudent.FullName}");

                        toastNotification.Message = "Leaving Certificate sent to printer!";
                        toastNotification.ToastType = ToastType.Success;
                        toastNotification.Show();

                        lblStatus.Text = "Certificate printed";
                    }
                    finally
                    {
                        // Reset transform
                        certificateToPrint.RenderTransform = null;
                    }
                }
            }
            else
            {
                toastNotification.Message = "Please select a student and generate certificate first.";
                toastNotification.ToastType = ToastType.Warning;
                toastNotification.Show();
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error printing certificate: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void BtnExportLeavingCertPDF_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbLeavingCertStudent.SelectedItem is StudentDto selectedStudent)
            {
                // First generate the certificate if not already done
                BtnGenerateLeavingCert_Click(sender, e);

                // Create save file dialog - offering both XPS and PDF options
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "XPS Document (*.xps)|*.xps|PDF files (*.pdf)|*.pdf",
                    DefaultExt = "xps",
                    FilterIndex = 1,
                    FileName = $"School_Leaving_Certificate_{selectedStudent.FullName.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var extension = Path.GetExtension(saveFileDialog.FileName).ToLower();

                    if (extension == ".xps")
                    {
                        // Direct XPS export (native Windows format)
                        ExportCertificateToXPS(LeavingCertificateBorder, saveFileDialog.FileName);

                        toastNotification.Message = "Certificate exported to XPS successfully!\n\nYou can view this in Windows or print to PDF.";
                        toastNotification.ToastType = ToastType.Success;
                        toastNotification.Show();

                        lblStatus.Text = "Certificate exported to XPS";
                    }
                    else
                    {
                        // PDF export (requires conversion)
                        ExportCertificateToPDF(LeavingCertificateBorder, saveFileDialog.FileName);

                        toastNotification.Message = "Certificate export initiated. Follow the on-screen instructions.";
                        toastNotification.ToastType = ToastType.Info;
                        toastNotification.Show();

                        lblStatus.Text = "Certificate export initiated";
                    }
                }
            }
            else
            {
                toastNotification.Message = "Please select a student and generate certificate first.";
                toastNotification.ToastType = ToastType.Warning;
                toastNotification.Show();
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error exporting certificate: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private void ExportCertificateToPDF(FrameworkElement element, string filePath)
    {
        try
        {
            // Save as XPS first (which can be opened in Windows or printed to PDF)
            var xpsPath = Path.ChangeExtension(filePath, ".xps");
            ExportCertificateToXPS(element, xpsPath);

            // Inform the user
            MessageBox.Show(
                $"Certificate saved as: {xpsPath}\n\n" +
                "To convert to PDF:\n" +
                "1. Open the XPS file\n" +
                "2. Press Ctrl+P to print\n" +
                "3. Select 'Microsoft Print to PDF'\n" +
                "4. Save as PDF",
                "Certificate Exported",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to export certificate: {ex.Message}", ex);
        }
    }

    private void ConvertXpsToPdf(string xpsFilePath, string pdfFilePath)
    {
        try
        {
            // Try to use Microsoft Print to PDF for conversion
            var printDialog = new PrintDialog();

            // Get all print queues and find a PDF printer
            var printServer = new System.Printing.LocalPrintServer();
            System.Printing.PrintQueue pdfPrintQueue = null;

            // Try to find Microsoft Print to PDF or any other PDF printer
            var pdfPrinters = new[] { "Microsoft Print to PDF", "Microsoft XPS Document Writer", "Adobe PDF" };

            foreach (var printerName in pdfPrinters)
            {
                try
                {
                    pdfPrintQueue = printServer.GetPrintQueue(printerName);
                    if (pdfPrintQueue != null)
                        break;
                }
                catch
                {
                    continue;
                }
            }

            if (pdfPrintQueue == null)
            {
                // Fallback: Copy XPS file as is (Windows can view XPS files natively)
                var xpsOutputPath = Path.ChangeExtension(pdfFilePath, ".xps");
                File.Copy(xpsFilePath, xpsOutputPath, true);
                throw new Exception($"No PDF printer found. Certificate saved as XPS file instead: {xpsOutputPath}\n\nYou can open this file in Windows or convert it to PDF using 'Microsoft Print to PDF' from your print dialog.");
            }

            // Load the XPS document
            using (var xpsDocument = new System.Windows.Xps.Packaging.XpsDocument(xpsFilePath, FileAccess.Read))
            {
                var documentPaginator = xpsDocument.GetFixedDocumentSequence().DocumentPaginator;

                // Set the print queue
                printDialog.PrintQueue = pdfPrintQueue;

                // For Microsoft Print to PDF, we need to set the output file
                if (pdfPrintQueue.Name.Contains("Microsoft Print to PDF"))
                {
                    // Use reflection to set the output file path for Microsoft Print to PDF
                    var printTicket = printDialog.PrintTicket;
                    printDialog.PrintDocument(documentPaginator, Path.GetFileNameWithoutExtension(pdfFilePath));

                    // Since PrintDialog doesn't directly support file output path,
                    // we'll use a workaround: save as XPS and let user print to PDF manually
                    var xpsOutputPath = Path.ChangeExtension(pdfFilePath, ".xps");
                    File.Copy(xpsFilePath, xpsOutputPath, true);

                    // Also copy to the desired PDF path as XPS for now
                    // User can manually print to PDF from Windows
                    File.Copy(xpsFilePath, pdfFilePath + ".xps", true);

                    throw new Exception($"Certificate saved as XPS file: {pdfFilePath}.xps\n\nTo convert to PDF:\n1. Open the XPS file\n2. Press Ctrl+P to print\n3. Select 'Microsoft Print to PDF'\n4. Save to {pdfFilePath}");
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"XPS to PDF conversion failed: {ex.Message}", ex);
        }
    }

    private void ExportCertificateToXPS(FrameworkElement element, string filePath)
    {
        try
        {
            // Ensure the element is measured and arranged
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();

            // Create a FixedDocument for the certificate
            var fixedDocument = new System.Windows.Documents.FixedDocument();
            var pageContent = new System.Windows.Documents.PageContent();
            var fixedPage = new System.Windows.Documents.FixedPage
            {
                Width = 793.7,  // A4 width at 96 DPI
                Height = 1122.5 // A4 height at 96 DPI
            };

            // Calculate scale to fit A4 page while maintaining aspect ratio
            var scaleX = fixedPage.Width / element.ActualWidth;
            var scaleY = fixedPage.Height / element.ActualHeight;
            var scale = Math.Min(scaleX, scaleY);

            // Create a visual brush from the certificate
            var visualBrush = new VisualBrush(element)
            {
                Stretch = Stretch.Uniform,
                TileMode = TileMode.None
            };

            // Create a rectangle to hold the visual
            var rectangle = new System.Windows.Shapes.Rectangle
            {
                Width = element.ActualWidth * scale,
                Height = element.ActualHeight * scale,
                Fill = visualBrush
            };

            // Center the content on the page
            System.Windows.Documents.FixedPage.SetLeft(rectangle, (fixedPage.Width - rectangle.Width) / 2);
            System.Windows.Documents.FixedPage.SetTop(rectangle, (fixedPage.Height - rectangle.Height) / 2);

            fixedPage.Children.Add(rectangle);
            pageContent.Child = fixedPage;
            fixedDocument.Pages.Add(pageContent);

            // Create XPS document using Package API
            using (var package = System.IO.Packaging.Package.Open(filePath, FileMode.Create))
            using (var xpsDocument = new System.Windows.Xps.Packaging.XpsDocument(package))
            {
                var xpsWriter = System.Windows.Xps.Packaging.XpsDocument.CreateXpsDocumentWriter(xpsDocument);
                xpsWriter.Write(fixedDocument);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to export certificate to XPS: {ex.Message}", ex);
        }
    }

    private void CmbMotherTongue_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        try
        {
            // Add null checks to prevent initialization issues
            if (cmbMotherTongue?.SelectedItem is ComboBoxItem selectedItem && txtCustomMotherTongue != null)
            {
                var selectedValue = selectedItem.Content.ToString();
                if (selectedValue == "Other")
                {
                    // Show the custom input textbox
                    txtCustomMotherTongue.Visibility = System.Windows.Visibility.Visible;
                    txtCustomMotherTongue.Focus();
                }
                else
                {
                    // Hide the custom input textbox
                    txtCustomMotherTongue.Visibility = System.Windows.Visibility.Collapsed;
                    txtCustomMotherTongue.Text = "";
                }
            }
        }
        catch (Exception ex)
        {
            toastNotification.Message = $"Error handling mother tongue selection: {ex.Message}";
            toastNotification.ToastType = ToastType.Error;
            toastNotification.Show();
        }
    }

    private string ConvertStandardToWords(string standard)
    {
        var standardMap = new Dictionary<string, string>
        {
            {"1", "First"},
            {"2", "Second"},
            {"3", "Third"},
            {"4", "Fourth"},
            {"5", "Fifth"},
            {"6", "Sixth"},
            {"7", "Seventh"},
            {"8", "Eighth"},
            {"9", "Ninth"},
            {"10", "Tenth"},
            {"11", "Eleventh"},
            {"12", "Twelfth"}
        };

        if (string.IsNullOrWhiteSpace(standard)) return standard ?? string.Empty;

        // Data stores standards as "1st", "2nd", "10th", "Nursery", "KG1" — normalise the
        // leading digits ("1st" -> "1") so the map actually resolves; fall back to the
        // original text for non-numeric standards (Nursery/KG).
        var digits = new string(standard.TakeWhile(char.IsDigit).ToArray());
        if (!string.IsNullOrEmpty(digits) && standardMap.ContainsKey(digits))
            return standardMap[digits];

        return standardMap.ContainsKey(standard) ? standardMap[standard] : standard;
    }

    private string ConvertAcademicSessionToWords(string academicSession)
    {
        // Convert format like "2025-26" to "Two Thousand Twenty-Five–Twenty-Six"
        if (string.IsNullOrWhiteSpace(academicSession))
            return "";

        var parts = academicSession.Split('-', '–');
        if (parts.Length != 2)
            return academicSession;

        try
        {
            var firstYear = int.Parse(parts[0]);
            var secondPart = parts[1];

            // Handle 2-digit second part (e.g., "26")
            int secondYear;
            if (secondPart.Length == 2)
            {
                // Convert "26" to "2026"
                var century = firstYear / 100 * 100; // Gets 2000 from 2025
                secondYear = century + int.Parse(secondPart);
            }
            else
            {
                // Full year format
                secondYear = int.Parse(secondPart);
            }

            var firstYearWords = ConvertYearToWords(firstYear);
            var secondYearWords = ConvertYearToWords(secondYear);

            return $"{firstYearWords}–{secondYearWords}";
        }
        catch
        {
            return academicSession; // Return original if parsing fails
        }
    }

    private string ConvertYearToWords(int year)
    {
        // Convert years like 2025 to "Two Thousand Twenty-Five"
        if (year < 1000 || year > 9999)
            return year.ToString();

        var thousands = year / 1000;
        var remainder = year % 1000;
        var hundreds = remainder / 100;
        var tens = (remainder % 100) / 10;
        var units = remainder % 10;

        var result = "";

        // Thousands
        if (thousands > 0)
        {
            result += ConvertNumberToWords(thousands) + " Thousand";
        }

        // Hundreds
        if (hundreds > 0)
        {
            result += (result.Length > 0 ? " " : "") + ConvertNumberToWords(hundreds) + " Hundred";
        }

        // Tens and units
        var lastTwoDigits = remainder % 100;
        if (lastTwoDigits > 0)
        {
            if (result.Length > 0) result += " ";

            if (lastTwoDigits < 20)
            {
                result += ConvertNumberToWords(lastTwoDigits);
            }
            else
            {
                var tensWords = ConvertNumberToWords(tens * 10);
                result += tensWords;
                if (units > 0)
                {
                    result += "-" + ConvertNumberToWords(units);
                }
            }
        }

        return result;
    }


    #region Bulk Promotion Methods

    private async Task LoadPromotionDataAsync()
    {
        try
        {
            if (_bulkPromotionService == null || _academicYearService == null)
                return;

            // Always load classes fresh from database for promotion dropdowns
            var classes = await _classService.GetAllClassesAsync();
            var classesForPromotion = classes
                .Select(c => new { Id = c.Id, Name = c.Name })
                .OrderBy(c => GetClassOrder(c.Name))
                .ToList();

            cmbPromotionFromClass.ItemsSource = classesForPromotion;
            cmbPromotionToClass.ItemsSource = classesForPromotion;

            // Load academic years
            var academicYears = await _academicYearService.GetRecentAcademicYearsAsync();
            cmbPromotionAcademicYear.ItemsSource = academicYears;

            // Use centralized method to get current academic year
            var currentAcademicYear = await _academicYearService.GetCurrentAcademicYearAsync();
            if (currentAcademicYear != null)
            {
                cmbPromotionAcademicYear.SelectedItem = academicYears.FirstOrDefault(ay => ay.Year == currentAcademicYear.Year);
            }
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Error loading promotion data: {ex.Message}";
        }
    }

    private int GetClassOrder(string className)
    {
        // Define order for sorting classes logically
        var classOrder = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            {"Nursery", 0}, {"KG1", 1}, {"KG2", 2},
            {"Class 1", 3}, {"Class 2", 4}, {"Class 3", 5},
            {"Class 4", 6}, {"Class 5", 7}, {"Class 6", 8},
            {"Class 7", 9}, {"Class 8", 10}, {"Class 9", 11},
            {"Class 10", 12}
        };

        return classOrder.TryGetValue(className, out var order) ? order : 99;
    }

    private void CmbPromotionFromClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ValidatePromotionInputs();
    }

    private void CmbPromotionToClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ValidatePromotionInputs();
    }

    private void CmbPromotionAcademicYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ValidatePromotionInputs();
    }

    private void ValidatePromotionInputs()
    {
        var hasValidInput = cmbPromotionFromClass?.SelectedValue != null &&
                           cmbPromotionToClass?.SelectedValue != null &&
                           cmbPromotionAcademicYear?.SelectedItem != null &&
                           !cmbPromotionFromClass.SelectedValue.Equals(cmbPromotionToClass.SelectedValue);

        btnExecutePromotion.IsEnabled = hasValidInput;

        if (cmbPromotionFromClass?.SelectedValue != null && cmbPromotionToClass?.SelectedValue != null &&
            cmbPromotionFromClass.SelectedValue.Equals(cmbPromotionToClass.SelectedValue))
        {
            lblStatus.Text = "Source and target classes must be different";
            txtPromotionSummary.Text = "Source and target classes must be different.";
        }
        else if (hasValidInput)
        {
            lblStatus.Text = "Ready to execute bulk promotion";
            txtPromotionSummary.Text = $"Ready to promote students from {cmbPromotionFromClass.Text} to {cmbPromotionToClass.Text} for academic year {((dynamic)cmbPromotionAcademicYear.SelectedItem).Year}.\n\nClick 'Execute Promotion' to proceed with bulk promotion.";
        }
        else
        {
            lblStatus.Text = "Please select source class, target class, and academic year";
            txtPromotionSummary.Text = "Select source and target classes, then click 'Execute Promotion' to proceed with bulk promotion.";
        }
    }


    private void BtnExecutePromotion_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(BtnExecutePromotion_ClickAsync);
    }

    private async Task BtnExecutePromotion_ClickAsync()
    {
        try
        {
            if (_bulkPromotionService == null)
            {
                MessageBox.Show("Bulk promotion service is not available.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (cmbPromotionFromClass.SelectedValue == null || cmbPromotionToClass.SelectedValue == null || cmbPromotionAcademicYear.SelectedValue == null)
            {
                MessageBox.Show("Please select source class, target class, and academic year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var fromClassId = (int)cmbPromotionFromClass.SelectedValue;
            var toClassId = (int)cmbPromotionToClass.SelectedValue;

            if (fromClassId == toClassId)
            {
                MessageBox.Show("Source and target classes must be different.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get a preview first to show how many students will be promoted
            var preview = await _bulkPromotionService.GetPromotionPreviewAsync(fromClassId, toClassId);
            var eligibleStudents = preview.Count(p => p.IsEligible);

            var result = MessageBox.Show(
                $"Are you sure you want to promote {eligibleStudents} eligible students from {cmbPromotionFromClass.Text} to {cmbPromotionToClass.Text}?\n\nThis action cannot be easily undone.",
                "Confirm Bulk Promotion",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            lblStatus.Text = "Executing bulk promotion...";
            btnExecutePromotion.IsEnabled = false;

            // Resolve the selected academic year's Id. The service writes
            // StudentPromotionHistory.AcademicYearId (a required FK), so passing only the
            // year string left AcademicYearId at 0 and every UI promotion failed with a
            // foreign-key violation. Also map Remarks/PromotedBy (the service reads those).
            var selectedYear = cmbPromotionAcademicYear.SelectedItem as AcademicYearDto;
            if (selectedYear == null)
            {
                MessageBox.Show("Please select a valid academic year.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var request = new BulkPromotionRequest
            {
                FromClassId = fromClassId,
                ToClassId = toClassId,
                AcademicYearId = selectedYear.Id,
                ExcludedStudentIds = new List<int>(), // No exclusions since no preview
                Reason = "Annual Promotion",
                Remarks = "Annual Promotion",
                PromotedBy = Environment.UserName
            };

            var promotionResult = await _bulkPromotionService.ExecuteBulkPromotionAsync(request);

            // Update summary and show results
            if (promotionResult.IsSuccess)
            {
                txtPromotionSummary.Text = $"✅ Promotion Completed Successfully!\n\nPromoted: {promotionResult.PromotedStudents} students\nFrom: {cmbPromotionFromClass.Text}\nTo: {cmbPromotionToClass.Text}\nAcademic Year: {selectedYear.Year}\nDate: {promotionResult.PromotionDate:yyyy-MM-dd HH:mm:ss}";
                MessageBox.Show($"Bulk promotion completed successfully!\n\nPromoted: {promotionResult.PromotedStudents} students from {cmbPromotionFromClass.Text} to {cmbPromotionToClass.Text}",
                               "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                txtPromotionSummary.Text = $"⚠️ Promotion Completed with Errors\n\nPromoted: {promotionResult.PromotedStudents} students\nFailed: {promotionResult.FailedPromotions} students\nFrom: {cmbPromotionFromClass.Text}\nTo: {cmbPromotionToClass.Text}\nAcademic Year: {selectedYear.Year}\nDate: {promotionResult.PromotionDate:yyyy-MM-dd HH:mm:ss}";
                var errorDetails = string.Join("\n", promotionResult.Errors.Take(5).Select(e => $"• {e.StudentName}: {e.Error}"));
                if (promotionResult.Errors.Count > 5)
                    errorDetails += $"\n... and {promotionResult.Errors.Count - 5} more errors";

                MessageBox.Show($"Bulk promotion completed with errors.\n\nPromoted: {promotionResult.PromotedStudents}\nFailed: {promotionResult.FailedPromotions}\n\nSample errors:\n{errorDetails}",
                               "Partial Success", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            lblStatus.Text = promotionResult.IsSuccess
                ? $"Successfully promoted {promotionResult.PromotedStudents} students"
                : $"Promotion completed with {promotionResult.FailedPromotions} errors";

            // Remember current class filter selection
            var currentClassFilter = cmbStudentClassFilter?.SelectedValue as int?;

            // Refresh students list
            await LoadStudentsAsync();

            // Refresh classes to update student counts and dropdown
            await LoadClassesAsync();

            // Restore the class filter selection if it was set
            if (currentClassFilter.HasValue && cmbStudentClassFilter != null)
            {
                cmbStudentClassFilter.SelectedValue = currentClassFilter.Value;
            }

            // Apply filters to show updated list
            ApplyStudentFilters();

            // Refresh dashboard
            AsyncHelper.SafeFireAndForget(LoadDashboardDataAsync);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error executing promotion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            lblStatus.Text = "Error executing promotion";
        }
        finally
        {
            btnExecutePromotion.IsEnabled = true;
        }
    }


    private void BtnCancelPromotion_Click(object sender, RoutedEventArgs e)
    {
        // Reset the promotion form
        cmbPromotionFromClass.SelectedValue = null;
        cmbPromotionToClass.SelectedValue = null;
        cmbPromotionAcademicYear.SelectedValue = null;
        txtPromotionSummary.Text = "Select source and target classes, then click 'Execute Promotion' to proceed with bulk promotion.";
        lblStatus.Text = "Promotion form reset";
    }

    #endregion



    private async Task LoadOutstandingFeesAsync()
    {
        try
        {
            foreach (var student in _allStudents)
            {
                var outstandingBalance = await _feePaymentService.GetStudentOutstandingBalanceAsync(student.Id, "2024-25");
                student.OutstandingFees = outstandingBalance;
                student.HasOutstandingFees = outstandingBalance > 0;
            }
        }
        catch (Exception ex)
        {
            // If fee loading fails, just set to zero to avoid breaking the student list
            foreach (var student in _allStudents)
            {
                student.OutstandingFees = 0;
                student.HasOutstandingFees = false;
            }
            lblStatus.Text = $"Warning: Could not load fee data - {ex.Message}";
        }
    }

    private void BtnViewStudentFees_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStudents.SelectedItem is StudentDto selectedStudent)
                {
                    var studentFeeStatusWindow = new StudentFeeStatusWindow(_feePaymentService, selectedStudent.Id, "2024-25");
                    studentFeeStatusWindow.Owner = this;
                    studentFeeStatusWindow.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Please select a student to view their fees.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening student fee status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "Student Fee Status Error");
    }

    private void DgStudents_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Double-click on a student row opens the fee status window
        if (dgStudents.SelectedItem is StudentDto selectedStudent)
        {
            BtnViewStudentFees_Click(sender, e);
        }
    }

    #region Fee Structure Management

    private async Task InitializeStudentFeeStructureFilters()
    {
        try
        {
            // Initialize Academic Year dropdown
            var currentYear = DateTime.Now.Year;
            var academicYears = new List<string> { "All" };
            for (int i = -2; i <= 2; i++)
            {
                var year = currentYear + i;
                academicYears.Add($"{year}-{(year + 1).ToString().Substring(2)}");
            }
            cmbStudentFeeAcademicYear.ItemsSource = academicYears;
            cmbStudentFeeAcademicYear.SelectedIndex = 0;

            // Initialize Class dropdown
            var classItems = new List<dynamic> { new { Id = -1, Display = "All Classes" } };
            classItems.AddRange(_allClasses.Select(c => new { Id = c.Id, Display = $"{c.Name} - {c.Section}" }));
            cmbStudentFeeClassFilter.ItemsSource = classItems;
            cmbStudentFeeClassFilter.DisplayMemberPath = "Display";
            cmbStudentFeeClassFilter.SelectedValuePath = "Id";
            cmbStudentFeeClassFilter.SelectedIndex = 0;

            // Initialize Fee Type dropdown
            var feeTypes = new List<dynamic> { new { Value = -1, Display = "All Types" } };
            feeTypes.AddRange(Enum.GetValues<IEMS.Core.Enums.FeeType>()
                .Select(ft => new { Value = (int)ft, Display = ft.ToString() }));
            cmbStudentFeeFeeTypeFilter.ItemsSource = feeTypes;
            cmbStudentFeeFeeTypeFilter.DisplayMemberPath = "Display";
            cmbStudentFeeFeeTypeFilter.SelectedValuePath = "Value";
            cmbStudentFeeFeeTypeFilter.SelectedIndex = 0;

            // Apply initial filters
            ApplyStudentFeeStructureFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing fee structure filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyStudentFeeStructureFilters()
    {
        try
        {
            if (_allFeeStructures == null || !_allFeeStructures.Any()) return;

            var filtered = _allFeeStructures.AsEnumerable();

            // Apply Academic Year filter
            if (cmbStudentFeeAcademicYear?.SelectedItem != null && cmbStudentFeeAcademicYear.SelectedItem.ToString() != "All")
            {
                var selectedYear = cmbStudentFeeAcademicYear.SelectedItem.ToString();
                filtered = filtered.Where(fs => fs.AcademicYear == selectedYear);
            }

            // Apply Class filter
            if (cmbStudentFeeClassFilter?.SelectedValue != null && (int)cmbStudentFeeClassFilter.SelectedValue != -1)
            {
                var classId = (int)cmbStudentFeeClassFilter.SelectedValue;
                filtered = filtered.Where(fs => fs.ClassId == classId);
            }

            // Apply Fee Type filter
            if (cmbStudentFeeFeeTypeFilter?.SelectedValue != null && (int)cmbStudentFeeFeeTypeFilter.SelectedValue != -1)
            {
                var feeType = (IEMS.Core.Enums.FeeType)cmbStudentFeeFeeTypeFilter.SelectedValue;
                filtered = filtered.Where(fs => fs.FeeType == feeType);
            }


            // Apply search filter
            if (!string.IsNullOrWhiteSpace(txtStudentFeeSearch?.Text))
            {
                var searchTerm = txtStudentFeeSearch.Text.ToLower();
                filtered = filtered.Where(fs => fs.Description.ToLower().Contains(searchTerm));
            }

            var filteredList = filtered.ToList();
            dgStudentFeeStructures.ItemsSource = filteredList;

            // Show/hide empty state
            if (studentFeeStructureEmptyStatePanel != null)
            {
                if (filteredList.Count == 0)
                {
                    dgStudentFeeStructures.Visibility = Visibility.Collapsed;
                    studentFeeStructureEmptyStatePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    dgStudentFeeStructures.Visibility = Visibility.Visible;
                    studentFeeStructureEmptyStatePanel.Visibility = Visibility.Collapsed;
                }
            }

            UpdateStudentFeeStructureStatusBar();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error applying fee structure filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateStudentFeeStructureStatusBar()
    {
        try
        {
            var items = dgStudentFeeStructures?.ItemsSource as List<FeeStructureDto>;
            if (items != null && lblStudentTotalFeeRecords != null && lblStudentTotalFeeAmount != null)
            {
                lblStudentTotalFeeRecords.Text = $"Total Records: {items.Count}";
                var totalAmount = items.Sum(fs => fs.Amount);
                lblStudentTotalFeeAmount.Text = $"Total Amount: ₹{totalAmount:N2}";
            }
            else if (lblStudentTotalFeeRecords != null && lblStudentTotalFeeAmount != null)
            {
                lblStudentTotalFeeRecords.Text = "Total Records: 0";
                lblStudentTotalFeeAmount.Text = "Total Amount: ₹0.00";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error updating fee structure status: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnAddStudentFeeStructure_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                var addEditWindow = App.ServiceProvider?.GetService(typeof(AddEditFeeStructureWindow)) as AddEditFeeStructureWindow;
                if (addEditWindow != null)
                {
                    addEditWindow.Owner = this;
                    if (addEditWindow.ShowDialog() == true)
                    {
                        await LoadFeeStructuresAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Unable to open fee structure window. Service not available.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening add fee structure window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "Add Fee Structure Error");
    }

    private void BtnEditStudentFeeStructure_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStudentFeeStructures.SelectedItem is FeeStructureDto selectedFeeStructure)
                {
                    var addEditWindow = App.ServiceProvider?.GetService(typeof(AddEditFeeStructureWindow)) as AddEditFeeStructureWindow;
                    if (addEditWindow != null)
                    {
                        addEditWindow.Owner = this;
                        addEditWindow.SetFeeStructureId(selectedFeeStructure.Id);
                        if (addEditWindow.ShowDialog() == true)
                        {
                            await LoadFeeStructuresAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Unable to open fee structure window. Service not available.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a fee structure to edit.", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening edit fee structure window: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "Edit Fee Structure Error");
    }

    private void BtnDeleteStudentFeeStructure_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStudentFeeStructures.SelectedItem is FeeStructureDto selectedFeeStructure)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete the fee structure for {selectedFeeStructure.ClassName} - {selectedFeeStructure.FeeType}?\n\n" +
                        $"Amount: ₹{selectedFeeStructure.Amount:N2}\n" +
                        $"Academic Year: {selectedFeeStructure.AcademicYear}",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _feeStructureService.DeleteFeeStructureAsync(selectedFeeStructure.Id);
                        MessageBox.Show("Fee structure deleted successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadFeeStructuresAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Please select a fee structure to delete.", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting fee structure: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "Delete Fee Structure Error");
    }

    private void DgStudentFeeStructures_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            try
            {
                if (dgStudentFeeStructures.SelectedItem is FeeStructureDto selectedFeeStructure)
                {
                    var addEditWindow = App.ServiceProvider?.GetService(typeof(AddEditFeeStructureWindow)) as AddEditFeeStructureWindow;
                    if (addEditWindow != null)
                    {
                        addEditWindow.Owner = this;
                        addEditWindow.SetFeeStructureId(selectedFeeStructure.Id);
                        if (addEditWindow.ShowDialog() == true)
                        {
                            ApplyStudentFeeStructureFilters();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening fee structure for editing: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }, "Fee Structure Double Click Error");
    }

    private void DgStudentFeeStructures_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var hasSelection = dgStudentFeeStructures.SelectedItem != null;
        btnEditStudentFeeStructure.IsEnabled = hasSelection;
        btnDeleteStudentFeeStructure.IsEnabled = hasSelection;
    }

    private void BtnRefreshStudentFeeStructures_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(LoadFeeStructuresAsync, "Refresh Fee Structures Error");
    }

    private void CmbStudentFeeAcademicYear_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyStudentFeeStructureFilters();
    }

    private void CmbStudentFeeClassFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyStudentFeeStructureFilters();
    }

    private void CmbStudentFeeFeeTypeFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyStudentFeeStructureFilters();
    }


    private void TxtStudentFeeSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyStudentFeeStructureFilters();
    }

    private void BtnClearStudentFeeFilters_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (cmbStudentFeeAcademicYear != null) cmbStudentFeeAcademicYear.SelectedIndex = 0;
            if (cmbStudentFeeClassFilter != null) cmbStudentFeeClassFilter.SelectedIndex = 0;
            if (cmbStudentFeeFeeTypeFilter != null) cmbStudentFeeFeeTypeFilter.SelectedIndex = 0;
            if (txtStudentFeeSearch != null) txtStudentFeeSearch.Text = "";
            ApplyStudentFeeStructureFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error clearing filters: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion
}