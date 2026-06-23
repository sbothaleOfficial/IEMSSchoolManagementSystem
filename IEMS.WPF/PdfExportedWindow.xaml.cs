using System.Windows;

namespace IEMS.WPF;

public partial class PdfExportedWindow : Window
{
    public enum Choice { Close, Open, Send }

    public Choice Result { get; private set; } = Choice.Close;

    public PdfExportedWindow(string fileName)
    {
        InitializeComponent();
        lblFile.Text = fileName;
    }

    private void Open_Click(object sender, RoutedEventArgs e) { Result = Choice.Open; DialogResult = true; Close(); }
    private void Send_Click(object sender, RoutedEventArgs e) { Result = Choice.Send; DialogResult = true; Close(); }
    private void Close_Click(object sender, RoutedEventArgs e) { Result = Choice.Close; DialogResult = false; Close(); }
}
