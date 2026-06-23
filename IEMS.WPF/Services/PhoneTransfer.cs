using System.Windows;

namespace IEMS.WPF.Services;

/// <summary>
/// Single shared entry point for moving files between the app and a phone over the local network.
/// Both the ID-card photo flow and the student-documents flow capture through <see cref="Capture"/>;
/// generated documents are sent through <see cref="Send"/>.
/// </summary>
public static class PhoneTransfer
{
    /// <summary>Shows a QR code and waits for the phone to upload a photo/document. Null if cancelled.</summary>
    public static UploadedFile? Capture(Window owner, string studentName, bool documentMode)
    {
        var win = new PhoneUploadWindow(studentName, documentMode) { Owner = owner };
        return win.ShowDialog() == true ? win.ReceivedFile : null;
    }

    /// <summary>Shows a QR code that lets the phone open/download the given file.</summary>
    public static void Send(Window owner, byte[] data, string fileName, string contentType, string displayName)
    {
        var win = new PhoneSendWindow(data, fileName, contentType, displayName) { Owner = owner };
        win.ShowDialog();
    }
}
