using IEMS.Core.Entities;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace IEMS.WPF
{
    public partial class BonafideCertificateWindow : Window
    {
        private readonly Student _student;

        public BonafideCertificateWindow(Student student)
        {
            InitializeComponent();
            _student = student;
            Loaded += BonafideCertificateWindow_Loaded;
        }

        private void BonafideCertificateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                PopulateCertificate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading certificate: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PopulateCertificate()
        {
            try
            {
                // Determine prefix based on gender
                string prefix = _student.Gender?.ToLower() == "female" ? "Miss." : "Mr.";
                string fullName = $"{_student.FirstName} {_student.Surname}".Trim();
                StudentNameWithPrefix.Text = $"{prefix} {fullName}";

                // Father's name
                FatherNameRun.Text = !string.IsNullOrEmpty(_student.FatherName)
                    ? $"Mr. {_student.FatherName}"
                    : "_________________";

                // Mother's name
                MotherNameRun.Text = !string.IsNullOrEmpty(_student.MotherName)
                    ? $"Mrs. {_student.MotherName}"
                    : "_________________";

                // Admission date formatting
                AdmissionDateRun.Text = _student.AdmissionDate.ToString("dd/MM/yyyy");

                // Standard with division
                string standard = _student.Standard;
                if (!string.IsNullOrEmpty(_student.ClassDivision))
                {
                    standard += $" ({_student.ClassDivision})";
                }
                StandardRun.Text = standard;

                // Student number
                StudentNumberRun.Text = !string.IsNullOrEmpty(_student.StudentNumber)
                    ? _student.StudentNumber
                    : "_______";

                // Date of birth
                DateOfBirthRun.Text = _student.DateOfBirth.ToString("dd/MM/yyyy");

                // Date of birth in words
                DateOfBirthInWordsRun.Text = ConvertDateToWords(_student.DateOfBirth);

                // Caste
                CasteRun.Text = !string.IsNullOrEmpty(_student.CasteCategory)
                    ? _student.CasteCategory
                    : "___________";

                // Religion
                ReligionRun.Text = !string.IsNullOrEmpty(_student.Religion)
                    ? _student.Religion
                    : "___________";

                // Current date for "Prepared by" section
                PreparedDateRun.Text = DateTime.Now.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error populating certificate: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string ConvertDateToWords(DateTime date)
        {
            try
            {
                var culture = new CultureInfo("en-US");
                string day = ConvertNumberToWords(date.Day);
                string month = date.ToString("MMMM", culture);
                string year = ConvertNumberToWords(date.Year);

                return $"{day} {month} {year}";
            }
            catch
            {
                return date.ToString("dd MMMM yyyy");
            }
        }

        private string ConvertNumberToWords(int number)
        {
            if (number == 0) return "Zero";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen",
                "Eighteen", "Nineteen" };

            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (number < 20)
                return ones[number];

            if (number < 100)
                return tens[number / 10] + (number % 10 > 0 ? " " + ones[number % 10] : "");

            if (number < 1000)
                return ones[number / 100] + " Hundred" + (number % 100 > 0 ? " " + ConvertNumberToWords(number % 100) : "");

            if (number < 100000)
                return ConvertNumberToWords(number / 1000) + " Thousand" + (number % 1000 > 0 ? " " + ConvertNumberToWords(number % 1000) : "");

            return number.ToString(); // Fallback for very large numbers
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PopulateCertificate();
                MessageBox.Show("Certificate generated successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating certificate: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Create a visual element for printing
                    var printVisual = CreatePrintVisual();

                    // Print the certificate
                    printDialog.PrintVisual(printVisual, $"Bonafide Certificate - {_student.FirstName} {_student.Surname}");

                    MessageBox.Show("Certificate sent to printer successfully!", "Print Success",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error printing certificate: {ex.Message}", "Print Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private FrameworkElement CreatePrintVisual()
        {
            // Create a copy of the certificate border for printing
            var printBorder = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(3),
                Padding = new Thickness(40),
                Width = 800,
                MinHeight = 600
            };

            var printContent = new StackPanel();

            // School Header
            var headerGrid = new Grid { Margin = new Thickness(0, 0, 0, 30) };
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(160) });
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // School Logo with border
            var logoBorder = new Border
            {
                Width = 120,
                Height = 120,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 30, 0),
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1)
            };
            var logoViewbox = new System.Windows.Controls.Viewbox
            {
                Stretch = Stretch.Uniform
            };
            var logoImage = new System.Windows.Controls.Image
            {
                Source = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/IEMS.WPF;component/Exact_color_logo.png")),
                Stretch = Stretch.Uniform
            };
            logoViewbox.Child = logoImage;
            logoBorder.Child = logoViewbox;
            Grid.SetColumn(logoBorder, 0);
            headerGrid.Children.Add(logoBorder);

            // School details
            var schoolDetailsPanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            schoolDetailsPanel.Children.Add(new TextBlock
            {
                Text = "INSPIRE ENGLISH MEDIUM SCHOOL, MARDI",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            });
            schoolDetailsPanel.Children.Add(new TextBlock
            {
                Text = "Tah. Maregaon, Dist. Yavatmal (Maharashtra) – 445303",
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            });

            var infoGrid = new Grid { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0) };
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            infoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            infoGrid.Children.Add(new TextBlock
            {
                Text = "U-Dise Code : 27140806704",
                FontSize = 12,
                Margin = new Thickness(0, 0, 40, 0)
            });
            var regNoText = new TextBlock
            {
                Text = "School Reg. No. MH-15381/16",
                FontSize = 12
            };
            Grid.SetColumn(regNoText, 1);
            infoGrid.Children.Add(regNoText);
            schoolDetailsPanel.Children.Add(infoGrid);

            Grid.SetColumn(schoolDetailsPanel, 1);
            headerGrid.Children.Add(schoolDetailsPanel);
            printContent.Children.Add(headerGrid);

            // Separator
            printContent.Children.Add(new Border
            {
                Height = 2,
                Background = Brushes.Black,
                Margin = new Thickness(0, 0, 0, 30)
            });

            // Title
            printContent.Children.Add(new TextBlock
            {
                Text = "BONAFIED CERTIFICATE",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 40),
                TextDecorations = TextDecorations.Underline
            });

            // Certificate content
            var contentPanel = new StackPanel { Margin = new Thickness(60, 0, 60, 0) };
            var contentText = new TextBlock
            {
                FontSize = 18,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 32,
                Margin = new Thickness(0, 0, 0, 10),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // Build the content text with proper formatting
            string prefix = _student.Gender?.ToLower() == "female" ? "Miss." : "Mr.";
            string fullName = $"{_student.FirstName} {_student.Surname}".Trim();
            string fatherName = !string.IsNullOrEmpty(_student.FatherName) ? $"Mr. {_student.FatherName}" : "_________________";
            string motherName = !string.IsNullOrEmpty(_student.MotherName) ? $"Mrs. {_student.MotherName}" : "_________________";
            string admissionDate = _student.AdmissionDate.ToString("dd/MM/yyyy");
            string standard = _student.Standard + (!string.IsNullOrEmpty(_student.ClassDivision) ? $" ({_student.ClassDivision})" : "");
            string studentNumber = !string.IsNullOrEmpty(_student.StudentNumber) ? _student.StudentNumber : "_______";
            string dateOfBirth = _student.DateOfBirth.ToString("dd/MM/yyyy");
            string dateOfBirthInWords = ConvertDateToWords(_student.DateOfBirth);
            string caste = !string.IsNullOrEmpty(_student.CasteCategory) ? _student.CasteCategory : "___________";
            string religion = !string.IsNullOrEmpty(_student.Religion) ? _student.Religion : "___________";

            contentText.Text = $"This is to Certify that {prefix} {fullName}\n" +
                              $"S/O, D/O {fatherName} and Mother {motherName}\n" +
                              $"is a Bonafide Student of this School Since {admissionDate} and now\n" +
                              $"He/She is Studying in Standard {standard} His/Her Reg./Roll No. {studentNumber}\n" +
                              $"The Date of Birth of this Student as per Record is {dateOfBirth} in\n" +
                              $"Words {dateOfBirthInWords}. His/Her Caste is {caste}\n" +
                              $"and in Religion {religion} According to School General Register.";

            contentPanel.Children.Add(contentText);
            printContent.Children.Add(contentPanel);

            // Footer
            var footerGrid = new Grid { Margin = new Thickness(0, 80, 0, 0) };
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            footerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var preparedPanel = new StackPanel();
            preparedPanel.Children.Add(new TextBlock { Text = "Prepared by,", FontSize = 14, Margin = new Thickness(0, 0, 0, 40) });
            preparedPanel.Children.Add(new TextBlock { Text = "Date : ", FontSize = 14 });
            preparedPanel.Children.Add(new TextBlock { Text = DateTime.Now.ToString("dd/MM/yyyy"), FontSize = 14, FontWeight = FontWeights.Bold });
            Grid.SetColumn(preparedPanel, 0);
            footerGrid.Children.Add(preparedPanel);

            var clarkText = new TextBlock
            {
                Text = "Clerk",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 60, 0, 0)
            };
            Grid.SetColumn(clarkText, 1);
            footerGrid.Children.Add(clarkText);

            var principalText = new TextBlock
            {
                Text = "Principal",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 60, 0, 0)
            };
            Grid.SetColumn(principalText, 2);
            footerGrid.Children.Add(principalText);

            printContent.Children.Add(footerGrid);
            printBorder.Child = printContent;

            // Measure and arrange for printing
            printBorder.Measure(new Size(800, double.PositiveInfinity));
            printBorder.Arrange(new Rect(0, 0, 800, printBorder.DesiredSize.Height));

            return printBorder;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}