using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using IEMS.Application.DTOs;
using IEMS.Application.Services;
using IEMS.Core.Enums;
using IEMS.WPF.Helpers;

namespace IEMS.WPF
{
    public partial class AddEditFeeStructureWindow : Window
    {
        private readonly FeeStructureService _feeStructureService;
        private readonly ClassService _classService;
        private readonly AcademicYearService _academicYearService;
        private int? _feeStructureId;
        private FeeStructureDto? _currentFeeStructure;

        public AddEditFeeStructureWindow(FeeStructureService feeStructureService, ClassService classService, AcademicYearService academicYearService)
        {
            InitializeComponent();
            _feeStructureService = feeStructureService;
            _classService = classService;
            _academicYearService = academicYearService;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    // Initialize Academic Year dropdown from database
                    try
                    {
                        var academicYears = await _academicYearService.GetAllAcademicYearsAsync();
                        var yearsList = academicYears.OrderByDescending(ay => ay.StartDate).Select(ay => ay.Year).ToList();
                        cmbAcademicYear.ItemsSource = yearsList;

                        // Use centralized method to get current academic year
                        var currentAcademicYear = await _academicYearService.GetCurrentAcademicYearAsync();
                        if (currentAcademicYear != null)
                        {
                            cmbAcademicYear.SelectedItem = currentAcademicYear.Year;
                        }
                        else if (yearsList.Any())
                        {
                            cmbAcademicYear.SelectedIndex = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        // Fallback: Generate years dynamically if database loading fails
                        var currentYear = DateTime.Now.Year;
                        var currentMonth = DateTime.Now.Month;
                        var academicYears = new List<string>();
                        for (int i = -2; i <= 2; i++)
                        {
                            var year = currentYear + i;
                            academicYears.Add($"{year}-{(year + 1).ToString().Substring(2)}");
                        }
                        cmbAcademicYear.ItemsSource = academicYears;

                        if (currentMonth >= 6)
                            cmbAcademicYear.SelectedItem = $"{currentYear}-{(currentYear + 1).ToString().Substring(2)}";
                        else
                            cmbAcademicYear.SelectedItem = $"{currentYear - 1}-{currentYear.ToString().Substring(2)}";
                    }

                    // Load classes
                    var classes = await _classService.GetAllClassesAsync();
                    var classItems = classes.Select(c => new
                    {
                        Id = c.Id,
                        Display = $"{c.Name} - {c.Section}"
                    }).ToList();
                    cmbClass.ItemsSource = classItems;
                    cmbClass.DisplayMemberPath = "Display";
                    cmbClass.SelectedValuePath = "Id";

                    // Initialize Fee Type dropdown
                    var feeTypes = Enum.GetValues<FeeType>()
                        .Select(ft => new
                        {
                            Value = (int)ft,
                            Display = GetFeeTypeDisplayName(ft)
                        }).ToList();
                    cmbFeeType.ItemsSource = feeTypes;
                    cmbFeeType.DisplayMemberPath = "Display";
                    cmbFeeType.SelectedValuePath = "Value";

                    // If editing, load the fee structure
                    if (_feeStructureId.HasValue)
                    {
                        await LoadFeeStructure();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading data: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Fee Structure Loading Error");
        }

        private string GetFeeTypeDisplayName(FeeType feeType)
        {
            return feeType switch
            {
                FeeType.TUITION => "Tuition Fee",
                FeeType.ADMISSION => "Admission Fee",
                FeeType.LIBRARY => "Library Fee",
                FeeType.EXAM => "Examination Fee",
                FeeType.SPORTS => "Sports Fee",
                FeeType.TRANSPORT => "Transport Fee",
                FeeType.UNIFORM => "Uniform Fee",
                FeeType.MISCELLANEOUS => "Miscellaneous Fee",
                _ => feeType.ToString()
            };
        }

        public void SetFeeStructureId(int feeStructureId)
        {
            _feeStructureId = feeStructureId;
        }

        private async Task LoadFeeStructure()
        {
            if (!_feeStructureId.HasValue) return;

            try
            {
                _currentFeeStructure = await _feeStructureService.GetFeeStructureByIdAsync(_feeStructureId.Value);
                if (_currentFeeStructure != null)
                {
                    lblTitle.Text = "Edit Fee Structure";
                    Title = "Edit Fee Structure";

                    cmbAcademicYear.SelectedItem = _currentFeeStructure.AcademicYear;
                    cmbClass.SelectedValue = _currentFeeStructure.ClassId;
                    cmbFeeType.SelectedValue = (int)_currentFeeStructure.FeeType;
                    txtAmount.Text = _currentFeeStructure.Amount.ToString("0.00");

                    // Show current info panel
                    pnlCurrentInfo.Visibility = Visibility.Visible;
                    lblFeeStructureId.Text = _currentFeeStructure.Id.ToString();
                    lblCreatedAt.Text = "N/A"; // CreatedAt not available in DTO
                    lblUpdatedAt.Text = "N/A"; // UpdatedAt not available in DTO
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading fee structure: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbClass_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void CmbFeeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDescription();
        }

        private void UpdateDescription()
        {
            // Description field removed - method kept for compatibility
        }

        private void TxtAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only numbers and decimal point
            var regex = new Regex(@"^[0-9]*\.?[0-9]*$");
            e.Handled = !regex.IsMatch(txtAmount.Text + e.Text);
        }

        private void TxtAmount_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (decimal.TryParse(txtAmount.Text, out decimal amount))
            {
                lblAmountInWords.Text = ConvertAmountToWords(amount);
            }
            else
            {
                lblAmountInWords.Text = "";
            }
        }

        private string ConvertAmountToWords(decimal amount)
        {
            if (amount == 0) return "Zero Rupees Only";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            long integerPart = (long)amount;
            int decimalPart = (int)((amount - integerPart) * 100);

            if (integerPart == 0) return "Zero Rupees Only";

            string words = "";
            int groupIndex = 0;

            // Process crores
            if (integerPart >= 10000000)
            {
                long crores = integerPart / 10000000;
                words = ConvertHundreds(crores) + " Crore ";
                integerPart %= 10000000;
            }

            // Process lakhs
            if (integerPart >= 100000)
            {
                long lakhs = integerPart / 100000;
                words += ConvertHundreds(lakhs) + " Lakh ";
                integerPart %= 100000;
            }

            // Process thousands
            if (integerPart >= 1000)
            {
                long thousands = integerPart / 1000;
                words += ConvertHundreds(thousands) + " Thousand ";
                integerPart %= 1000;
            }

            // Process hundreds
            if (integerPart > 0)
            {
                words += ConvertHundreds(integerPart);
            }

            words = words.Trim() + " Rupees";

            // Add paise if present
            if (decimalPart > 0)
            {
                words += " and " + ConvertHundreds(decimalPart) + " Paise";
            }

            return words + " Only";
        }

        private string ConvertHundreds(long number)
        {
            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };
            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string result = "";

            if (number >= 100)
            {
                result = ones[number / 100] + " Hundred ";
                number %= 100;
            }

            if (number >= 20)
            {
                result += tens[number / 10] + " ";
                number %= 10;
            }
            else if (number >= 10)
            {
                result += teens[number - 10] + " ";
                number = 0;
            }

            if (number > 0)
            {
                result += ones[number] + " ";
            }

            return result.Trim();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput()) return;

            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    if (_feeStructureId.HasValue)
                    {
                        // Update existing fee structure
                        var updateDto = new CreateFeeStructureDto
                        {
                            ClassId = (int)cmbClass.SelectedValue,
                            FeeType = (FeeType)cmbFeeType.SelectedValue,
                            Amount = decimal.Parse(txtAmount.Text),
                            AcademicYear = cmbAcademicYear.SelectedItem.ToString(),
                            // Preserve the existing description (no UI field) instead of wiping it on edit
                            Description = _currentFeeStructure?.Description ?? ""
                        };

                        await _feeStructureService.UpdateFeeStructureAsync(_feeStructureId.Value, updateDto);
                        MessageBox.Show("Fee structure updated successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Create new fee structure
                        var createDto = new CreateFeeStructureDto
                        {
                            ClassId = (int)cmbClass.SelectedValue,
                            FeeType = (FeeType)cmbFeeType.SelectedValue,
                            Amount = decimal.Parse(txtAmount.Text),
                            AcademicYear = cmbAcademicYear.SelectedItem.ToString(),
                            // Generate a sensible default description (no UI field for it yet)
                            Description = $"{(FeeType)cmbFeeType.SelectedValue} fee"
                        };

                        await _feeStructureService.CreateFeeStructureAsync(createDto);
                        MessageBox.Show("Fee structure created successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    DialogResult = true;
                    Close();
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("already exists"))
                {
                    ShowValidationError("A fee structure already exists for this class, fee type, and academic year.");
                }
                catch (Exception ex)
                {
                    ShowValidationError($"Error saving fee structure: {ex.Message}");
                }
            }, "Fee Structure Save Error");
        }

        private bool ValidateInput()
        {
            if (cmbAcademicYear.SelectedItem == null)
            {
                ShowValidationError("Please select an academic year.");
                cmbAcademicYear.Focus();
                return false;
            }

            if (!_feeStructureId.HasValue && cmbClass.SelectedValue == null)
            {
                ShowValidationError("Please select a class.");
                cmbClass.Focus();
                return false;
            }

            if (!_feeStructureId.HasValue && cmbFeeType.SelectedValue == null)
            {
                ShowValidationError("Please select a fee type.");
                cmbFeeType.Focus();
                return false;
            }

            if (!decimal.TryParse(txtAmount.Text, out decimal amount) || amount < 0)
            {
                ShowValidationError("Please enter a valid amount (must be 0 or greater).");
                txtAmount.Focus();
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
    }
}