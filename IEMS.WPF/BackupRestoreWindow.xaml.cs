using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using IEMS.Application.Services;

namespace IEMS.WPF
{
    public partial class BackupRestoreWindow : Window
    {
        private readonly IBackupService _backupService;
        private readonly IServiceProvider _services;
        private List<BackupInfoViewModel> _backupHistory;
        private string _selectedBackupPath = string.Empty;

        public BackupRestoreWindow(IServiceProvider services)
        {
            InitializeComponent();
            // Resolve from the window's own DI scope (passed in by MainWindow) rather than the
            // root provider, so scoped services aren't captured for the app's whole lifetime.
            _services = services;
            _backupService = services.GetRequiredService<IBackupService>();
            Loaded += Window_Loaded;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadBackupHistory();
            await UpdateDatabaseInfo();
            await LoadScheduleSettings();
            await RefreshGoogleDriveStatus();
        }

        /// <summary>If a cloud/Drive backup folder is already configured, show it on the card.</summary>
        private async Task RefreshGoogleDriveStatus()
        {
            try
            {
                var settings = _services.GetRequiredService<IEMS.Application.Interfaces.ISystemSettingsService>();
                var configured = await settings.GetSettingValueAsync("Backup.BackupPath");
                if (!string.IsNullOrWhiteSpace(configured))
                    GoogleDriveStatusText.Text =
                        $"✅ Backups go to: {configured}\nGoogle Drive for Desktop uploads them automatically while it is running.";
            }
            catch { /* non-fatal: leave the default hint text */ }
        }

