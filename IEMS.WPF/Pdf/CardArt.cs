using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IEMS.WPF.Pdf
{
    /// <summary>
    /// Renders the decorative navy/gold ID-card backgrounds (front and back) as high-resolution PNGs
    /// using WPF drawing, so QuestPDF can simply composite them behind the card content.
    /// </summary>
    public static class CardArt
    {
        public static readonly Color Navy = Color.FromRgb(0x14, 0x23, 0x5B);
        public static readonly Color NavyDark = Color.FromRgb(0x0E, 0x19, 0x44);
        public static readonly Color Gold = Color.FromRgb(0xF0, 0xB4, 0x29);

        private const double Px = 12.0; // pixels per millimetre (~305 DPI)

        public static byte[] RenderFront(double widthMm, double heightMm)
            => Render(widthMm, heightMm, isBack: false);

        public static byte[] RenderBack(double widthMm, double heightMm)
            => Render(widthMm, heightMm, isBack: true);

        private static byte[] Render(double widthMm, double heightMm, bool isBack)
        {
            int w = (int)Math.Round(widthMm * Px);
            int h = (int)Math.Round(heightMm * Px);

            var navy = new SolidColorBrush(Navy);
            var navyDark = new SolidColorBrush(NavyDark);
            var gold = new SolidColorBrush(Gold);
            navy.Freeze(); navyDark.Freeze(); gold.Freeze();

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, w, h));

                // ----- Header: navy panel with a wavy bottom edge + gold accent -----
                double headL = isBack ? h * 0.26 : h * 0.24; // bottom edge height on the left
                double headR = isBack ? h * 0.20 : h * 0.18; // … and on the right

                // Gold accent sits just under the navy curve.
                dc.DrawGeometry(gold, null, HeaderWave(w, headL + h * 0.025, headR + h * 0.025));
                dc.DrawGeometry(navy, null, HeaderWave(w, headL, headR));

                // ----- Footer: navy panel with a wavy top edge + gold accent -----
                // The back has more footer content (3 contact lines), so give it a taller band.
                double footL = (isBack ? 0.19 : 0.11) * h;
                double footR = (isBack ? 0.22 : 0.15) * h;
                dc.DrawGeometry(gold, null, FooterWave(w, h, footL + h * 0.022, footR + h * 0.022));
                dc.DrawGeometry(navyDark, null, FooterWave(w, h, footL, footR));
            }

            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        // Navy header filling the top, with a smooth curved bottom edge (lower on the left).
        private static Geometry HeaderWave(int w, double leftY, double rightY)
        {
            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(0, 0), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(w, 0), true, false);
                ctx.LineTo(new Point(w, rightY), true, false);
                // gentle S-curve from right edge to left edge
                ctx.BezierTo(new Point(w * 0.66, rightY - (leftY - rightY) * 0.9),
                             new Point(w * 0.34, leftY + (leftY - rightY) * 0.9),
                             new Point(0, leftY), true, false);
            }
            g.Freeze();
            return g;
        }

        // Navy footer filling the bottom, with a smooth curved top edge (higher on the right).
        private static Geometry FooterWave(int w, int h, double leftH, double rightH)
        {
            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(0, h), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(w, h), true, false);
                ctx.LineTo(new Point(w, h - rightH), true, false);
                ctx.BezierTo(new Point(w * 0.66, h - rightH + (rightH - leftH) * 0.9),
                             new Point(w * 0.34, h - leftH - (rightH - leftH) * 0.9),
                             new Point(0, h - leftH), true, false);
            }
            g.Freeze();
            return g;
        }

        /// <summary>Returns the photo re-encoded with rounded corners on a transparent canvas.</summary>
        public static byte[] RoundPhoto(byte[] photo, double cornerFraction = 0.10)
        {
            var src = PhotoToBitmap(photo);
            int w = src.PixelWidth, h = src.PixelHeight;
            double r = Math.Min(w, h) * cornerFraction;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                var clip = new RectangleGeometry(new Rect(0, 0, w, h), r, r);
                dc.PushClip(clip);
                dc.DrawImage(src, new Rect(0, 0, w, h));
                dc.Pop();
            }
            var rtb = new RenderTargetBitmap(w, h, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }

        private static BitmapSource PhotoToBitmap(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
            return decoder.Frames[0];
        }
    }
}
