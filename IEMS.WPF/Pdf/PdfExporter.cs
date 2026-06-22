using System;
using System.Diagnostics;
using System.Windows;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>Shared "save as PDF + offer to open" flow for the QuestPDF documents.</summary>
    public static class PdfExporter
    {
        public static void SaveAndOpen(IDocument document, string suggestedFileName)
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    FileName = suggestedFileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                        ? suggestedFileName
                        : suggestedFileName + ".pdf",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
                };

                if (dialog.ShowDialog() != true)
                    return;

                document.GeneratePdf(dialog.FileName);

                var result = MessageBox.Show(
                    "PDF saved successfully. Do you want to open it now?",
                    "PDF Exported", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    Process.Start(new ProcessStartInfo { FileName = dialog.FileName, UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not export PDF: {ex.Message}", "Export Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
