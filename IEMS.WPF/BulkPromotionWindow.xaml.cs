using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.WPF.Helpers;

namespace IEMS.WPF
{
    public partial class BulkPromotionWindow : Window
    {
        private readonly BulkPromotionService _bulkPromotionService;
        private readonly ClassService _classService;
        private readonly AcademicYearService _academicYearService;
        private List<StudentPromotionViewModel> _currentPreview = new();
        private List<ClassDto> _allClasses = new();

        public BulkPromotionWindow(BulkPromotionService bulkPromotionService, ClassService classService, AcademicYearService academicYearService)
        {
            InitializeComponent();
            _bulkPromotionService = bulkPromotionService;
            _classService = classService;
            _academicYearService = academicYearService;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("=== BulkPromotionWindow.Window_Loaded() CALLED ===");
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    await LoadInitialDataAsync();
                    lblStatus.Text = "Bulk Promotion Ready";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading bulk promotion data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    lblStatus.Text = "Error loading data";
                }
            }, "Bulk Promotion Window Loading Error");
        }

        private async Task LoadInitialDataAsync()
        {
            Console.WriteLine("=== SIMPLIFIED IMPLEMENTATION - LoadInitialDataAsync() ===");

            // Just load academic years - no class dropdown complexity needed
            await LoadExtendedAcademicYearsAsync();

            // Set placeholder text for class inputs
            txtFromClass.Text = "";
            txtToClass.Text = "";

            Console.WriteLine("=== Initialization complete - using simple text inputs ===");
        }


        private async Task LoadExtendedAcademicYearsAsync()
        {
            try
            {
                // Load from database
                var academicYears = await _academicYearService.GetAllAcademicYearsAsync();
                var yearsList = academicYears.OrderByDescending(ay => ay.StartDate).Select(ay => new SimpleAcademicYear
                {
                    Year = ay.Year,
                    IsCurrent = ay.IsCurrent
                }).ToList();

                cmbAcademicYear.ItemsSource = yearsList;

                // Use centralized method to get current academic year
                var currentAcademicYear = await _academicYearService.GetCurrentAcademicYearAsync();
                if (currentAcademicYear != null)
                {
                    cmbAcademicYear.SelectedItem = yearsList.FirstOrDefault(ay => ay.Year == currentAcademicYear.Year);
                }
            }
            catch (Exception)
            {
                // Fallback: Generate years dynamically
                var currentYear = DateTime.Now.Year;
                var academicYears = new List<SimpleAcademicYear>();

                // Add past 10 years
                for (int i = 10; i >= 1; i--)
                {
                    var year = currentYear - i;
                    academicYears.Add(new SimpleAcademicYear
                    {
                        Year = $"{year}-{(year + 1).ToString().Substring(2)}",
                        IsCurrent = false
                    });
                }

                // Add current year
                academicYears.Add(new SimpleAcademicYear
                {
                    Year = $"{currentYear}-{(currentYear + 1).ToString().Substring(2)}",
                    IsCurrent = true
                });

                // Add future 5 years
                for (int i = 1; i <= 5; i++)
                {
                    var year = currentYear + i;
                    academicYears.Add(new SimpleAcademicYear
                    {
                        Year = $"{year}-{(year + 1).ToString().Substring(2)}",
                        IsCurrent = false
                    });
                }

                cmbAcademicYear.ItemsSource = academicYears;
                cmbAcademicYear.SelectedItem = academicYears.FirstOrDefault(ay => ay.IsCurrent);
            }
        }

        private void TxtFromClass_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePromotionInputs();
        }

        private void TxtToClass_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidatePromotionInputs();
        }

        private void ValidatePromotionInputs()
        {
            var fromClass = txtFromClass.Text?.Trim();
            var toClass = txtToClass.Text?.Trim();

            var hasValidInput = !string.IsNullOrWhiteSpace(fromClass) &&
                               !string.IsNullOrWhiteSpace(toClass) &&
                               cmbAcademicYear.SelectedItem != null &&
                               !fromClass.Equals(toClass, StringComparison.OrdinalIgnoreCase);

            btnLoadPreview.IsEnabled = hasValidInput;
            btnExecutePromotion.IsEnabled = false;

            if (!string.IsNullOrWhiteSpace(fromClass) && !string.IsNullOrWhiteSpace(toClass) &&
                fromClass.Equals(toClass, StringComparison.OrdinalIgnoreCase))
            {
                lblStatus.Text = "Source and target classes must be different";
            }
            else if (hasValidInput)
            {
                lblStatus.Text = "Ready to load preview";
            }
            else
            {
                lblStatus.Text = "Please enter source class, target class, and select academic year";
            }
        }

        private void BtnLoadPreview_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(LoadPreviewAsync);
        }

        private async Task LoadPreviewAsync()
        {
            try
            {
                var fromClassName = txtFromClass.Text?.Trim();
                var toClassName = txtToClass.Text?.Trim();

                if (string.IsNullOrWhiteSpace(fromClassName) || string.IsNullOrWhiteSpace(toClassName))
                {
                    MessageBox.Show("Please enter both source and target class names.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                lblStatus.Text = "Loading promotion preview...";

                // For simplicity, we'll use class names directly - the service can handle name-based lookups
                var fromClassId = GetClassIdByName(fromClassName);
                var toClassId = GetClassIdByName(toClassName);

                var preview = await _bulkPromotionService.GetPromotionPreviewAsync(fromClassId, toClassId);

                _currentPreview = preview.Select(p => new StudentPromotionViewModel
                {
                    StudentId = p.StudentId,
                    StudentName = p.StudentName,
                    StudentNumber = p.StudentNumber,
                    CurrentClass = p.CurrentClass,
                    TargetClass = p.TargetClass,
                    IsEligible = p.IsEligible,
                    IneligibilityReason = p.IneligibilityReason,
                    HasPendingFees = p.HasPendingFees,
                    PendingAmount = p.PendingAmount,
                    IsExcluded = false
                }).ToList();

                UpdatePreviewGrid();
                UpdateSummaryCards();

                btnExecutePromotion.IsEnabled = _currentPreview.Any(p => p.IsEligible && !p.IsExcluded);
                lblStatus.Text = $"Loaded {_currentPreview.Count} students for preview";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preview: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading preview";
            }
        }

        private int GetClassIdByName(string className)
        {
            // Resolve against the actually-loaded classes rather than a hard-coded ID map
            // (the map assumed contiguous seed IDs and broke for any other data set).
            if (string.IsNullOrWhiteSpace(className))
                return -1;

            var match = _allClasses.FirstOrDefault(c =>
                string.Equals(c.Name, className, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.DisplayName, className, StringComparison.OrdinalIgnoreCase));

            return match?.Id ?? -1;
        }

        private void UpdatePreviewGrid()
        {
            var filteredStudents = _currentPreview.AsEnumerable();

            if (chkShowOnlyEligible.IsChecked == true)
            {
                filteredStudents = filteredStudents.Where(s => s.IsEligible);
            }

            if (chkShowPendingFees.IsChecked == true)
            {
                filteredStudents = filteredStudents.Where(s => s.HasPendingFees);
            }

            dgPromotionPreview.ItemsSource = filteredStudents.ToList();
        }

        private void UpdateSummaryCards()
        {
            var total = _currentPreview.Count;
            var eligible = _currentPreview.Count(p => p.IsEligible && !p.IsExcluded);
            var excluded = _currentPreview.Count(p => p.IsExcluded);

            txtTotalStudents.Text = total.ToString();
            txtEligibleStudents.Text = eligible.ToString();
            txtExcludedStudents.Text = excluded.ToString();
        }

        private void BtnExecutePromotion_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(ExecutePromotionAsync);
        }

        private async Task ExecutePromotionAsync()
        {
            try
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to promote {_currentPreview.Count(p => p.IsEligible && !p.IsExcluded)} students?\n\nThis action cannot be easily undone.",
                    "Confirm Bulk Promotion",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                lblStatus.Text = "Executing bulk promotion...";
                btnExecutePromotion.IsEnabled = false;

                var fromClassName = txtFromClass.Text?.Trim();
                var toClassName = txtToClass.Text?.Trim();

                var selectedAcademicYear = cmbAcademicYear.SelectedItem as SimpleAcademicYear;

                var request = new BulkPromotionRequest
                {
                    FromClassId = GetClassIdByName(fromClassName!),
                    ToClassId = GetClassIdByName(toClassName!),
                    AcademicYear = selectedAcademicYear!.Year,
                    ExcludedStudentIds = _currentPreview.Where(p => p.IsExcluded).Select(p => p.StudentId).ToList(),
                    Reason = "Annual Promotion",
                    PromotedBy = LoginWindow.CurrentUser?.Username,
                    Remarks = "Bulk promotion via Bulk Promotion Window"
                };

                var promotionResult = await _bulkPromotionService.ExecuteBulkPromotionAsync(request);

                // Show results
                DisplayPromotionResults(promotionResult);

                lblStatus.Text = promotionResult.IsSuccess
                    ? $"Successfully promoted {promotionResult.PromotedStudents} students"
                    : $"Promotion completed with {promotionResult.FailedPromotions} errors";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error executing promotion: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error executing promotion";
                btnExecutePromotion.IsEnabled = true;
            }
        }

        private void DisplayPromotionResults(BulkPromotionResult result)
        {
            tabResults.IsSelected = true;
            borderResultsSummary.Visibility = Visibility.Visible;

            txtResultsSummary.Text = $"Promoted: {result.PromotedStudents} | Failed: {result.FailedPromotions} | Total: {result.TotalStudents}";
            txtPromotionDate.Text = $"Promotion Date: {result.PromotionDate:yyyy-MM-dd HH:mm:ss} | Academic Year: {result.AcademicYear}";

            dgPromotionErrors.ItemsSource = result.Errors;

            if (result.IsSuccess)
            {
                borderResultsSummary.Background = new SolidColorBrush(Color.FromRgb(232, 245, 233)); // Light green
                MessageBox.Show($"Bulk promotion completed successfully!\n\nPromoted: {result.PromotedStudents} students",
                               "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                borderResultsSummary.Background = new SolidColorBrush(Color.FromRgb(255, 243, 224)); // Light orange
                MessageBox.Show($"Bulk promotion completed with errors.\n\nPromoted: {result.PromotedStudents}\nFailed: {result.FailedPromotions}\n\nCheck the Results tab for details.",
                               "Partial Success", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ChkShowOnlyEligible_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePreviewGrid();
        }

        private void ChkShowOnlyEligible_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePreviewGrid();
        }

        private void ChkShowPendingFees_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePreviewGrid();
        }

        private void ChkShowPendingFees_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdatePreviewGrid();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class StudentPromotionViewModel : INotifyPropertyChanged
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
        public string CurrentClass { get; set; } = string.Empty;
        public string TargetClass { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public string IneligibilityReason { get; set; } = string.Empty;
        public bool HasPendingFees { get; set; }
        public decimal PendingAmount { get; set; }

        private bool _isExcluded;
        public bool IsExcluded
        {
            get => _isExcluded;
            set
            {
                if (_isExcluded != value)
                {
                    _isExcluded = value;
                    OnPropertyChanged(nameof(IsExcluded));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SimpleAcademicYear
    {
        public string Year { get; set; } = string.Empty;  // e.g., "2024-25"
        public bool IsCurrent { get; set; }
    }
}