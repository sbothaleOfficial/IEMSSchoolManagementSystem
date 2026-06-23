using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace IEMS.WPF.Helpers;

/// <summary>Shared helpers for picking, scanning, validating, cropping and displaying student photos.</summary>
public static class PhotoHelper
{
    public const int MaxPhotoBytes = 2 * 1024 * 1024; // 2 MB

    // Passport aspect (35:45) used for ID-card photos.
    private const double TargetAspect = 35.0 / 45.0;
    private const int TargetMaxWidthPx = 500;

    /// <summary>Decodes image bytes into a frozen, source-independent bitmap (null if empty).</summary>
    public static BitmapImage? Decode(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        using var ms = new MemoryStream(bytes);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    /// <summary>
    /// Opens a file picker, validates the size (&lt;= 2 MB) and that the bytes decode as an image.
    /// Returns the raw bytes, or null if the user cancels; throws with a clear message if invalid.
    /// </summary>
    public static byte[]? Pick()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select Student Photo",
            Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };
        if (dialog.ShowDialog() != true)
            return null;

        var info = new FileInfo(dialog.FileName);
        if (info.Length > MaxPhotoBytes)
            throw new InvalidOperationException("The selected image is larger than 2 MB. Please choose a smaller photo.");

        var bytes = File.ReadAllBytes(dialog.FileName);
        _ = Decode(bytes); // throws if the bytes are not a valid image
        return bytes;
    }

    /// <summary>
    /// Acquires an image from a connected scanner (or camera) via Windows Image Acquisition (WIA),
    /// showing the system scan dialog. Returns the scanned bytes, or null if the user cancels.
    /// Throws a clear message if WIA/no device is available.
    /// </summary>
    public static byte[]? ScanFromScanner()
    {
        var wiaType = Type.GetTypeFromProgID("WIA.CommonDialog");
        if (wiaType == null)
            throw new InvalidOperationException("Scanning (Windows Image Acquisition) is not available on this PC.");

        dynamic dialog = Activator.CreateInstance(wiaType)!;
        dynamic? image;
        try
        {
            // Shows the device-selection + scan dialog; returns the acquired image (null if cancelled).
            image = dialog.ShowAcquireImage();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Could not start the scan. Make sure a scanner is connected and switched on.\n\nDetails: " + ex.Message);
        }

        if (image == null)
            return null; // user cancelled

        var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".img");
        try
        {
            image.SaveFile(tempFile);
            return File.ReadAllBytes(tempFile);
        }
        finally
        {
            try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { /* best effort */ }
        }
    }

    /// <summary>
    /// Centre-crops an image to the passport aspect (35:45), downscales it to a sensible size and
    /// re-encodes it as JPEG. This makes every photo fill the ID-card photo box (no grey margins)
    /// and keeps the stored bytes small, regardless of the source (upload or scanner).
    /// </summary>
    public static byte[] NormalizeForCard(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            throw new InvalidOperationException("The image is empty.");

        using var ms = new MemoryStream(bytes);
        var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        BitmapSource src = decoder.Frames[0];

        // Center-crop to the target aspect.
        double srcRatio = src.PixelWidth / (double)src.PixelHeight;
        Int32Rect crop;
        if (srcRatio > TargetAspect)
        {
            int w = (int)Math.Round(src.PixelHeight * TargetAspect);
            crop = new Int32Rect(Math.Max(0, (src.PixelWidth - w) / 2), 0, Math.Min(w, src.PixelWidth), src.PixelHeight);
        }
        else
        {
            int h = (int)Math.Round(src.PixelWidth / TargetAspect);
            crop = new Int32Rect(0, Math.Max(0, (src.PixelHeight - h) / 2), src.PixelWidth, Math.Min(h, src.PixelHeight));
        }

        BitmapSource result = new CroppedBitmap(src, crop);

        // Downscale if wider than the target.
        if (result.PixelWidth > TargetMaxWidthPx)
        {
            double scale = TargetMaxWidthPx / (double)result.PixelWidth;
            result = new TransformedBitmap(result, new ScaleTransform(scale, scale));
        }

        var encoder = new JpegBitmapEncoder { QualityLevel = 88 };
        encoder.Frames.Add(BitmapFrame.Create(result));
        using var outMs = new MemoryStream();
        encoder.Save(outMs);
        return outMs.ToArray();
    }
}
