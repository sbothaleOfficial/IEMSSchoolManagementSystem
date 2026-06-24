using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using IEMS.Application.DTOs;
using IEMS.Application.Services;
using IEMS.WPF.Helpers;

namespace IEMS.WPF
{
    public partial class AcademicYearManagementWindow : Window
    {
        private readonly AcademicYearService _academicYearService;
        private List<AcademicYearDto> _academicYears = new();
        private AcademicYearDto? _editingAcademicYear;
        private static readonly Regex YearFormatRegex = new Regex(@"^\d{4}-\d{2}$", RegexOptions.Compiled);

        public AcademicYearManagementWindow(AcademicYearService academicYearService)
        {
            InitializeComponent();
            _academicYearService = academicYearService;
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    await LoadAcademicYears();
                    lblStatus.Text = "Academic Year Management Ready";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading Academic Year Management: {ex.Message}\n\n{ex.StackTrace}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, "Academic Year Management Window Loading Error");
        }

        private async System.Threading.Tasks.Task LoadAcademicYears()
        {
            try
            {
                ShowLoading(true);
                _academicYears = (await _academicYearService.GetAllAcademicYearsAsync())
                    .OrderByDescending(ay => ay.StartDate)
                    .ToList();
                dgAcademicYears.ItemsSource = _academicYears;
                lblStatus.Text = $"Loaded {_academicYears.Count} academic year(s)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading academic years: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Error loading academic years";
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void ShowLoading(bool show)
        {
            LoadingOverlay.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ClearForm()
        {
            _editingAcademicYear = null;
            txtYear.Text = "";
            dpStartDate.SelectedDate = null;
            dpEndDate.SelectedDate = null;
            chkIsCurrent.IsChecked = false;
            lblFormTitle.Text = "Add New Academic Year";
            HideValidationMessage();
        }

        private void ShowValidationMessage(string message)
        {
            lblValidationMessage.Text = message;
            pnlValidationMessage.Visibility = Visibility.Visible;
        }

        private void HideValidationMessage()
        {
            pnlValidationMessage.Visibility = Visibility.Collapsed;
        }

        private bool ValidateForm()
        {
            // Validate Year
            if (string.IsNullOrWhiteSpace(txtYear.Text))
            {
                ShowValidationMessage("Academic year is required.");
                txtYear.Focus();
                return false;
            }

            if (!YearFormatRegex.IsMatch(txtYear.Text))
            {
                ShowValidationMessage("Invalid year format. Expected: YYYY-YY (e.g., 2024-25)");
                txtYear.Focus();
                return false;
            }

            // Validate consecutive years
            var parts = txtYear.Text.Split('-');
            if (parts.Length == 2)
            {
                if (int.TryParse(parts[0], out int startYear) && int.TryParse(parts[1], out int endYearShort))
                {
                    int expectedEndYear = (startYear + 1) % 100;
                    if (endYearShort != expectedEndYear)
                    {
                        ShowValidationMessage($"Years must be consecutive (e.g., 2024-25, not 2024-26)");
                        txtYear.Focus();
                        return false;
                    }
                }
            }

            // Validate Start Date
            if (!dpStartDate.SelectedDate.HasValue)
            {
                ShowValidationMessage("Start date is required.");
                dpStartDate.Focus();
                return false;
            }

            // Validate End Date
            if (!dpEndDate.SelectedDate.HasValue)
            {
                ShowValidationMessage("End date is required.");
                dpEndDate.Focus();
                return false;
            }

            // Validate date range
            if (dpEndDate.SelectedDate.Value <= dpStartDate.SelectedDate.Value)
            {
                ShowValidationMessage("End date must be after start date.");
                dpEndDate.Focus();
                return false;
            }

            // Validate duration (9-15 months)
            var duration = dpEndDate.SelectedDate.Value - dpStartDate.SelectedDate.Value;
            if (duration.TotalDays < 270 || duration.TotalDays > 450)
            {
                ShowValidationMessage($"Academic year duration must be between 9 and 15 months. Current: {duration.TotalDays:F0} days");
                dpEndDate.Focus();
                return false;
            }

            // Check for duplicate year (excluding current record if editing)
            var duplicate = _academicYears.FirstOrDefault(ay =>
                ay.Year == txtYear.Text && ay.Id != (_editingAcademicYear?.Id ?? 0));
            if (duplicate != null)
            {
                ShowValidationMessage($"Academic year '{txtYear.Text}' already exists.");
                txtYear.Focus();
                return false;
            }

            return true;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();

            // Auto-suggest next academic year
            if (_academicYears.Any())
            {
                var latestYear = _academicYears.OrderByDescending(ay => ay.StartDate).First();
                var yearParts = latestYear.Year.Split('-');
                if (yearParts.Length == 2 && int.TryParse(yearParts[0], out int year))
                {
                    var nextYear = year + 1;
                    txtYear.Text = $"{nextYear}-{(nextYear + 1).ToString().Substring(2)}";
                    var start = latestYear.EndDate.AddDays(1);
                    dpStartDate.SelectedDate = start;
                    // End on the day before the one-year anniversary (e.g. 01-Jun-2026 → 31-May-2027)
                    // so the suggested span matches the school's academic year.
                    dpEndDate.SelectedDate = start.AddYears(1).AddDays(-1);
                }
            }
            else
            {
                // No years yet: suggest the Indian academic year (June–May) containing today.
                var now = DateTime.Now;
                var startYear = now.Month >= 6 ? now.Year : now.Year - 1;
                txtYear.Text = $"{startYear}-{(startYear + 1).ToString().Substring(2)}";
                dpStartDate.SelectedDate = new DateTime(startYear, 6, 1);
                dpEndDate.SelectedDate = new DateTime(startYear + 1, 5, 31);
            }

            txtYear.Focus();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            AsyncHelper.SafeFireAndForget(async () =>
            {
                lblStatus.Text = "Refreshing...";
                await LoadAcademicYears();
                ClearForm();
            }, "Error refreshing academic years");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
                return;

            AsyncHelper.SafeFireAndForget(async () =>
            {
                try
                {
                    ShowLoading(true);

                    var dto = new AcademicYearDto
                    {
                        Id = _editingAcademicYear?.Id ?? 0,
                        Year = txtYear.Text,
                        StartDate = dpStartDate.SelectedDate!.Value,
                        EndDate = dpEndDate.SelectedDate!.Value,
                        IsCurrent = chkIsCurrent.IsChecked ?? false
                    };

                    if (_editingAcademicYear == null)
                    {
                        await _academicYearService.AddAcademicYearAsync(dto);
                        MessageBox.Show("Academic year added successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        await _academicYearService.UpdateAcademicYearAsync(dto);
                        MessageBox.Show("Academic year updated successfully.", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    await LoadAcademicYears();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving academic year: {ex.Message}", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    ShowLoading(false);
                }
            }, "Error saving academic year");
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgAcademicYears.SelectedItem is AcademicYearDto academicYear)
            {
                EditAcademicYear(academicYear);
            }
        }

        private void EditAcademicYear(AcademicYearDto academicYear)
        {
            _editingAcademicYear = academicYear;
            lblFormTitle.Text = "Edit Academic Year";
            txtYear.Text = academicYear.Year;
            dpStartDate.SelectedDate = academicYear.StartDate;
            dpEndDate.SelectedDate = academicYear.EndDate;
            chkIsCurrent.IsChecked = academicYear.IsCurrent;
            HideValidationMessage();
            txtYear.Focus();
        }

        private void BtnSetCurrent_Click(object sender, RoutedEventArgs e)
        {
            if (dgAcademicYears.SelectedItem is AcademicYearDto academicYear)
            {
                var result = MessageBox.Show(
                    $"Set '{academicYear.Year}' as the current academic year?\n\n" +
                    "This will unset any other current academic year.",
                    "Confirm Set Current",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AsyncHelper.SafeFireAndForget(async () =>
                    {
                        try
                        {
                            ShowLoading(true);
                            await _academicYearService.SetCurrentAcademicYearAsync(academicYear.Id);
                            MessageBox.Show("Current academic year updated successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadAcademicYears();
                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error setting current academic year: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            ShowLoading(false);
                        }
                    }, "Error setting current academic year");
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgAcademicYears.SelectedItem is AcademicYearDto academicYear)
            {
                if (academicYear.IsCurrent)
                {
                    MessageBox.Show("Cannot delete the current academic year. Please set another year as current first.",
                        "Cannot Delete", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    $"Are you sure you want to delete the academic year '{academicYear.Year}'?\n\n" +
                    $"Start Date: {academicYear.StartDate:dd/MM/yyyy}\n" +
                    $"End Date: {academicYear.EndDate:dd/MM/yyyy}\n\n" +
                    "This action cannot be undone.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    AsyncHelper.SafeFireAndForget(async () =>
                    {
                        try
                        {
                            ShowLoading(true);
                            await _academicYearService.DeleteAcademicYearAsync(academicYear.Id);
                            MessageBox.Show("Academic year deleted successfully.", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                            await LoadAcademicYears();
                            ClearForm();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error deleting academic year: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            ShowLoading(false);
                        }
                    }, "Error deleting academic year");
                }
            }
        }

        private void DgAcademicYears_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedYear = dgAcademicYears.SelectedItem as AcademicYearDto;
            bool hasSelection = selectedYear != null;

            // Enable/disable toolbar buttons based on selection
            btnEdit.IsEnabled = hasSelection;
            btnDelete.IsEnabled = hasSelection;

            // Set Current button only enabled if selection exists and is not already current
            btnSetCurrent.IsEnabled = hasSelection && selectedYear?.IsCurrent == false;
        }

        private void DgAcademicYears_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgAcademicYears.SelectedItem is AcademicYearDto academicYear)
            {
                EditAcademicYear(academicYear);
            }
        }

        private void TxtYear_TextChanged(object sender, TextChangedEventArgs e)
        {
            HideValidationMessage();
        }

        private void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            HideValidationMessage();

            // Auto-suggest end date (10 months after start date)
            if (dpStartDate.SelectedDate.HasValue && !dpEndDate.SelectedDate.HasValue)
            {
                dpEndDate.SelectedDate = dpStartDate.SelectedDate.Value.AddMonths(10);
            }
        }
    }

    // Value Converters for Academic Year Status
    public class CurrentStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrent)
            {
                return isCurrent ? new SolidColorBrush(Color.FromRgb(16, 185, 129)) : // Success green
                                  new SolidColorBrush(Color.FromRgb(156, 163, 175)); // Gray
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CurrentStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isCurrent)
            {
                return isCurrent ? "Current" : "Inactive";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibility)
            {
                return visibility != Visibility.Visible;
            }
            return false;
        }
    }
}