        private async Task UpdateDatabaseInfo()
        {
            try
            {
                // FIXED BUG #1: Use Directory.GetCurrentDirectory() to match Entity Framework and BackupService
                var dbPath = IEMS.Core.Configuration.DatabaseLocation.DatabaseFilePath;
                if (File.Exists(dbPath))
                {
                    var fileInfo = new FileInfo(dbPath);
                    var sizeInMB = fileInfo.Length / (1024.0 * 1024.0);
                    DatabaseSizeText.Text = $"Database Size: {sizeInMB:F2} MB";
                }

                var history = await _backupService.GetBackupHistoryAsync();
                if (history.Any())
                {
                    var lastBackup = history.OrderByDescending(b => b.Timestamp).First();
                    LastBackupText.Text = $"Last Backup: {lastBackup.Timestamp:g}";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load database info: {ex.Message}");
            }
        }

        private async Task LoadBackupHistory()
        {
            try
            {
                var history = await _backupService.GetBackupHistoryAsync();
                _backupHistory = history.Select(h => new BackupInfoViewModel
                {
                    Id = h.Id,
                    Type = h.Type,
                    Timestamp = h.Timestamp,
                    BackupPath = h.BackupPath,
                    FileSize = h.FileSize,
                    FileSizeFormatted = FormatFileSize(h.FileSize)
                }).ToList();

                BackupHistoryDataGrid.ItemsSource = _backupHistory;

                // Update summary
                if (_backupHistory.Any())
                {
                    TotalBackupsText.Text = $"Total Backups: {_backupHistory.Count}";
                    var totalSize = _backupHistory.Sum(b => b.FileSize);
                    TotalSizeText.Text = $"Total Size: {FormatFileSize(totalSize)}";
                    var oldest = _backupHistory.Min(b => b.Timestamp);
                    OldestBackupText.Text = $"Oldest: {oldest:d}";
                }
                else
                {
                    TotalBackupsText.Text = "Total Backups: 0";
                    TotalSizeText.Text = "Total Size: 0 MB";
                    OldestBackupText.Text = "Oldest: N/A";
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load backup history: {ex.Message}");
            }
        }

        private async Task LoadScheduleSettings()
        {
            try
            {
                var schedulePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "IEMS_Backups",
                    "backup_schedule.json");

                if (File.Exists(schedulePath))
                {
                    var json = await File.ReadAllTextAsync(schedulePath);
                    var schedule = System.Text.Json.JsonSerializer.Deserialize<BackupSchedule>(json);

                    if (schedule != null)
                    {
                        EnableDailyBackupCheckBox.IsChecked = schedule.EnableDaily;
                        EnableWeeklyBackupCheckBox.IsChecked = schedule.EnableWeekly;
                        EnableMonthlyBackupCheckBox.IsChecked = schedule.EnableMonthly;
                        EnableIncrementalBackupCheckBox.IsChecked = schedule.EnableIncremental;

                        // Set the time ComboBox to match the saved time
                        SetTimeComboBox(schedule.DailyTime);
                        SetWeeklyDayComboBox(schedule.WeeklyDay);
                        SetMonthlyDayComboBox(schedule.MonthlyDay);

                        UpdateNextBackupInfo();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to load schedule settings: {ex.Message}");
            }
        }

        private async void BackupToDesktopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BackupStatusText.Text = "Creating backup to Desktop...";
                BackupToDesktopButton.IsEnabled = false;

                var result = await _backupService.CreateBackupAsync(BackupType.Manual);

                if (result.Success)
                {
                    BackupStatusText.Text = $"✅ Backup created successfully!\n" +
                                           $"Location: {result.BackupPath}\n" +
                                           $"Size: {FormatFileSize(result.BackupInfo.FileSize)}\n" +
                                           $"Timestamp: {result.BackupInfo.Timestamp:G}";

                    var openFolder = MessageBox.Show(
                        "Backup created successfully! Do you want to open the backup folder?",
                        "Success",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (openFolder == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer.exe", $"/select,\"{result.BackupPath}\"");
                    }

                    await LoadBackupHistory();
                    await UpdateDatabaseInfo();
                }
                else
                {
                    BackupStatusText.Text = $"❌ Backup failed: {result.Message}";
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                BackupStatusText.Text = $"❌ Error: {ex.Message}";
                ShowError($"Backup failed: {ex.Message}");
            }
            finally
            {
                BackupToDesktopButton.IsEnabled = true;
            }
        }

        private async void BackupToCustomButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Title = "Save Database Backup",
                    Filter = "Database Backup Files (*.db)|*.db|All Files (*.*)|*.*",
                    FileName = $"school_backup_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.db",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    OverwritePrompt = true
                };

                if (saveDialog.ShowDialog() == true)
                {
                    var customPath = Path.GetDirectoryName(saveDialog.FileName);
                    BackupStatusText.Text = $"Creating backup to {customPath}...";
                    BackupToCustomButton.IsEnabled = false;

                    var result = await _backupService.CreateBackupAsync(BackupType.Manual, customPath);

                    if (result.Success)
                    {
                        BackupStatusText.Text = $"✅ Backup created successfully!\n" +
                                               $"Location: {result.BackupPath}\n" +
                                               $"Size: {FormatFileSize(result.BackupInfo.FileSize)}\n" +
                                               $"Timestamp: {result.BackupInfo.Timestamp:G}";

                        MessageBox.Show(
                            $"Backup created successfully at:\n{result.BackupPath}",
                            "Success",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        await LoadBackupHistory();
                        await UpdateDatabaseInfo();
                    }
                    else
                    {
                        BackupStatusText.Text = $"❌ Backup failed: {result.Message}";
                        ShowError(result.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                BackupStatusText.Text = $"❌ Error: {ex.Message}";
                ShowError($"Backup failed: {ex.Message}");
            }
            finally
            {
                BackupToCustomButton.IsEnabled = true;
            }
        }

        private void OpenBackupFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var backupPath = Path.Combine(desktopPath, "IEMS_Backups");

                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                Process.Start("explorer.exe", backupPath);
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open backup folder: {ex.Message}");
            }
        }

        private async void SelectBackupFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Backup File",
                    Filter = "Database Backup Files (*.db)|*.db|All Files (*.*)|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _selectedBackupPath = openFileDialog.FileName;
                    SelectedBackupFileText.Text = Path.GetFileName(_selectedBackupPath);

                    // Validate the selected file
                    await ValidateBackupFile(_selectedBackupPath);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to select backup file: {ex.Message}");
            }
        }

