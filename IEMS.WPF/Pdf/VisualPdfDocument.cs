using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace IEMS.WPF.Pdf
{
    /// <summary>
    /// Wraps a high-resolution snapshot (PNG) of a WPF visual as a single-page A4 PDF. This lets
    /// documents that are laid out in XAML (e.g. the leaving-certificate preview) be exported and
    /// shared as a real PDF through the same PdfExporter / PhoneTransfer flow as the QuestPDF
    /// documents, instead of the old "save as XPS and print to PDF yourself" workaround.
    /// </summary>
    public class VisualPdfDocument : IDocument
    {
        private readonly byte[] _png;

        public VisualPdfDocument(byte[] png) => _png = png;

        public DocumentSettings GetSettings() => DocumentSettings.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(10, Unit.Millimetre);
                page.Content().AlignCenter().AlignMiddle().Image(_png).FitArea();
            });
        }

        /// <summary>
        /// Renders a laid-out WPF element to PNG bytes (on a white background) at the given DPI.
        /// </summary>
        public static byte[] RenderToPng(FrameworkElement element, double dpi = 200)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            // Make sure the element is fully measured/arranged at its natural size.
            element.Measure(new System.Windows.Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(element.DesiredSize));
            element.UpdateLayout();

            double width = element.ActualWidth > 0 ? element.ActualWidth : element.DesiredSize.Width;
            double height = element.ActualHeight > 0 ? element.ActualHeight : element.DesiredSize.Height;
            if (width <= 0 || height <= 0)
                throw new InvalidOperationException("There is nothing to render yet. Generate the document first.");

            var rtb = new RenderTargetBitmap(
                (int)Math.Ceiling(width * dpi / 96.0),
                (int)Math.Ceiling(height * dpi / 96.0),
                dpi, dpi, PixelFormats.Pbgra32);

            // Paint a white background first, then the element on top (it may have transparent areas).
            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
                dc.DrawRectangle(new VisualBrush(element), null, new Rect(0, 0, width, height));
            }
            rtb.Render(dv);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
    }
}
