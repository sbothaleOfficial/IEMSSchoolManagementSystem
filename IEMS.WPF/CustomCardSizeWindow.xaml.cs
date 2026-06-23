using System.Globalization;
using System.Windows;

namespace IEMS.WPF;

public partial class CustomCardSizeWindow : Window
{
    public double WidthMm { get; private set; }
    public double HeightMm { get; private set; }

    public CustomCardSizeWindow(double initialWidthMm, double initialHeightMm)
    {
        InitializeComponent();
        txtWidth.Text = initialWidthMm.ToString("0.#", CultureInfo.InvariantCulture);
        txtHeight.Text = initialHeightMm.ToString("0.#", CultureInfo.InvariantCulture);
        Loaded += (_, _) => { txtWidth.Focus(); txtWidth.SelectAll(); };
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        txtError.Visibility = Visibility.Collapsed;

        if (!TryParse(txtWidth.Text, out var w) || !TryParse(txtHeight.Text, out var h))
        {
            ShowError("Please enter the width and height as numbers (millimetres).");
            return;
        }

        if (w < 35 || w > 150)
        {
            ShowError("Width must be between 35 and 150 mm.");
            return;
        }
        if (h < 45 || h > 200)
        {
            ShowError("Height must be between 45 and 200 mm.");
            return;
        }
        if (h < w)
        {
            ShowError("These are portrait cards, so the height should be at least the width.");
            return;
        }

        WidthMm = w;
        HeightMm = h;
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private static bool TryParse(string text, out double value) =>
        double.TryParse(text?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value);

    private void ShowError(string message)
    {
        txtError.Text = message;
        txtError.Visibility = Visibility.Visible;
    }
}
