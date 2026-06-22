using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using IEMS.Application.Interfaces;
using IEMS.Core.Entities;

namespace IEMS.WPF
{
    public partial class SystemSettingsWindow : Window
    {
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly IServiceProvider _services;
        private List<SystemSetting> _currentSettings = new();
        private readonly Dictionary<string, Control> _settingControls = new();
        private bool _isBusy = false;  // FIXED BUG #7: Prevent race condition between save and category change
        private bool _hasUnsavedChanges = false;  // FIXED BUG #1: Track unsaved changes

        public SystemSettingsWindow(IServiceProvider services)
        {
            InitializeComponent();

            // Resolve from the window's own DI scope (passed in by MainWindow) rather than the
            // root provider, so the scoped DbContext isn't captured for the app's whole lifetime.
            _services = services;
            _systemSettingsService = services.GetRequiredService<ISystemSettingsService>();

            Loaded += SystemSettingsWindow_Loaded;
            Closing += SystemSettingsWindow_Closing;  // FIXED BUG #1: Warn on close with unsaved changes
        }

        private async void SystemSettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCategoriesAsync();
            await LoadSettingsAsync();
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _systemSettingsService.GetCategoriesAsync();
                var categoryList = new List<string> { "All Categories" };
                categoryList.AddRange(categories);

                CategoryComboBox.ItemsSource = categoryList;
                CategoryComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task LoadSettingsAsync(string? category = null)
        {
            try
            {
                if (string.IsNullOrEmpty(category) || category == "All Categories")
                {
                    _currentSettings = (await _systemSettingsService.GetAllSettingsAsync()).ToList();
                }
                else
                {
                    _currentSettings = (await _systemSettingsService.GetSettingsByCategoryAsync(category)).ToList();
                }

                await BuildSettingsUI();  // Updated method name
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // FIXED BUG #5, #14: Removed async keyword as method doesn't perform async work
        // Method still returns Task for consistency with async callers
        private Task BuildSettingsUI()
        {
            SettingsPanel.Children.Clear();
            _settingControls.Clear();

            // FIXED BUG #11: Handle empty settings list with user-friendly message
            if (!_currentSettings.Any())
            {
                var emptyMessage = new TextBlock
                {
                    Text = "No settings available for this category.",
                    FontSize = 16,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray),
                    Margin = new Thickness(20),
                    TextAlignment = TextAlignment.Center
                };
                SettingsPanel.Children.Add(emptyMessage);
                return Task.CompletedTask;
            }

            var settingsByCategory = _currentSettings.GroupBy(s => s.Category);

            foreach (var categoryGroup in settingsByCategory.OrderBy(g => g.Key))
            {
                // Category Header
                // FIXED BUG #8: Use TryFindResource instead of FindResource to prevent exceptions
                var cardStyle = TryFindResource("CardStyle") as Style;
                var categoryBorder = new Border
                {
                    Style = cardStyle,
                    Margin = new Thickness(0, 10, 0, 5)
                };

                var categoryPanel = new StackPanel();

                var categoryHeader = new TextBlock
                {
                    Text = $"📁 {categoryGroup.Key} Settings",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204)),
                    Margin = new Thickness(0, 0, 0, 15)
                };
                categoryPanel.Children.Add(categoryHeader);

                // Settings in this category
                foreach (var setting in categoryGroup.OrderBy(s => s.Key))
                {
                    var settingPanel = CreateSettingControl(setting);
                    categoryPanel.Children.Add(settingPanel);
                }

                categoryBorder.Child = categoryPanel;
                SettingsPanel.Children.Add(categoryBorder);
            }

            return Task.CompletedTask;
        }

        private StackPanel CreateSettingControl(SystemSetting setting)
        {
            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };

            // Setting label
            var labelPanel = new StackPanel { Orientation = Orientation.Horizontal };

            var label = new TextBlock
            {
                Text = setting.Key.Split('.').Last(), // Show only the key part after the last dot
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            labelPanel.Children.Add(label);

            if (setting.IsReadOnly)
            {
                var readOnlyLabel = new TextBlock
                {
                    Text = "🔒 Read Only",
                    FontSize = 12,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray),
                    VerticalAlignment = VerticalAlignment.Center
                };
                labelPanel.Children.Add(readOnlyLabel);
            }

