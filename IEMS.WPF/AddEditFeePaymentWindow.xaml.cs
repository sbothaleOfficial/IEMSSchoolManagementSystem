using System.Windows;
using System.Windows.Controls;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AddEditFeePaymentWindow : Window
{
    private readonly FeePaymentService _feePaymentService;
    private readonly FeeStructureService _feeStructureService;
    private readonly StudentService _studentService;
    private readonly AcademicYearService _academicYearService;
    private List<StudentDto> _students = new List<StudentDto>();
    private List<FeeStructureDto> _feeStructures = new List<FeeStructureDto>();
    private int? _preSelectedStudentId;

    public AddEditFeePaymentWindow(
        FeePaymentService feePaymentService,
        FeeStructureService feeStructureService,
        StudentService studentService,
        AcademicYearService academicYearService)
    {
        try
        {
            InitializeComponent();

            // Add null checks for all services
            _feePaymentService = feePaymentService ?? throw new ArgumentNullException(nameof(feePaymentService));
            _feeStructureService = feeStructureService ?? throw new ArgumentNullException(nameof(feeStructureService));
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            _academicYearService = academicYearService ?? throw new ArgumentNullException(nameof(academicYearService));

            // Use Loaded event to ensure UI is fully initialized
            this.Loaded += AddEditFeePaymentWindow_Loaded;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing fee payment window: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
    }

    private void AddEditFeePaymentWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () => await LoadData(), "Fee Payment Window Loading Error");
    }

    public void SetStudentId(int studentId)
    {
        _preSelectedStudentId = studentId;
    }

    private async Task LoadData()
    {
        try
        {
            // Check if all UI elements are available
            if (cmbStudent == null || cmbFeeType == null || cmbPaymentMethod == null || txtPaymentSummary == null)
            {
                throw new InvalidOperationException("UI elements not properly initialized");
            }

            await LoadStudents();
            await LoadAcademicYears();
            LoadFeeTypes();
            LoadPaymentMethods();

            // Initialize the payment summary
            txtPaymentSummary.Text = "Amount: ₹0.00\nIn Words: Zero Rupees Only";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading fee payment data: {ex.Message}", "Data Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task LoadStudents()
    {
        var students = await _studentService.GetAllStudentsAsync();
        _students = students.ToList();

        var studentItems = _students.Select(s => new
        {
            Id = s.Id,
            DisplayName = $"{s.FullName} - {s.StudentNumber} ({s.ClassName})"
        }).ToList();

        studentItems.Insert(0, new { Id = 0, DisplayName = "-- Select Student --" });

        cmbStudent.ItemsSource = studentItems;
        // Pre-select student if specified
        if (_preSelectedStudentId.HasValue && _preSelectedStudentId.Value > 0)
        {
            cmbStudent.SelectedValue = _preSelectedStudentId.Value;
        }
        else
        {
            cmbStudent.SelectedValue = 0;
        }
    }

    private void LoadFeeTypes()
    {
        var feeTypes = Enum.GetValues<FeeType>().Select(ft => new
        {
            Value = ft,
            Display = ft.ToString()
        }).ToList();

        feeTypes.Insert(0, new { Value = (FeeType)(-1), Display = "-- Select Fee Type --" });

        cmbFeeType.ItemsSource = feeTypes;
        cmbFeeType.DisplayMemberPath = "Display";
        cmbFeeType.SelectedValuePath = "Value";
        cmbFeeType.SelectedIndex = 0;
    }

    private void LoadPaymentMethods()
    {
        var paymentMethods = Enum.GetValues<PaymentMethod>().Select(pm => new
        {
            Value = pm,
            Display = pm.ToString()
        }).ToList();

        paymentMethods.Insert(0, new { Value = (PaymentMethod)(-1), Display = "-- Select Payment Method --" });

        cmbPaymentMethod.ItemsSource = paymentMethods;
        cmbPaymentMethod.DisplayMemberPath = "Display";
        cmbPaymentMethod.SelectedValuePath = "Value";
        cmbPaymentMethod.SelectedIndex = 0;
    }

    private async Task LoadAcademicYears()
    {
        try
        {
            var academicYears = await _academicYearService.GetRecentAcademicYearsAsync(10);
            var yearItems = academicYears.Select(ay => new
            {
                Value = ay.Year,
                Display = ay.Year,
                IsCurrent = ay.IsCurrent
            }).ToList();

            yearItems.Insert(0, new { Value = "", Display = "-- Select Academic Year --", IsCurrent = false });

            cmbAcademicYear.ItemsSource = yearItems;
            cmbAcademicYear.DisplayMemberPath = "Display";
            cmbAcademicYear.SelectedValuePath = "Value";

            // Use centralized method to get current academic year
            var currentAcademicYear = await _academicYearService.GetCurrentAcademicYearAsync();
            if (currentAcademicYear != null)
            {
                cmbAcademicYear.SelectedValue = currentAcademicYear.Year;
            }
            else
            {
                cmbAcademicYear.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading academic years: {ex.Message}", "Data Loading Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Fallback to default years if database loading fails
            cmbAcademicYear.Items.Clear();
            cmbAcademicYear.Items.Add("2024-25");
            cmbAcademicYear.Items.Add("2025-26");
            cmbAcademicYear.Items.Add("2023-24");
            cmbAcademicYear.SelectedIndex = 0;
        }
    }

    private string GetSelectedAcademicYear()
    {
        if (cmbAcademicYear?.SelectedValue != null && !string.IsNullOrEmpty(cmbAcademicYear.SelectedValue.ToString()))
        {
            return cmbAcademicYear.SelectedValue.ToString();
        }
        return "2024-25"; // Fallback to default year
    }

    private void CmbStudent_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (cmbStudent.SelectedValue != null && (int)cmbStudent.SelectedValue > 0)
            {
                var selectedStudent = _students.FirstOrDefault(s => s.Id == (int)cmbStudent.SelectedValue);
                if (selectedStudent != null)
                {
                    txtStudentInfo.Text = $"Name: {selectedStudent.FullName}\n" +
                                        $"Class: {selectedStudent.ClassName}\n" +
                                        $"Student Number: {selectedStudent.StudentNumber}\n" +
                                        $"Parent Mobile: {selectedStudent.ParentMobileNumber}";
                    borderStudentInfo.Visibility = Visibility.Visible;

                    await LoadFeeStructuresForStudent();
                }
            }
            else
            {
                borderStudentInfo.Visibility = Visibility.Collapsed;
                borderFeeInfo.Visibility = Visibility.Collapsed;
            }
        }, "Student Selection Error");
    }

    private async Task LoadFeeStructuresForStudent()
    {
        if (cmbStudent.SelectedValue == null || (int)cmbStudent.SelectedValue <= 0) return;

        var selectedStudent = _students.FirstOrDefault(s => s.Id == (int)cmbStudent.SelectedValue);
        if (selectedStudent != null)
        {
            var academicYear = GetSelectedAcademicYear();
            _feeStructures = (await _feeStructureService.GetFeeStructuresByClassIdAsync(selectedStudent.ClassId)).ToList();
        }
    }

    private void CmbFeeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () => await UpdateFeeInfo(), "Fee Type Selection Error");
    }

    private async Task UpdateFeeInfo()
    {
        if (cmbStudent.SelectedValue == null || (int)cmbStudent.SelectedValue <= 0 ||
            cmbFeeType.SelectedValue == null || (int)cmbFeeType.SelectedValue == -1) return;

        var selectedStudent = _students.FirstOrDefault(s => s.Id == (int)cmbStudent.SelectedValue);
        var selectedFeeType = (FeeType)cmbFeeType.SelectedValue;

        if (selectedStudent != null)
        {
            try
            {
                var academicYear = GetSelectedAcademicYear();
                var feeStructure = await _feeStructureService.GetFeeStructureByClassFeeTypeAndYearAsync(
                    selectedStudent.ClassId, selectedFeeType, academicYear);

                var totalPaid = await _feePaymentService.GetTotalPaidAmountAsync(selectedStudent.Id, selectedFeeType);
                var pendingAmount = await _feePaymentService.GetPendingAmountAsync(selectedStudent.Id, selectedFeeType);

                if (feeStructure != null)
                {
                    txtFeeStructureInfo.Text = $"Fee Type: {selectedFeeType}\n" +
                                             $"Total Fee Amount: ₹{feeStructure.Amount:F2}\n" +
                                             $"Total Paid: ₹{totalPaid:F2}\n" +
                                             $"Pending Balance: ₹{pendingAmount:F2}\n" +
                                             $"Description: {feeStructure.Description}";
                    borderFeeInfo.Visibility = Visibility.Visible;

                    // Suggest pending amount as default
                    if (pendingAmount > 0)
                    {
                        txtAmount.Text = pendingAmount.ToString("F2");
                    }
                }
                else
                {
                    txtFeeStructureInfo.Text = $"No fee structure defined for {selectedFeeType} in {academicYear}";
                    borderFeeInfo.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                ShowValidationError($"Error loading fee information: {ex.Message}");
            }
        }
    }

    private void CmbPaymentMethod_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbPaymentMethod.SelectedValue != null && (int)cmbPaymentMethod.SelectedValue != -1)
        {
            var selectedMethod = (PaymentMethod)cmbPaymentMethod.SelectedValue;
            pnlTransactionDetails.Visibility = Visibility.Visible;

            switch (selectedMethod)
            {
                case PaymentMethod.CASH:
                    pnlTransactionDetails.Visibility = Visibility.Collapsed;
                    break;
                case PaymentMethod.ONLINE:
                    lblTransactionField.Content = "Transaction ID *";
                    pnlBankDetails.Visibility = Visibility.Collapsed;
                    break;
                case PaymentMethod.CHEQUE:
                    lblTransactionField.Content = "Cheque Number *";
                    pnlBankDetails.Visibility = Visibility.Visible;
                    break;
                case PaymentMethod.DD:
                    lblTransactionField.Content = "DD Number *";
                    pnlBankDetails.Visibility = Visibility.Visible;
                    break;
            }
        }
        else
        {
            pnlTransactionDetails.Visibility = Visibility.Collapsed;
        }
    }

    private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Skip if UI elements are not fully initialized
        if (txtAmount == null || txtAmountInWords == null || txtPaymentSummary == null)
            return;

        if (decimal.TryParse(txtAmount.Text, out decimal amount))
        {
            txtAmountInWords.Text = ConvertAmountToWords(amount);
            txtPaymentSummary.Text = $"Amount: ₹{amount:F2}\nIn Words: {ConvertAmountToWords(amount)}";
        }
        else
        {
            txtAmountInWords.Text = "";
            txtPaymentSummary.Text = "Amount: ₹0.00\nIn Words: Zero Rupees Only";
        }
    }


    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (!ValidateInput()) return;

            try
            {
                var selectedMethod = (PaymentMethod)cmbPaymentMethod.SelectedValue;
                var referenceNumber = txtTransactionId.Text.Trim();
                var isChequeOrDd = selectedMethod == PaymentMethod.CHEQUE || selectedMethod == PaymentMethod.DD;

                var createDto = new CreateFeePaymentDto
                {
                    StudentId = (int)cmbStudent.SelectedValue,
                    FeeType = (FeeType)cmbFeeType.SelectedValue,
                    AmountPaid = decimal.Parse(txtAmount.Text),
                    PaymentMethod = selectedMethod,
                    // The form has a single reference field reused for Transaction ID (ONLINE) and
                    // Cheque/DD Number. Route it to the semantically-correct column ONLY, instead of
                    // writing the same value into BOTH TransactionId and ChequeNumber (which polluted
                    // every payment with a wrong value in the other column).
                    TransactionId = selectedMethod == PaymentMethod.ONLINE ? referenceNumber : null,
                    ChequeNumber = isChequeOrDd ? referenceNumber : null,
                    BankName = isChequeOrDd ? txtBankName.Text.Trim() : null,
                    PaymentNotes = "",
                    LateFee = 0,
                    Discount = 0,
                    InstallmentNumber = null,
                    NextDueDate = null,
                    AcademicYear = GetSelectedAcademicYear(),
                    GeneratedBy = Environment.UserName
                };

                var createdPayment = await _feePaymentService.CreateFeePaymentAsync(createDto);

                MessageBox.Show($"Fee payment recorded successfully!\n\nReceipt Number: {createdPayment.ReceiptNumber}\nAmount: ₹{createdPayment.AmountPaid:F2}",
                              "Payment Successful", MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowValidationError($"Error recording payment: {ex.Message}");
            }
        }, "Payment Save Error");
    }

    private bool ValidateInput()
    {
        if (cmbStudent.SelectedValue == null || (int)cmbStudent.SelectedValue <= 0)
        {
            ShowValidationError("Please select a student.");
            cmbStudent.Focus();
            return false;
        }

        if (cmbFeeType.SelectedValue == null || (int)cmbFeeType.SelectedValue == -1)
        {
            ShowValidationError("Please select a fee type.");
            cmbFeeType.Focus();
            return false;
        }

        if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount <= 0)
        {
            ShowValidationError("Please enter a valid amount greater than zero.");
            txtAmount.Focus();
            return false;
        }

        if (cmbPaymentMethod.SelectedValue == null || (int)cmbPaymentMethod.SelectedValue == -1)
        {
            ShowValidationError("Please select a payment method.");
            cmbPaymentMethod.Focus();
            return false;
        }

        var selectedMethod = (PaymentMethod)cmbPaymentMethod.SelectedValue;
        if (selectedMethod != PaymentMethod.CASH && string.IsNullOrWhiteSpace(txtTransactionId.Text))
        {
            ShowValidationError($"Please enter {lblTransactionField.Content.ToString().Replace(" *", "")}.");
            txtTransactionId.Focus();
            return false;
        }

        if ((selectedMethod == PaymentMethod.CHEQUE || selectedMethod == PaymentMethod.DD) &&
            string.IsNullOrWhiteSpace(txtBankName.Text))
        {
            ShowValidationError("Please enter bank name for cheque/DD payments.");
            txtBankName.Focus();
            return false;
        }

        HideValidationError();
        return true;
    }

    private void ShowValidationError(string message)
    {
        lblValidation.Text = message;
        lblValidation.Visibility = Visibility.Visible;
    }

    private void HideValidationError()
    {
        lblValidation.Visibility = Visibility.Collapsed;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static string ConvertAmountToWords(decimal amount)
    {
        if (amount == 0) return "Zero Rupees Only";

        string[] ones = {"", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine"};
        string[] teens = {"Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen"};
        string[] tens = {"", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety"};

        int rupees = (int)amount;
        int paise = (int)((amount - rupees) * 100);

        string result = ConvertNumberToWords(rupees, ones, teens, tens);

        if (!string.IsNullOrEmpty(result))
        {
            result += " Rupee" + (rupees > 1 ? "s" : "");
        }

        if (paise > 0)
        {
            if (!string.IsNullOrEmpty(result))
                result += " and ";
            result += ConvertNumberToWords(paise, ones, teens, tens) + " Paise";
        }

        return result + " Only";
    }

    private static string ConvertNumberToWords(int number, string[] ones, string[] teens, string[] tens)
    {
        if (number == 0) return "";

        string result = "";

        if (number >= 10000000)
        {
            result += ConvertNumberToWords(number / 10000000, ones, teens, tens) + " Crore ";
            number %= 10000000;
        }

        if (number >= 100000)
        {
            result += ConvertNumberToWords(number / 100000, ones, teens, tens) + " Lakh ";
            number %= 100000;
        }

        if (number >= 1000)
        {
            result += ConvertNumberToWords(number / 1000, ones, teens, tens) + " Thousand ";
            number %= 1000;
        }

        if (number >= 100)
        {
            result += ones[number / 100] + " Hundred ";
            number %= 100;
        }

        if (number >= 20)
        {
            result += tens[number / 10];
            if (number % 10 > 0)
                result += " " + ones[number % 10];
        }
        else if (number >= 10)
        {
            result += teens[number - 10];
        }
        else if (number > 0)
        {
            result += ones[number];
        }

        return result.Trim();
    }
}