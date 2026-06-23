using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>Shared "save as PDF, then Open / Send to Phone" flow for the QuestPDF documents.</summary>
    public static class PdfExporter
    {
        public static void SaveAndOpen(IDocument document, string suggestedFileName)
        {
            try
            {
                var fileName = suggestedFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                    ? suggestedFileName : suggestedFileName + ".pdf";

                var dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = fileName,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (dialog.ShowDialog() != true)
                    return;

                document.GeneratePdf(dialog.FileName);

                var owner = ActiveOwner();
                var dlg = new PdfExportedWindow(Path.GetFileName(dialog.FileName));
                if (owner != null) dlg.Owner = owner;
                dlg.ShowDialog();

                switch (dlg.Result)
                {
                    case PdfExportedWindow.Choice.Open:
                        Process.Start(new ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
                        break;
                    case PdfExportedWindow.Choice.Send:
                        var bytes = File.ReadAllBytes(dialog.FileName);
                        IEMS.WPF.Services.PhoneTransfer.Send(owner!, bytes, Path.GetFileName(dialog.FileName),
                            "application/pdf", Path.GetFileNameWithoutExtension(dialog.FileName));
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export PDF: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static Window? ActiveOwner()
            => System.Windows.Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
               ?? System.Windows.Application.Current?.MainWindow;
    }
}