        private async Task ValidateBackupFile(string path)
        {
            try
            {
                RestoreStatusText.Text = "Validating backup file...";

                // Check file exists
                if (File.Exists(path))
                {
                    ValidationFileExistsText.Text = "✓ Backup file exists";
                    ValidationFileExistsText.Foreground = System.Windows.Media.Brushes.Green;

                    // Validate SQLite header
                    var header = new byte[16];
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                    {
                        await fs.ReadAsync(header, 0, 16);
                    }

                    var headerString = System.Text.Encoding.ASCII.GetString(header);
                    if (headerString.StartsWith("SQLite format 3"))
                    {
                        ValidationFileValidText.Text = "✓ Valid SQLite database";
                        ValidationFileValidText.Foreground = System.Windows.Media.Brushes.Green;
                    }
                    else
                    {
                        ValidationFileValidText.Text = "✗ Invalid database file";
                        ValidationFileValidText.Foreground = System.Windows.Media.Brushes.Red;
                        RestoreButton.IsEnabled = false;
                        return;
                    }

                    // Show file date
                    var fileInfo = new FileInfo(path);
                    ValidationFileDateText.Text = $"Backup date: {fileInfo.LastWriteTime:G}";

                    // Check if backup is older than current
                    // FIXED BUG #1: Use Directory.GetCurrentDirectory() to match Entity Framework and BackupService
                    var currentDbPath = Path.Combine(Directory.GetCurrentDirectory(), "school.db");
                    if (File.Exists(currentDbPath))
                    {
                        var currentInfo = new FileInfo(currentDbPath);
                        if (fileInfo.LastWriteTime < currentInfo.LastWriteTime)
                        {
                            // FIXED BUG #12: Better time difference display showing hours for recent backups
                            var timeDiff = currentInfo.LastWriteTime - fileInfo.LastWriteTime;
                            string timeDiffStr;
                            if (timeDiff.TotalHours < 24)
                            {
                                timeDiffStr = $"{(int)timeDiff.TotalHours} hours";
                            }
                            else if (timeDiff.TotalDays < 7)
                            {
                                timeDiffStr = $"{timeDiff.Days} days";
                            }
                            else
                            {
                                timeDiffStr = $"{timeDiff.Days} days ({timeDiff.Days / 7} weeks)";
                            }
                            RestoreStatusText.Text = $"⚠️ Warning: This backup is {timeDiffStr} older than current database";
                        }
                    }

                    RestoreButton.IsEnabled = true;
                    RestoreStatusText.Text = "Backup file validated. Ready to restore.";
                }
                else
                {
                    ValidationFileExistsText.Text = "✗ File not found";
                    ValidationFileExistsText.Foreground = System.Windows.Media.Brushes.Red;
                    RestoreButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                RestoreStatusText.Text = $"❌ Validation failed: {ex.Message}";
                RestoreButton.IsEnabled = false;
            }
        }

        private async void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(_selectedBackupPath))
                {
                    ShowError("Please select a backup file first");
                    return;
                }

