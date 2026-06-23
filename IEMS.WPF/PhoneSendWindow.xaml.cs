using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using IEMS.WPF.Services;
using QRCoder;

namespace IEMS.WPF;

public partial class PhoneSendWindow : Window
{
    private readonly PhoneDownloadServer _server;

    public PhoneSendWindow(byte[] data, string fileName, string contentType, string displayName)
    {
        InitializeComponent();
        lblFile.Text = displayName;
        _server = new PhoneDownloadServer(data, fileName, contentType, displayName);
        _server.Downloaded += OnDownloaded;

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

    private void OnDownloaded()
    {
        Dispatcher.Invoke(() =>
        {
            lblStatus.Foreground = System.Windows.Media.Brushes.Green;
            lblStatus.Text = "✓ Opened on the phone. You can close this.";
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

    private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
}
