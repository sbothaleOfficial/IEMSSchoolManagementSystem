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

    /// <summary>The photo bytes received from the phone (set when DialogResult == true).</summary>
    public byte[]? ReceivedPhoto { get; private set; }

    public PhoneUploadWindow(string studentName)
    {
        InitializeComponent();
        lblFor.Text = $"For {studentName}";
        _server = new PhoneUploadServer(studentName);
        _server.PhotoReceived += OnPhotoReceived;

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

    private void OnPhotoReceived(byte[] bytes)
    {
        // Raised on a background thread — marshal to the UI thread.
        Dispatcher.Invoke(() =>
        {
            if (_received) return;
            _received = true;
            ReceivedPhoto = bytes;
            lblStatus.Foreground = System.Windows.Media.Brushes.Green;
            lblStatus.Text = "✓ Photo received!";
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
