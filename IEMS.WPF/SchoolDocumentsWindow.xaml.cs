using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IEMS.Application.DTOs;
using IEMS.Application.Services;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class SchoolDocumentsWindow : Window
{
    private readonly SchoolDocumentService _service;
    private readonly string _currentUser;
    private IReadOnlyList<SchoolDocumentDto> _all = new List<SchoolDocumentDto>();

    public SchoolDocumentsWindow(SchoolDocumentService service, string currentUser)
    {
        InitializeComponent();
        _service = service;
        _currentUser = string.IsNullOrWhiteSpace(currentUser) ? "admin" : currentUser;

        foreach (var t in SchoolDocumentService.DocumentTypes) cmbDocType.Items.Add(t);
        cmbDocType.SelectedIndex = 0;

        Loaded += (_, _) => AsyncHelper.SafeFireAndForget(LoadAsync, "Documents Load Error");
    }

    private async Task LoadAsync()
    {
        _all = await _service.GetDocumentsAsync();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var search = txtSearch?.Text?.Trim();
        IEnumerable<SchoolDocumentDto> q = _all;
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLowerInvariant();
            q = q.Where(d => d.FileName.ToLowerInvariant().Contains(s)
                          || d.DocumentType.ToLowerInvariant().Contains(s));
        }
        var list = q.ToList();
        dgDocs.ItemsSource = list;
        lblStatus.Text = _all.Count == 0
            ? "No documents yet. Use Upload File or Upload from Phone."
            : $"{list.Count} of {_all.Count} document{(_all.Count == 1 ? "" : "s")}.";
    }

    private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilter();

    private string SelectedType => cmbDocType.SelectedItem as string ?? "Other";

    private void BtnUploadFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Select a school document",
            Filter = "Documents (images, PDF)|*.jpg;*.jpeg;*.png;*.pdf|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var info = new FileInfo(dialog.FileName);
            if (info.Length > 25 * 1024 * 1024)
            {
                MessageBox.Show("The file is larger than 25 MB. Please choose a smaller file.",
                    "File too large", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var bytes = File.ReadAllBytes(dialog.FileName);
            var contentType = ContentTypeFromName(dialog.FileName);
            AddDocument(Path.GetFileName(dialog.FileName), contentType, bytes);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not read the file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnUploadPhone_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var f = IEMS.WPF.Services.PhoneTransfer.Capture(this, "School documents", documentMode: true);
            if (f != null)
            {
                var name = string.IsNullOrWhiteSpace(f.FileName) ? SuggestName(f.ContentType) : f.FileName;
                AddDocument(name, f.ContentType, f.Data);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not receive the phone upload: {ex.Message}", "Upload from Phone",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnSendPhone_Click(object sender, RoutedEventArgs e)
    {
        if (dgDocs.SelectedItem is not SchoolDocumentDto dto)
        {
            MessageBox.Show("Select a document to send to a phone.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        AsyncHelper.SafeFireAndForget(async () =>
        {
            var file = await _service.GetFileAsync(dto.Id);
            if (file == null) return;
            Dispatcher.Invoke(() => IEMS.WPF.Services.PhoneTransfer.Send(this, file.Data, file.FileName, file.ContentType, dto.FileName));
        }, "Send Document Error");
    }

    private void AddDocument(string fileName, string contentType, byte[] bytes)
    {
        AsyncHelper.SafeFireAndForget(async () =>
        {
            await _service.AddDocumentAsync(SelectedType, fileName, contentType, bytes, _currentUser);
            await LoadAsync();
            lblStatus.Text = $"Added {SelectedType}: {fileName}";
        }, "Add Document Error");
    }

    private void BtnOpen_Click(object sender, RoutedEventArgs e) => OpenSelected();
    private void DgDocs_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) => OpenSelected();

    private void OpenSelected()
    {
        if (dgDocs.SelectedItem is not SchoolDocumentDto dto)
        {
            MessageBox.Show("Select a document to open.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        AsyncHelper.SafeFireAndForget(async () =>
        {
            var file = await _service.GetFileAsync(dto.Id);
            if (file == null) return;

            // Write to a temp file with a sensible extension, then open in the default viewer.
            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(ext)) ext = ExtFromContentType(file.ContentType);
            var safe = string.Join("_", file.FileName.Split(Path.GetInvalidFileNameChars()));
            if (string.IsNullOrWhiteSpace(Path.GetFileNameWithoutExtension(safe))) safe = "document" + ext;
            var temp = Path.Combine(Path.GetTempPath(), "IEMS_schooldoc_" + Path.GetFileNameWithoutExtension(safe) + ext);
            File.WriteAllBytes(temp, file.Data);
            Process.Start(new ProcessStartInfo { FileName = temp, UseShellExecute = true });
        }, "Open Document Error");
    }

    private void BtnDelete_Click(object sender, RoutedEventArgs e)
    {
        if (dgDocs.SelectedItem is not SchoolDocumentDto dto)
        {
            MessageBox.Show("Select a document to delete.", "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var ok = MessageBox.Show($"Delete \"{dto.FileName}\" ({dto.DocumentType})? This cannot be undone.",
            "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (ok != MessageBoxResult.Yes) return;

        AsyncHelper.SafeFireAndForget(async () =>
        {
            await _service.DeleteDocumentAsync(dto.Id);
            await LoadAsync();
            lblStatus.Text = "Document deleted.";
        }, "Delete Document Error");
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

    private static string ContentTypeFromName(string name) => ExtFromName(name) switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".pdf" => "application/pdf",
        ".gif" => "image/gif",
        ".bmp" => "image/bmp",
        ".webp" => "image/webp",
        _ => "application/octet-stream"
    };

    private static string ExtFromName(string name) => Path.GetExtension(name).ToLowerInvariant();

    private static string ExtFromContentType(string contentType) => contentType?.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "application/pdf" => ".pdf",
        "image/gif" => ".gif",
        "image/bmp" => ".bmp",
        "image/webp" => ".webp",
        _ => ".bin"
    };

    private static string SuggestName(string contentType) => "document" + ExtFromContentType(contentType);
}
