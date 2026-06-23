using System;
using System.IO;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace IEMS.WPF.Helpers;

/// <summary>Shared helpers for picking, validating and displaying student photos.</summary>
public static class PhotoHelper
{
    public const int MaxPhotoBytes = 2 * 1024 * 1024; // 2 MB

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
    /// Opens a file picker, validates the size (&lt;= 2 MB) and that the bytes decode as an image,
    /// and returns them. Returns null if the user cancels; throws with a clear message if invalid.
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
}
