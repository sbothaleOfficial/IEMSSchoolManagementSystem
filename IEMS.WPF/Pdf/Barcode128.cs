using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IEMS.WPF.Pdf
{
    /// <summary>Generates a Code 128-B barcode as a PNG (drawn with WPF, no external libraries).</summary>
    public static class Barcode128
    {
        // Module-width patterns for Code 128 values 0..106 (each "bar space bar space bar space",
        // the last entry is the Stop pattern with a trailing bar).
        private static readonly string[] Patterns =
        {
            "212222","222122","222221","121223","121322","131222","122213","122312","132212","221213",
            "221312","231212","112232","122132","122231","113222","123122","123221","223211","221132",
            "221231","213212","223112","312131","311222","321122","321221","312212","322112","322211",
            "212123","212321","232121","111323","131123","131321","112313","132113","132311","211313",
            "231113","231311","112133","112331","132131","113123","113321","133121","313121","211331",
            "231131","213113","213311","213131","311123","311321","331121","312113","312311","332111",
            "314111","221411","431111","111224","111422","121124","121421","141122","141221","112214",
            "112412","122114","122411","142112","142211","241211","221114","413111","241112","134111",
            "111242","121142","121241","114212","124112","124211","411212","421112","421211","212141",
            "214121","412121","111143","111341","131141","114113","114311","411113","411311","113141",
            "114131","311141","411131","211412","211214","211232","2331112"
        };

        private const int StartB = 104;
        private const int Stop = 106;

        /// <summary>Renders <paramref name="data"/> as a Code 128-B barcode PNG of the given pixel size.</summary>
        public static byte[] RenderPng(string data, int widthPx, int heightPx)
        {
            if (string.IsNullOrEmpty(data)) data = " ";

            // Build the symbol values: Start-B, data, checksum, Stop.
            var values = new List<int> { StartB };
            long sum = StartB;
            for (int i = 0; i < data.Length; i++)
            {
                int c = data[i];
                if (c < 32 || c > 126) c = '?';
                int v = c - 32;
                values.Add(v);
                sum += (long)v * (i + 1);
            }
            values.Add((int)(sum % 103)); // checksum
            values.Add(Stop);

            // Flatten to module widths (alternating bar, space, bar, … starting with a bar).
            var modules = new List<int>();
            foreach (var v in values)
                foreach (var ch in Patterns[v])
                    modules.Add(ch - '0');

            int quiet = 10; // quiet zone (modules) each side
            int totalModules = quiet * 2;
            foreach (var m in modules) totalModules += m;

            double moduleW = widthPx / (double)totalModules;

            var dv = new DrawingVisual();
            using (var dc = dv.RenderOpen())
            {
                dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, widthPx, heightPx));
                double x = quiet * moduleW;
                bool bar = true; // sequence starts with a bar
                foreach (var m in modules)
                {
                    double w = m * moduleW;
                    if (bar)
                        dc.DrawRectangle(Brushes.Black, null, new Rect(x, 0, w, heightPx));
                    x += w;
                    bar = !bar;
                }
            }

            var rtb = new RenderTargetBitmap(widthPx, heightPx, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(dv);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using var ms = new MemoryStream();
            encoder.Save(ms);
            return ms.ToArray();
        }
    }
}