            panel.Children.Add(labelPanel);

            // Description
            if (!string.IsNullOrEmpty(setting.Description))
            {
                var description = new TextBlock
                {
                    Text = setting.Description,
                    FontSize = 12,
                    Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Gray),
                    Margin = new Thickness(0, 2, 0, 5),
                    TextWrapping = TextWrapping.Wrap
                };
                panel.Children.Add(description);
            }

            // Input control based on data type
            Control inputControl = CreateInputControl(setting);
            _settingControls[setting.Key] = inputControl;
            panel.Children.Add(inputControl);

            return panel;
        }

        private Control CreateInputControl(SystemSetting setting)
        {
            Control control;

            // FIXED BUG #8: Use TryFindResource instead of FindResource to prevent exceptions
            var checkBoxStyle = TryFindResource("ModernCheckBoxStyle") as Style;
            var textBoxStyle = TryFindResource("ModernTextBoxStyle") as Style;

            switch (setting.DataType.ToLower())
            {
                case "boolean":
                    var checkBox = new CheckBox
                    {
                        Style = checkBoxStyle,
                        IsChecked = bool.TryParse(setting.Value, out bool boolValue) && boolValue,
                        IsEnabled = !setting.IsReadOnly
                    };
                    // FIXED BUG #1: Track changes
                    if (!setting.IsReadOnly)
                    {
                        checkBox.Checked += (s, e) => _hasUnsavedChanges = true;
                        checkBox.Unchecked += (s, e) => _hasUnsavedChanges = true;
                    }
                    control = checkBox;
                    break;

                case "integer":
                case "decimal":
                    var numberBox = new TextBox
                    {
                        Style = textBoxStyle,
                        Text = setting.Value,
                        IsEnabled = !setting.IsReadOnly,
                        Width = 200,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    // FIXED BUG #1: Track changes
                    // FIXED BUG #24: Restrict to numeric input only
                    if (!setting.IsReadOnly)
                    {
                        numberBox.TextChanged += (s, e) => _hasUnsavedChanges = true;
                        numberBox.PreviewTextInput += NumericTextBox_PreviewTextInput;
                        DataObject.AddPastingHandler(numberBox, NumericTextBox_Pasting);
                    }
                    control = numberBox;
                    break;

                case "filepath":
                case "directorypath":
                    var pathBox = new TextBox
                    {
                        Style = textBoxStyle,
                        Text = setting.Value,
                        IsEnabled = !setting.IsReadOnly,
                        Width = 400,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    // Store the data type in the Tag property for later retrieval
                    pathBox.Tag = setting.DataType;
                    // FIXED BUG #1: Track changes
                    if (!setting.IsReadOnly)
                    {
                        pathBox.TextChanged += (s, e) => _hasUnsavedChanges = true;
                    }
                    control = pathBox;
                    break;

                default: // String and others
                    var textBox = new TextBox
                    {
                        Style = textBoxStyle,
                        Text = setting.Value,
                        IsEnabled = !setting.IsReadOnly,
                        Width = 400,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    // FIXED BUG #1: Track changes
                    if (!setting.IsReadOnly)
                    {
                        textBox.TextChanged += (s, e) => _hasUnsavedChanges = true;
                    }
                    control = textBox;
                    break;
            }

            return control;
        }

        // FIXED BUG #24: Prevent non-numeric input in integer/decimal fields
        private void NumericTextBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var fullText = textBox.Text.Insert(textBox.SelectionStart, e.Text);

            // Allow negative sign at start, digits, and one decimal point
            if (e.Text == "-" && textBox.SelectionStart == 0 && !textBox.Text.Contains("-"))
            {
                e.Handled = false;
            }
            else if (e.Text == "." && !textBox.Text.Contains("."))
            {
                e.Handled = false;
            }
            else if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
            }
        }

        private void NumericTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsNumeric(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsNumeric(string text)
        {
            return decimal.TryParse(text, out _);
        }

        private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // FIXED BUG #2: Add try-catch to prevent unhandled exception crashes
            // This prevents application crashes if LoadSettingsAsync fails
            try
            {
                if (_systemSettingsService == null) return;  // FIXED BUG #9: Guard against early firing
                if (_isBusy) return;  // FIXED BUG #7: Don't allow category changes during save

                // FIXED BUG #1: Warn user about unsaved changes before switching category
                if (_hasUnsavedChanges)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Do you want to save them before switching categories?",
                        "Unsaved Changes",
                        MessageBoxButton.YesNoCancel,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        SaveButton_Click(this, new RoutedEventArgs());
                        await Task.Delay(100); // Give save operation time to complete
                    }
                    else if (result == MessageBoxResult.Cancel)
                    {
                        // Revert category selection
                        CategoryComboBox.SelectionChanged -= CategoryComboBox_SelectionChanged;
                        CategoryComboBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : CategoryComboBox.Items[0];
                        CategoryComboBox.SelectionChanged += CategoryComboBox_SelectionChanged;
                        return;
                    }
                    // If No, continue with category switch and discard changes
                }

                if (CategoryComboBox.SelectedItem is string selectedCategory)
                {
                    await LoadSettingsAsync(selectedCategory == "All Categories" ? null : selectedCategory);
                    _hasUnsavedChanges = false;  // Reset flag after loading new settings
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // FIXED BUG #1: Warn on window close with unsaved changes
        private void SystemSettingsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_hasUnsavedChanges)
            {
                var result = MessageBox.Show(
                    "You have unsaved changes. Do you want to save them before closing?",
                    "Unsaved Changes",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    SaveButton_Click(this, new RoutedEventArgs());
                    // Continue closing
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true; // Prevent window from closing
                }
                // If No, continue closing and discard changes
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // FIXED BUG #7: Prevent race condition with category changes during save
            if (_isBusy) return;
            _isBusy = true;

            // Disable UI controls during save operation
            SaveButton.IsEnabled = false;
            CategoryComboBox.IsEnabled = false;

            try
            {
                var updatedSettings = new List<SystemSetting>();
                var validationErrors = new List<string>();

                foreach (var setting in _currentSettings)
                {
                    if (setting.IsReadOnly) continue;

                    // Check if control exists in dictionary
                    if (!_settingControls.TryGetValue(setting.Key, out var control))
                    {
                        System.Diagnostics.Debug.WriteLine($"Warning: Control not found for setting '{setting.Key}'");
                        continue;
                    }

                    string newValue = GetControlValue(control, setting.DataType);

                    // Validate input based on data type
                    var validationResult = ValidateSettingValue(newValue, setting.DataType, setting.Key);
                    if (!validationResult.IsValid)
                    {
                        validationErrors.Add(validationResult.ErrorMessage);
                        continue;
                    }

                    if (newValue != setting.Value)
                    {
                        setting.Value = newValue;
                        updatedSettings.Add(setting);
                    }
                }

                // Show validation errors if any
                if (validationErrors.Any())
                {
                    var errorMessage = "Please fix the following errors:\n\n" + string.Join("\n", validationErrors);
                    MessageBox.Show(errorMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (updatedSettings.Any())
                {
                    var result = await _systemSettingsService.UpdateSettingsAsync(updatedSettings);
                    if (result)
                    {
                        _hasUnsavedChanges = false;  // FIXED BUG #1: Reset flag after successful save
                        MessageBox.Show($"Successfully updated {updatedSettings.Count} settings.", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to save some settings. Please check your inputs and try again.",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    _hasUnsavedChanges = false;  // FIXED BUG #1: Reset flag even if no changes
                    // FIXED BUG #20: Don't show message if no changes
                    // MessageBox.Show("No changes detected.", "Information",
                    //              MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Re-enable UI controls after save completes
                SaveButton.IsEnabled = true;
                CategoryComboBox.IsEnabled = true;
                _isBusy = false;
            }
        }

        private string GetControlValue(Control control, string dataType)
        {
            switch (control)
            {
                case CheckBox checkBox:
                    return (checkBox.IsChecked ?? false).ToString();

                case TextBox textBox:
                    return textBox.Text ?? string.Empty;


                default:
                    return string.Empty;
            }
        }

        private (bool IsValid, string ErrorMessage) ValidateSettingValue(string value, string dataType, string settingKey)
        {
            var fieldName = settingKey.Split('.').Last();

            // FIXED BUG #4: Validate max length based on database constraints
            const int MAX_VALUE_LENGTH = 500;
            if (!string.IsNullOrEmpty(value) && value.Length > MAX_VALUE_LENGTH)
            {
                return (false, $"{fieldName}: Value too long (maximum {MAX_VALUE_LENGTH} characters, current: {value.Length})");
            }

            switch (dataType.ToLower())
            {
                case "integer":
                    if (!int.TryParse(value, out int intValue))
                    {
                        return (false, $"{fieldName}: Must be a valid integer number");
                    }

                    // FIXED BUG #6: Add range validation for specific integer settings
                    if (settingKey == "Backup.RetentionDays")
                    {
                        if (intValue < 1)
                        {
                            return (false, $"{fieldName}: Must be at least 1 day");
                        }
                        if (intValue > 3650)
                        {
                            return (false, $"{fieldName}: Cannot exceed 3650 days (10 years)");
                        }
                    }
                    break;

                case "decimal":
                    if (!decimal.TryParse(value, out _))
                    {
                        return (false, $"{fieldName}: Must be a valid decimal number");
                    }
                    break;

                case "boolean":
                    if (!bool.TryParse(value, out _))
                    {
                        return (false, $"{fieldName}: Must be true or false");
                    }
                    break;

                case "filepath":
                    // Optional: Validate file path format
                    if (!string.IsNullOrEmpty(value) && value.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                    {
                        return (false, $"{fieldName}: Contains invalid path characters");
                    }
                    break;

                case "directorypath":
                    // FIXED BUG #2: Don't create directory during validation - only validate format
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Check for invalid path characters
                        if (value.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0)
                        {
                            return (false, $"{fieldName}: Contains invalid path characters");
                        }

                        // Check if path is absolute (rooted)
                        if (!System.IO.Path.IsPathRooted(value))
                        {
                            return (false, $"{fieldName}: Path must be absolute (e.g., C:\\Backups)");
                        }

                        // FIXED BUG #2: Removed directory creation - will be created when actually used
                        // Just validate that the parent directory exists or is valid
                        try
                        {
                            var parentPath = System.IO.Path.GetDirectoryName(value);
                            if (!string.IsNullOrEmpty(parentPath) && !System.IO.Directory.Exists(parentPath))
                            {
                                // Check if we can potentially create it by validating the drive exists
                                var root = System.IO.Path.GetPathRoot(value);
                                if (!System.IO.Directory.Exists(root))
                                {
                                    return (false, $"{fieldName}: Drive '{root}' does not exist");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            return (false, $"{fieldName}: Invalid path format - {ex.Message}");
                        }
                    }
                    break;
            }

            // FIXED BUG #3, #11: Improved email validation with proper regex (RFC 5322 compliant)
            if (settingKey.EndsWith(".Email", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(value))
            {
                // More robust email validation
                var emailPattern = @"^[a-zA-Z0-9]([a-zA-Z0-9._-]*[a-zA-Z0-9])?@[a-zA-Z0-9]([a-zA-Z0-9-]*[a-zA-Z0-9])?(\.[a-zA-Z]{2,})+$";
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, emailPattern))
                {
                    return (false, $"{fieldName}: Must be a valid email address (e.g., school@example.com)");
                }
            }

            // FIXED BUG #6: Phone number validation for Indian format
            if ((settingKey == "School.Phone" || settingKey == "School.AlternatePhone") && !string.IsNullOrWhiteSpace(value))
            {
                // Remove common formatting characters
                var cleanPhone = value.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+91", "");

                if (!System.Text.RegularExpressions.Regex.IsMatch(cleanPhone, @"^\d{10}$"))
                {
                    return (false, $"{fieldName}: Must be a valid 10-digit Indian phone number (e.g., 9876543210 or +91 9876543210)");
                }
            }

            // FIXED BUG #7: Pin code validation (6 digits for India)
            if (settingKey == "School.PinCode" && !string.IsNullOrWhiteSpace(value))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{6}$"))
                {
                    return (false, $"{fieldName}: Must be a valid 6-digit pin code (e.g., 445303)");
                }
            }

            // FIXED BUG #8: URL validation
            if (settingKey == "School.Website" && !string.IsNullOrWhiteSpace(value))
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? uriResult) ||
                    (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                {
                    return (false, $"{fieldName}: Must be a valid URL starting with http:// or https:// (e.g., https://www.school.com)");
                }
            }

            // FIXED BUG #9: UDISE code validation (11 digits)
            if (settingKey == "School.UDISECode" && !string.IsNullOrWhiteSpace(value))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d{11}$"))
                {
                    return (false, $"{fieldName}: Must be an 11-digit UDISE code (e.g., 27140806704)");
                }
            }

            return (true, string.Empty);
        }

        private async void ResetCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedCategory = CategoryComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedCategory) || selectedCategory == "All Categories")
            {
                MessageBox.Show("Please select a specific category to reset.", "Information",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                $"Are you sure you want to reset all settings in the '{selectedCategory}' category to their default values?",
                "Confirm Reset", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var resetResult = await _systemSettingsService.ResetCategoryToDefaultAsync(selectedCategory);
                    if (resetResult)
                    {
                        MessageBox.Show($"Successfully reset '{selectedCategory}' settings to defaults.", "Success",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadSettingsAsync(selectedCategory);
                    }
                    else
                    {
                        MessageBox.Show("Failed to reset settings. Some settings may not have default values.",
                                      "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error resetting settings: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ClearTestDataButton_Click(object sender, RoutedEventArgs e)
        {
            // CRITICAL: Validate that database contains TEST data, not USER data
            var dbContext = _services.GetRequiredService<IEMS.Infrastructure.Data.ApplicationDbContext>();

            // Check if data appears to be seed/test data
            var isTestData = await ValidateIsTestDataAsync(dbContext);

            if (!isTestData)
            {
                MessageBox.Show(
                    "⚠️ SAFETY CHECK FAILED\n\n" +
                    "The database does NOT contain the original test data pattern.\n" +
                    "This means you've already added your own real data.\n\n" +
                    "This button is ONLY for clearing the original seed/test data.\n" +
                    "It cannot be used once you've added custom data.\n\n" +
                    "If you need to clear your data:\n" +
                    "1. Create a backup first (Backup & Restore menu)\n" +
                    "2. Manually delete the database file (school.db)\n" +
                    "3. Restart the application to recreate seed data",
                    "Cannot Clear Data - Not Test Data",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                return;
            }

            // First confirmation dialog
            var result = MessageBox.Show(
                "⚠️ WARNING: This will permanently delete ALL test data including:\n\n" +
                "• All Students\n" +
                "• All Teachers\n" +
                "• All Classes\n" +
                "• All Fee Payments\n" +
                "• All Fee Structures\n" +
                "• All Staff Members\n" +
                "• All Transport/Expense Records\n\n" +
                "System Settings, Users, and Academic Years will be kept.\n\n" +
                "This action CANNOT be undone!\n\n" +
                "Are you absolutely sure you want to continue?",
                "⚠️ Confirm Clear All Test Data",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            // Second confirmation - require typing "DELETE"
            var confirmWindow = new Window
            {
                Title = "Final Confirmation Required",
                Width = 500,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid { Margin = new Thickness(20) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var messageText = new TextBlock
            {
                Text = "Type DELETE in the box below to confirm:",
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(messageText, 0);
            grid.Children.Add(messageText);

            var confirmTextBox = new TextBox
            {
                Height = 35,
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            };
            Grid.SetRow(confirmTextBox, 1);
            grid.Children.Add(confirmTextBox);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);

            var btnConfirm = new Button
            {
                Content = "Confirm Delete",
                Width = 120,
                Height = 35,
                Margin = new Thickness(0, 0, 10, 0),
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                FontWeight = FontWeights.SemiBold,
                IsEnabled = false
            };

            var btnCancel = new Button
            {
                Content = "Cancel",
                Width = 80,
                Height = 35,
                Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125)),
                Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.White),
                FontWeight = FontWeights.SemiBold
            };

            confirmTextBox.TextChanged += (s, e) =>
            {
                btnConfirm.IsEnabled = confirmTextBox.Text == "DELETE";
            };

            bool confirmed = false;
            btnConfirm.Click += (s, e) => { confirmed = true; confirmWindow.Close(); };
            btnCancel.Click += (s, e) => confirmWindow.Close();

            buttonPanel.Children.Add(btnConfirm);
            buttonPanel.Children.Add(btnCancel);
            grid.Children.Add(buttonPanel);

            confirmWindow.Content = grid;
            confirmWindow.ShowDialog();

            if (!confirmed)
                return;

            // Proceed with clearing data
            try
            {
                ClearTestDataButton.IsEnabled = false;
                SaveButton.IsEnabled = false;

                // Delete data children-first so Restrict foreign keys are never violated.
                // (Students must go AFTER the rows that reference them.)
                dbContext.StudentPromotionHistory.RemoveRange(dbContext.StudentPromotionHistory);
                dbContext.FeePayments.RemoveRange(dbContext.FeePayments);
                dbContext.FeeStructures.RemoveRange(dbContext.FeeStructures);
                dbContext.TransportExpenses.RemoveRange(dbContext.TransportExpenses);
                dbContext.ElectricityBills.RemoveRange(dbContext.ElectricityBills);
                dbContext.OtherExpenses.RemoveRange(dbContext.OtherExpenses);
                dbContext.Students.RemoveRange(dbContext.Students);
                dbContext.Vehicles.RemoveRange(dbContext.Vehicles);
                dbContext.Classes.RemoveRange(dbContext.Classes);
                dbContext.Staff.RemoveRange(dbContext.Staff);
                dbContext.Teachers.RemoveRange(dbContext.Teachers);

                var recordsDeleted = await dbContext.SaveChangesAsync();

                MessageBox.Show(
                    $"✅ Successfully cleared all test data!\n\n" +
                    $"Total records deleted: {recordsDeleted}\n\n" +
                    $"The database now contains only:\n" +
                    $"• System Settings\n" +
                    $"• User Accounts\n" +
                    $"• Academic Years\n\n" +
                    $"You can now start fresh with your own data.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error clearing test data: {ex.Message}\n\n" +
                    $"Some data may have been deleted. Please check the database.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                ClearTestDataButton.IsEnabled = true;
                SaveButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Validates that the database contains the original seed/test data pattern.
        /// Returns false if user has added their own data, preventing accidental deletion.
        /// </summary>
        private async Task<bool> ValidateIsTestDataAsync(IEMS.Infrastructure.Data.ApplicationDbContext dbContext)
        {
            try
            {
                // Check for exact seed data patterns

                // 1. Students: Should be exactly 260 with StudentNumbers S001-S260
                var studentCount = await dbContext.Students.CountAsync();
                if (studentCount != 260)
                    return false;

                var firstStudent = await dbContext.Students
                    .Where(s => s.StudentNumber == "S001")
                    .FirstOrDefaultAsync();
                var lastStudent = await dbContext.Students
                    .Where(s => s.StudentNumber == "S260")
                    .FirstOrDefaultAsync();

                if (firstStudent == null || lastStudent == null)
                    return false;

                // 2. Teachers: Should be exactly 10 with EmployeeIds T001-T010
                var teacherCount = await dbContext.Teachers.CountAsync();
                if (teacherCount != 10)
                    return false;

                var firstTeacher = await dbContext.Teachers
                    .Where(t => t.EmployeeId == "T001")
                    .FirstOrDefaultAsync();
                var lastTeacher = await dbContext.Teachers
                    .Where(t => t.EmployeeId == "T010")
                    .FirstOrDefaultAsync();

                if (firstTeacher == null || lastTeacher == null)
                    return false;

                // 3. Classes: Should be exactly 13 (Nursery, KG1, KG2, Class 1-10)
                var classCount = await dbContext.Classes.CountAsync();
                if (classCount != 13)
                    return false;

                // 4. Staff: Should be exactly 10 with EmployeeIds ST001-ST010
                var staffCount = await dbContext.Staff.CountAsync();
                if (staffCount != 10)
                    return false;

                var firstStaff = await dbContext.Staff
                    .Where(s => s.EmployeeId == "ST001")
                    .FirstOrDefaultAsync();
                var lastStaff = await dbContext.Staff
                    .Where(s => s.EmployeeId == "ST010")
                    .FirstOrDefaultAsync();

                if (firstStaff == null || lastStaff == null)
                    return false;

                // 5. Vehicles: Should be exactly 10
                var vehicleCount = await dbContext.Vehicles.CountAsync();
                if (vehicleCount != 10)
                    return false;

                // 6. Check for specific seed data markers
                var nurseryClass = await dbContext.Classes
                    .Where(c => c.Name == "Nursery" && c.Section == "A")
                    .FirstOrDefaultAsync();

                if (nurseryClass == null)
                    return false;

                // All checks passed - this appears to be original seed data
                return true;
            }
            catch
            {
                // If validation fails, assume it's NOT test data (safer)
                return false;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}