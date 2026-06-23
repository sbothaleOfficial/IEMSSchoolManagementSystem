using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IEMS.Application.DTOs;
using IEMS.Application.Services;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AuditLogWindow : Window
{
    private readonly AuditLogService _auditLogService;
    private const int MaxRows = 500;
    private IReadOnlyList<AuditLogDto> _currentLogs = new List<AuditLogDto>();

    public AuditLogWindow(AuditLogService auditLogService)
    {
        InitializeComponent();
        _auditLogService = auditLogService;
        Loaded += (_, _) => AsyncHelper.SafeFireAndForget(InitializeAsync, "Audit Log Load Error");
    }

    private async Task InitializeAsync()
    {
        await LoadEntityTypesAsync();
        await LoadLogsAsync();
    }

    private async Task LoadEntityTypesAsync()
    {
        var types = await _auditLogService.GetEntityTypesAsync();
        cmbEntityType.Items.Clear();
        cmbEntityType.Items.Add("(All)");
        foreach (var t in types)
            cmbEntityType.Items.Add(t);
        cmbEntityType.SelectedIndex = 0;
    }

    private async Task LoadLogsAsync()
    {
        var entity = cmbEntityType.SelectedItem as string;
        if (entity == "(All)") entity = null;

        var action = (cmbAction.SelectedItem as ComboBoxItem)?.Content as string;
        if (action == "(All)") action = null;

        var search = string.IsNullOrWhiteSpace(txtSearch.Text) ? null : txtSearch.Text.Trim();

        var logs = await _auditLogService.GetLogsAsync(entity, action, search, maxRows: MaxRows);
        _currentLogs = logs;
        dgAudit.ItemsSource = logs;

        lblStatus.Text = logs.Count >= MaxRows
            ? $"Showing the most recent {MaxRows} entries (refine filters to narrow down)."
            : $"{logs.Count} audit entr{(logs.Count == 1 ? "y" : "ies")} shown.";
    }

    private void BtnApply_Click(object sender, RoutedEventArgs e) =>
        AsyncHelper.SafeFireAndForget(LoadLogsAsync, "Audit Log Filter Error");

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        txtSearch.Text = string.Empty;
        if (cmbEntityType.Items.Count > 0) cmbEntityType.SelectedIndex = 0;
        cmbAction.SelectedIndex = 0;
        AsyncHelper.SafeFireAndForget(LoadLogsAsync, "Audit Log Filter Error");
    }

    private void BtnExportCsv_Click(object sender, RoutedEventArgs e)
    {
        if (_currentLogs.Count == 0)
        {
            MessageBox.Show("There are no audit entries to export.", "Export CSV",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"AuditTrail_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };
        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("When,User,Action,Entity,Record Id,Summary");
            foreach (var log in _currentLogs)
            {
                sb.Append(Csv(log.FormattedTimestamp)).Append(',')
                  .Append(Csv(log.UserName)).Append(',')
                  .Append(Csv(log.Action)).Append(',')
                  .Append(Csv(log.EntityType)).Append(',')
                  .Append(Csv(log.EntityId)).Append(',')
                  .Append(Csv(log.Summary ?? string.Empty)).Append('\n');
            }

            // UTF-8 BOM so Excel opens non-ASCII (names, ₹) correctly.
            System.IO.File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));

            var open = MessageBox.Show($"Exported {_currentLogs.Count} entries. Open the file now?",
                "Export CSV", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (open == MessageBoxResult.Yes)
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not export CSV: {ex.Message}", "Export Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>Quotes a CSV field and escapes embedded quotes, per RFC 4180.</summary>
    private static string Csv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n') || value.Contains('\r'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
