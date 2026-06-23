using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using IEMS.WPF.Services;
using QRCoder;

namespace IEMS.WPF;

public partial class PhoneUploadWindow : Window
{
    private readonly PhoneUploadServer _server;
    private bool _received;

    /// <summary>The file received from the phone (set when DialogResult == true).</summary>
    public UploadedFile? ReceivedFile { get; private set; }

    /// <summary>Convenience for the photo flow.</summary>
    public byte[]? ReceivedPhoto => ReceivedFile?.Data;

    public PhoneUploadWindow(string studentName, bool documentMode = false)
    {
        InitializeComponent();
        Title = documentMode ? "Upload Document from Phone" : "Upload Photo from Phone";
        lblFor.Text = $"For {studentName}";
        _server = new PhoneUploadServer(studentName, documentMode);
        _server.FileReceived += OnFileReceived;

        Loaded += (_, _) => Start();
        Closed += (_, _) => _server.Dispose();
    }

    private void Start()
    {
        try
        {
            var url = _server.Start();
            txtUrl.Text = url;
            imgQr.Source = MakeQr(url);
        }
        catch (Exception ex)
        {
            lblStatus.Foreground = System.Windows.Media.Brushes.Firebrick;
            lblStatus.Text = ex.Message;
        }
    }

    private void OnFileReceived(UploadedFile file)
    {
        // Raised on a background thread — marshal to the UI thread.
        Dispatcher.Invoke(() =>
        {
            if (_received) return;
            _received = true;
            ReceivedFile = file;
            lblStatus.Foreground = System.Windows.Media.Brushes.Green;
            lblStatus.Text = "✓ Received!";
            DialogResult = true;
            Close();
        });
    }

    private static BitmapImage MakeQr(string text)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data).GetGraphic(10);
        using var ms = new MemoryStream(png);
        var bmp = new BitmapImage();
        bmp.BeginInit();
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.StreamSource = ms;
        bmp.EndInit();
        bmp.Freeze();
        return bmp;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