                var confirmResult = MessageBox.Show(
                    "Are you sure you want to restore from this backup?\n\n" +
                    "This will:\n" +
                    "• Replace ALL current data with the backup data\n" +
                    "• Create a safety backup of current data\n" +
                    "• Require application restart\n\n" +
                    "Continue?",
                    "Confirm Restore",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (confirmResult != MessageBoxResult.Yes)
                    return;

                RestoreButton.IsEnabled = false;
                RestoreStatusText.Text = "Starting restore process...";

                var result = await _backupService.RestoreBackupAsync(_selectedBackupPath);

                if (result.Success)
                {
                    RestoreStatusText.Text = "✅ Database restored successfully! Restarting…";
                    MessageBox.Show(
                        "Database restored successfully!\n\n" +
                        (string.IsNullOrEmpty(result.SafetyBackupPath) ? "" : $"Safety backup of the previous data:\n{result.SafetyBackupPath}\n\n") +
                        "The application will now restart to load the restored data.",
                        "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                    RestartApplication();
                }
                else if (result.RequiresConfirmation)
                {
                    var overwriteResult = MessageBox.Show(
                        result.Message + "\n\nDo you want to proceed?",
                        "Confirmation Required",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (overwriteResult == MessageBoxResult.Yes)
                    {
                        // FIXED BUG #6: Force restore should ALWAYS create safety backup for data protection.
                        // force: true bypasses the "current DB is newer" guard the user just confirmed past.
                        result = await _backupService.RestoreBackupAsync(_selectedBackupPath, validateChecksum: true, skipSafetyBackup: false, force: true);
                        if (result.Success)
                        {
                            MessageBox.Show(
                                "Database restored successfully!\n\n" +
                                (string.IsNullOrEmpty(result.SafetyBackupPath) ? "" : $"Safety backup of the previous data:\n{result.SafetyBackupPath}\n\n") +
                                "The application will now restart to load the restored data.",
                                "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);
                            RestartApplication();
                        }
                        else
                        {
                            ShowError(result.Message);
                        }
                    }
                }
                else
                {
                    RestoreStatusText.Text = $"❌ Restore failed: {result.Message}";
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                RestoreStatusText.Text = $"❌ Error: {ex.Message}";
                ShowError($"Restore failed: {ex.Message}");
            }
            finally
            {
                RestoreButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Guided "Back up to Google Drive": find the Google Drive for Desktop folder (or let the
        /// user pick one), point all backups at an "IEMS Backups" folder there, turn on a daily
        /// schedule, and create a first backup. Google Drive for Desktop then uploads them for free.
        /// </summary>
        private async void SetupGoogleDriveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var driveFolder = IEMS.WPF.Helpers.GoogleDriveLocator.FindMyDriveFolder();
                if (driveFolder == null)
                {
                    var choice = MessageBox.Show(
                        "Google Drive for Desktop was not detected on this PC.\n\n" +
                        "Install it from drive.google.com/download and sign in, then run this again — " +
                        "or click OK to pick a synced folder manually (e.g. an existing Google Drive or OneDrive folder).",
                        "Google Drive not found", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (choice != MessageBoxResult.OK) return;

                    var picker = new Microsoft.Win32.OpenFolderDialog
                    {
                        Title = "Choose your Google Drive (or other synced) folder"
                    };
                    if (picker.ShowDialog() != true) return;
                    driveFolder = picker.FolderName;
                }

                var backupFolder = Path.Combine(driveFolder, "IEMS Backups");
                Directory.CreateDirectory(backupFolder);

                // 1) Point manual-default and scheduled backups at the Drive folder.
                var settings = _services.GetRequiredService<IEMS.Application.Interfaces.ISystemSettingsService>();
                await settings.UpdateSettingAsync("Backup.BackupPath", backupFolder);

                // 2) Turn on a daily automatic backup so it keeps uploading unattended.
                var schedule = new BackupSchedule
                {
                    EnableDaily = true,
                    DailyTime = new TimeSpan(18, 0, 0), // 6:00 PM
                    EnableWeekly = false,
                    EnableMonthly = false,
                    EnableIncremental = false,
                    WeeklyDay = DayOfWeek.Friday,
                    MonthlyDay = 1,
                    IncrementalIntervalHours = 6
                };
                await _backupService.SetupAutomaticBackupAsync(schedule);

                // 3) Create a first backup right now to confirm the folder is writable and seed the cloud.
                SetupGoogleDriveButton.IsEnabled = false;
                BackupStatusText.Text = $"Creating the first Google Drive backup in {backupFolder}...";
                var result = await _backupService.CreateBackupAsync(BackupType.Manual, backupFolder);
                SetupGoogleDriveButton.IsEnabled = true;

                if (result.Success)
                {
                    GoogleDriveStatusText.Text =
                        $"✅ Backups go to: {backupFolder}\n" +
                        "Daily auto-backup is ON (6:00 PM). Google Drive for Desktop uploads them automatically.";
                    BackupStatusText.Text = $"✅ First Google Drive backup created:\n{result.BackupPath}";

                    // Reflect the daily schedule in the Automatic Backup tab.
                    EnableDailyBackupCheckBox.IsChecked = true;
                    UpdateNextBackupInfo();
                    await LoadBackupHistory();

                    MessageBox.Show(
                        $"Google Drive backup is set up.\n\nBackups are saved to:\n{backupFolder}\n\n" +
                        "Keep Google Drive for Desktop running and signed in — it uploads these to your Drive automatically.",
                        "Google Drive backup ready", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    GoogleDriveStatusText.Text = $"❌ Could not write a backup to {backupFolder}: {result.Message}";
                    BackupStatusText.Text = $"❌ {result.Message}";
                }
            }
            catch (Exception ex)
            {
                SetupGoogleDriveButton.IsEnabled = true;
                ShowError($"Could not set up Google Drive backup: {ex.Message}");
            }
        }

        /// <summary>
        /// One-click recovery (ideal on a new PC): find the newest backup in the Google Drive
        /// "IEMS Backups" folder and restore it, with the same safety backup + restart as a normal
        /// restore. Falls back to the configured Backup.BackupPath (e.g. a OneDrive folder).
        /// </summary>
        private async void RestoreFromGoogleDriveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Find the cloud backups folder: Google Drive first, then the configured backup path.
                string? backupsDir = null;
                var driveFolder = IEMS.WPF.Helpers.GoogleDriveLocator.FindMyDriveFolder();
                if (driveFolder != null)
                {
                    var candidate = Path.Combine(driveFolder, "IEMS Backups");
                    if (Directory.Exists(candidate)) backupsDir = candidate;
                }
                if (backupsDir == null)
                {
                    var settings = _services.GetRequiredService<IEMS.Application.Interfaces.ISystemSettingsService>();
                    var configured = await settings.GetSettingValueAsync("Backup.BackupPath");
                    if (!string.IsNullOrWhiteSpace(configured) && Directory.Exists(configured))
                        backupsDir = configured;
                }

                if (backupsDir == null)
                {
                    MessageBox.Show(
                        "No Google Drive backups folder was found on this PC.\n\n" +
                        "Install Google Drive for Desktop, sign in, and wait for it to finish syncing your " +
                        "\"IEMS Backups\" folder — then try again.\n\n" +
                        "(You can also use \"Select Backup File\" on the Restore tab to pick a backup file manually.)",
                        "No Google Drive backups", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var latest = new DirectoryInfo(backupsDir).GetFiles("school_backup_*.db")
                    .OrderByDescending(f => f.LastWriteTime).FirstOrDefault();
                if (latest == null)
                {
                    MessageBox.Show(
                        $"Found the folder:\n{backupsDir}\n\n" +
                        "…but there are no backups in it yet (or Google Drive hasn't finished downloading them). " +
                        "Wait for syncing to complete and try again.",
                        "No backups found", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var confirm = MessageBox.Show(
                    "Restore the most recent backup from Google Drive?\n\n" +
                    $"File: {latest.Name}\n" +
                    $"Date: {latest.LastWriteTime:G}\n" +
                    $"Size: {FormatFileSize(latest.Length)}\n\n" +
                    "This REPLACES all current data with the backup (a safety backup of the current data " +
                    "is created first), and the application will restart.\n\nContinue?",
                    "Restore from Google Drive", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return;

                RestoreGoogleDriveButton.IsEnabled = false;
                RestoreStatusText.Text = $"Restoring from {latest.FullName}...";
                await RestoreThenRestartAsync(latest.FullName);
                RestoreGoogleDriveButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                RestoreGoogleDriveButton.IsEnabled = true;
                ShowError($"Could not restore from Google Drive: {ex.Message}");
            }
        }

        /// <summary>Restores a backup (handling the "current data is newer" re-confirm) then restarts the app.</summary>
        private async Task RestoreThenRestartAsync(string backupPath)
        {
            var result = await _backupService.RestoreBackupAsync(backupPath);

            if (!result.Success && result.RequiresConfirmation)
            {
                var ow = MessageBox.Show(result.Message + "\n\nDo you want to proceed anyway?",
                    "Confirmation Required", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (ow != MessageBoxResult.Yes) return;
                result = await _backupService.RestoreBackupAsync(backupPath, validateChecksum: true, skipSafetyBackup: false, force: true);
            }

            if (!result.Success)
            {
                ShowError(result.Message);
                return;
            }

            MessageBox.Show(
                "Database restored successfully!\n\n" +
                (string.IsNullOrEmpty(result.SafetyBackupPath) ? "" : $"Safety backup of the previous data:\n{result.SafetyBackupPath}\n\n") +
                "The application will now restart to load the restored data.",
                "Restore Complete", MessageBoxButton.OK, MessageBoxImage.Information);

            RestartApplication();
        }

        /// <summary>
        /// Launches a fresh copy of the app and HARD-exits this one. A restore must restart cleanly:
        /// Application.Current.Shutdown() can't reliably stop the Generic Host's background services,
        /// which left the old process alive (two instances after a restore). Environment.Exit guarantees
        /// the old process is gone; the new instance opens the just-restored database.
        /// </summary>
        private void RestartApplication()
        {
            var exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrEmpty(exePath))
            {
                try { Process.Start(exePath); }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not restart automatically: {ex.Message}\n\nPlease open IEMS again manually.",
                        "Manual Restart Needed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            Serilog.Log.CloseAndFlush();
            Environment.Exit(0);
        }

        private async void SaveScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // FIXED BUG #11: Use Enum.TryParse instead of Enum.Parse to avoid exceptions
                var weeklyDayString = GetSelectedComboBoxText(WeeklyBackupDayComboBox) ?? "Friday";
                if (!Enum.TryParse<DayOfWeek>(weeklyDayString, out var weeklyDay))
                {
                    weeklyDay = DayOfWeek.Friday; // Default fallback
                }

                var schedule = new BackupSchedule
                {
                    EnableDaily = EnableDailyBackupCheckBox.IsChecked ?? false,
                    EnableWeekly = EnableWeeklyBackupCheckBox.IsChecked ?? false,
                    EnableMonthly = EnableMonthlyBackupCheckBox.IsChecked ?? false,
                    EnableIncremental = EnableIncrementalBackupCheckBox.IsChecked ?? false,
                    DailyTime = ParseTimeString(GetSelectedComboBoxText(DailyBackupTimeComboBox) ?? "6:00 PM"),
                    WeeklyDay = weeklyDay,
                    MonthlyDay = ParseMonthlyDay(GetSelectedComboBoxText(MonthlyBackupDayComboBox) ?? "1st"),
                    IncrementalIntervalHours = 6
                };

                var result = await _backupService.SetupAutomaticBackupAsync(schedule);

                if (result.Success)
                {
                    MessageBox.Show(
                        "Backup schedule saved successfully!",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    UpdateNextBackupInfo();
                }
                else
                {
                    ShowError(result.Message);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to save schedule: {ex.Message}");
            }
        }

        private void BackupScheduleChanged(object sender, RoutedEventArgs e)
        {
            UpdateNextBackupInfo();
        }

        private void UpdateNextBackupInfo()
        {
            try
            {
                var info = new List<string>();

                if (EnableDailyBackupCheckBox?.IsChecked == true)
                {
                    var time = GetSelectedComboBoxText(DailyBackupTimeComboBox) ?? "6:00 PM";
                    info.Add($"Daily at {time}");
                }

                if (EnableWeeklyBackupCheckBox?.IsChecked == true)
                {
                    var day = GetSelectedComboBoxText(WeeklyBackupDayComboBox) ?? "Friday";
                    info.Add($"Weekly on {day}");
                }

                if (EnableMonthlyBackupCheckBox?.IsChecked == true)
                {
                    var day = GetSelectedComboBoxText(MonthlyBackupDayComboBox) ?? "1st";
                    info.Add($"Monthly on {day}");
                }

                if (EnableIncrementalBackupCheckBox?.IsChecked == true)
                {
                    info.Add("Incremental every 6 hours");
                }

                NextBackupInfoText.Text = info.Any()
                    ? string.Join("\n", info)
                    : "No automatic backups scheduled";
            }
            catch { }
        }

        private void OpenBackupLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var path = button?.Tag as string;

                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to open location: {ex.Message}");
            }
        }

        private async void RestoreFromHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var path = button?.Tag as string;

                if (!string.IsNullOrEmpty(path))
                {
                    _selectedBackupPath = path;
                    SelectedBackupFileText.Text = Path.GetFileName(path);

                    // Switch to Restore tab
                    var tabControl = FindName("TabControl") as TabControl;
                    if (tabControl != null)
                        tabControl.SelectedIndex = 1;

                    await ValidateBackupFile(path);
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to select backup: {ex.Message}");
            }
        }

        private async void DeleteBackup_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var backupInfo = button?.Tag as BackupInfoViewModel;

                if (backupInfo != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this backup?\n\n{backupInfo.BackupPath}",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (result == MessageBoxResult.Yes)
                    {
                        bool deletionSuccessful = true;

                        if (File.Exists(backupInfo.BackupPath))
                        {
                            try
                            {
                                File.Delete(backupInfo.BackupPath);

                                // Verify main file was deleted
                                if (File.Exists(backupInfo.BackupPath))
                                {
                                    deletionSuccessful = false;
                                }
                                else
                                {
                                    // Also delete associated WAL and SHM files if they exist
                                    var walPath = backupInfo.BackupPath + "-wal";
                                    var shmPath = backupInfo.BackupPath + "-shm";

                                    if (File.Exists(walPath))
                                        File.Delete(walPath);
                                    if (File.Exists(shmPath))
                                        File.Delete(shmPath);
                                }
                            }
                            catch
                            {
                                deletionSuccessful = false;
                                throw; // Re-throw to be caught by outer catch
                            }
                        }

                        // Only update metadata if file deletion was successful
                        if (deletionSuccessful)
                        {
                            await _backupService.RemoveBackupFromMetadataAsync(backupInfo.Id);
                            await LoadBackupHistory();
                            MessageBox.Show("Backup deleted successfully", "Success",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError($"Failed to delete backup: {ex.Message}");
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private TimeSpan ParseTimeString(string timeString)
        {
            try
            {
                // Handle AM/PM format
                if (DateTime.TryParse(timeString, out DateTime dateTime))
                {
                    return dateTime.TimeOfDay;
                }

                // Fallback to direct TimeSpan parsing
                if (TimeSpan.TryParse(timeString, out TimeSpan timeSpan))
                {
                    return timeSpan;
                }

                // Default fallback
                return new TimeSpan(18, 0, 0); // 6:00 PM
            }
            catch
            {
                return new TimeSpan(18, 0, 0); // 6:00 PM default
            }
        }

        private int ParseMonthlyDay(string day)
        {
            return day switch
            {
                "1st" => 1,
                "15th" => 15,
                "Last Day" => -1,
                _ => 1
            };
        }

        private void SetTimeComboBox(TimeSpan time)
        {
            try
            {
                var timeString = DateTime.Today.Add(time).ToString("h:mm tt");

                foreach (ComboBoxItem item in DailyBackupTimeComboBox.Items)
                {
                    if (item.Content.ToString() == timeString)
                    {
                        DailyBackupTimeComboBox.SelectedItem = item;
                        return;
                    }
                }

                // Default to 6:00 PM if not found
                DailyBackupTimeComboBox.SelectedIndex = 2;
            }
            catch
            {
                DailyBackupTimeComboBox.SelectedIndex = 2; // Default to 6:00 PM
            }
        }

        private void SetWeeklyDayComboBox(DayOfWeek day)
        {
            try
            {
                var dayString = day.ToString();

                foreach (ComboBoxItem item in WeeklyBackupDayComboBox.Items)
                {
                    if (item.Content.ToString() == dayString)
                    {
                        WeeklyBackupDayComboBox.SelectedItem = item;
                        return;
                    }
                }

                // Default to Friday
                WeeklyBackupDayComboBox.SelectedIndex = 5;
            }
            catch
            {
                WeeklyBackupDayComboBox.SelectedIndex = 5; // Default to Friday
            }
        }

        private void SetMonthlyDayComboBox(int day)
        {
            try
            {
                string dayString = day switch
                {
                    1 => "1st",
                    15 => "15th",
                    -1 => "Last Day",
                    _ => "1st"
                };

                foreach (ComboBoxItem item in MonthlyBackupDayComboBox.Items)
                {
                    if (item.Content.ToString() == dayString)
                    {
                        MonthlyBackupDayComboBox.SelectedItem = item;
                        return;
                    }
                }

                // Default to 1st
                MonthlyBackupDayComboBox.SelectedIndex = 0;
            }
            catch
            {
                MonthlyBackupDayComboBox.SelectedIndex = 0; // Default to 1st
            }
        }

        private string? GetSelectedComboBoxText(ComboBox? comboBox)
        {
            try
            {
                if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
                {
                    return selectedItem.Content?.ToString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class BackupInfoViewModel
        {
            public string Id { get; set; }
            public BackupType Type { get; set; }
            public DateTime Timestamp { get; set; }
            public string BackupPath { get; set; }
            public long FileSize { get; set; }
            public string FileSizeFormatted { get; set; }
        }
    }
}