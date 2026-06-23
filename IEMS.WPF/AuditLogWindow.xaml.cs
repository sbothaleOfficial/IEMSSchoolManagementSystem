using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IEMS.Application.Services;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AuditLogWindow : Window
{
    private readonly AuditLogService _auditLogService;
    private const int MaxRows = 500;

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

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
