using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using IEMS.Application.DTOs;
using IEMS.Application.Services;

namespace IEMS.WPF;

public partial class FeeReceiptWindow : Window
{
    private readonly FeeReceiptDto _receipt;
    private readonly FeePaymentService _feePaymentService;

    public FeeReceiptWindow(FeeReceiptDto receipt, FeePaymentService feePaymentService)
    {
        InitializeComponent();
        _receipt = receipt;
        _feePaymentService = feePaymentService;
        LoadReceiptData();
    }

    private void LoadReceiptData()
    {
        try
        {
            // School Information
            txtSchoolName.Text = _receipt.SchoolName;
            txtSchoolAddress.Text = _receipt.SchoolAddress;
            txtSchoolPhone.Text = $"Phone: {_receipt.SchoolPhone}";

            // Receipt Header
            txtReceiptNumber.Text = $"Receipt No: {_receipt.ReceiptNumber}";
            txtReceiptDate.Text = $"Date: {_receipt.ReceiptDate:dd/MM/yyyy}";
            txtAcademicYear.Text = $"Academic Year: {_receipt.AcademicYear}";

            // Student Details
            txtStudentName.Text = $"Name: {_receipt.StudentName}";
            txtClassName.Text = $"Class: {_receipt.ClassName}";
            txtStudentNumber.Text = $"Roll No: {_receipt.StudentNumber}";
            txtParentPhone.Text = $"Phone: {_receipt.ParentPhone}";

            // Payment Details
            txtFeeType.Text = $"Fee Type: {_receipt.FeeType}";
            txtPaymentMethod.Text = $"Payment Method: {_receipt.PaymentMethod}";
            txtAmountPaid.Text = $"Amount Paid: ₹{_receipt.AmountPaid:F2}";
            txtAmountInWords.Text = _receipt.AmountInWords;

            // Transaction Details
            var transactionDetails = "";
            switch (_receipt.PaymentMethod.ToString())
            {
                case "ONLINE":
                    if (!string.IsNullOrEmpty(_receipt.TransactionId))
                        transactionDetails = $"Transaction ID: {_receipt.TransactionId}";
                    break;
                case "CHEQUE":
                    if (!string.IsNullOrEmpty(_receipt.ChequeNumber))
                        transactionDetails = $"Cheque No: {_receipt.ChequeNumber}";
                    if (!string.IsNullOrEmpty(_receipt.BankName))
                        transactionDetails += $"\nBank: {_receipt.BankName}";
                    break;
                case "DD":
                    if (!string.IsNullOrEmpty(_receipt.ChequeNumber))
                        transactionDetails = $"DD No: {_receipt.ChequeNumber}";
                    if (!string.IsNullOrEmpty(_receipt.BankName))
                        transactionDetails += $"\nBank: {_receipt.BankName}";
                    break;
            }
            txtTransactionDetails.Text = transactionDetails;

            // Fee Summary
            txtTotalFees.Text = $"₹{_receipt.TotalFees:F2}";
            txtPreviousBalance.Text = $"₹{_receipt.PreviousBalance:F2}";
            txtPaymentAmount.Text = $"₹{_receipt.AmountPaid:F2}";
            txtRemainingBalance.Text = $"₹{_receipt.RemainingBalance:F2}";

            // Late Fee and Discount (show only if applicable)
            if (_receipt.LateFee > 0)
            {
                txtLateFeeDisplay.Text = "Late Fee:";
                txtLateFeeAmount.Text = $"₹{_receipt.LateFee:F2}";
                txtLateFeeDisplay.Visibility = Visibility.Visible;
                txtLateFeeAmount.Visibility = Visibility.Visible;
            }
            else
            {
                txtLateFeeDisplay.Visibility = Visibility.Collapsed;
                txtLateFeeAmount.Visibility = Visibility.Collapsed;
            }

            if (_receipt.Discount > 0)
            {
                txtDiscountDisplay.Text = "Discount:";
                txtDiscountAmount.Text = $"-₹{_receipt.Discount:F2}";
                txtDiscountDisplay.Visibility = Visibility.Visible;
                txtDiscountAmount.Visibility = Visibility.Visible;
            }
            else
            {
                txtDiscountDisplay.Visibility = Visibility.Collapsed;
                txtDiscountAmount.Visibility = Visibility.Collapsed;
            }

            // Additional Details (show only if applicable)
            var hasAdditionalDetails = false;

            if (_receipt.InstallmentNumber.HasValue)
            {
                txtInstallmentInfo.Text = $"Installment Number: {_receipt.InstallmentNumber}";
                hasAdditionalDetails = true;
            }

            if (_receipt.NextDueDate.HasValue)
            {
                txtNextDueDate.Text = $"Next Due Date: {_receipt.NextDueDate.Value:dd/MM/yyyy}";
                hasAdditionalDetails = true;
            }

            if (!string.IsNullOrEmpty(_receipt.PaymentNotes))
            {
                txtPaymentNotes.Text = $"Notes: {_receipt.PaymentNotes}";
                hasAdditionalDetails = true;
            }

            borderAdditionalDetails.Visibility = hasAdditionalDetails ? Visibility.Visible : Visibility.Collapsed;

            // Footer - generation info removed for cleaner receipt
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading receipt data: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnPrint_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                // Hide action buttons for printing
                var printButton = this.FindName("btnPrint") as Button;
                var buttonsGrid = printButton?.Parent as StackPanel;
                if (buttonsGrid != null)
                    buttonsGrid.Visibility = Visibility.Collapsed;

                // Set up the receipt content for printing
                var receiptToPrint = receiptBorder;
                receiptToPrint.Background = Brushes.White;

                // Calculate scaling
                double scale = Math.Min(
                    printDialog.PrintableAreaWidth / receiptToPrint.ActualWidth,
                    printDialog.PrintableAreaHeight / receiptToPrint.ActualHeight);

                // Apply scaling transform
                receiptToPrint.LayoutTransform = new ScaleTransform(scale, scale);

                // Print the receipt
                printDialog.PrintVisual(receiptToPrint, $"Fee Receipt - {_receipt.ReceiptNumber}");

                // Reset the transform
                receiptToPrint.LayoutTransform = null;

                // Show buttons again
                if (buttonsGrid != null)
                    buttonsGrid.Visibility = Visibility.Visible;

                MessageBox.Show("Receipt printed successfully!", "Print Complete",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error printing receipt: {ex.Message}", "Print Error",
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var document = new IEMS.WPF.Pdf.FeeReceiptDocument(_receipt);
        IEMS.WPF.Pdf.PdfExporter.SaveAndOpen(document, $"FeeReceipt_{_receipt.ReceiptNumber}");
    }

    private void BtnSendPhone_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var bytes = QuestPDF.Fluent.GenerateExtensions.GeneratePdf(new IEMS.WPF.Pdf.FeeReceiptDocument(_receipt));
            IEMS.WPF.Services.PhoneTransfer.Send(this, bytes,
                $"FeeReceipt_{_receipt.ReceiptNumber}.pdf", "application/pdf",
                $"Fee Receipt {_receipt.ReceiptNumber}");
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Could not prepare the receipt for sending: {ex.Message}",
                "Send to Phone", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}