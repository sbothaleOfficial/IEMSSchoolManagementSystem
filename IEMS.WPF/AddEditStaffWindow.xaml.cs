using System.Windows;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AddEditStaffWindow : Window
{
    private readonly StaffService _staffService;
    private readonly StaffDto? _staffToEdit;

    public AddEditStaffWindow(StaffService staffService, StaffDto? staffToEdit = null)
    {
        InitializeComponent();
        _staffService = staffService;
        _staffToEdit = staffToEdit;

        LoadStaffData();

        Title = staffToEdit == null ? "Add Staff Member" : "Edit Staff Member";
    }

    private void LoadStaffData()
    {
        if (_staffToEdit != null)
        {
            txtEmployeeId.Text = _staffToEdit.EmployeeId;
            txtFirstName.Text = _staffToEdit.FirstName;
            txtLastName.Text = _staffToEdit.LastName;
            txtPhoneNumber.Text = _staffToEdit.PhoneNumber;
            txtAddress.Text = _staffToEdit.Address;
            dpJoiningDate.SelectedDate = _staffToEdit.JoiningDate;
            txtMonthlySalary.Text = _staffToEdit.MonthlySalary.ToString("F2");
            cmbPosition.Text = _staffToEdit.Position;

            txtEmail.Text = _staffToEdit.Email ?? "";
            txtBankAccount.Text = _staffToEdit.BankAccountNumber ?? "";
            txtAadharNumber.Text = _staffToEdit.AadharNumber ?? "";
            txtPANNumber.Text = _staffToEdit.PANNumber ?? "";
        }
        else
        {
            dpJoiningDate.SelectedDate = DateTime.Today;
        }
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput())
            return;

        AsyncHelper.SafeFireAndForget(SaveStaffAsync, "Save Staff Error");
    }

    private async Task SaveStaffAsync()
    {
        var staffDto = new StaffDto
        {
            Id = _staffToEdit?.Id ?? 0,
            EmployeeId = txtEmployeeId.Text.Trim(),
            FirstName = txtFirstName.Text.Trim(),
            LastName = txtLastName.Text.Trim(),
            PhoneNumber = txtPhoneNumber.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            JoiningDate = dpJoiningDate.SelectedDate ?? DateTime.Today,
            MonthlySalary = decimal.TryParse(txtMonthlySalary.Text.Trim(), out var salary) ? salary : 0,
            Position = cmbPosition.Text.Trim(),
            Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
            BankAccountNumber = string.IsNullOrWhiteSpace(txtBankAccount.Text) ? null : txtBankAccount.Text.Trim(),
            AadharNumber = string.IsNullOrWhiteSpace(txtAadharNumber.Text) ? null : txtAadharNumber.Text.Trim(),
            PANNumber = string.IsNullOrWhiteSpace(txtPANNumber.Text) ? null : txtPANNumber.Text.Trim()
        };

        // Check for unique employee ID
        if (!await _staffService.IsEmployeeIdUniqueAsync(staffDto.EmployeeId, staffDto.Id == 0 ? null : staffDto.Id))
        {
            MessageBox.Show("Employee ID already exists. Please choose a different one.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmployeeId.Focus();
            return;
        }

        if (_staffToEdit == null)
        {
            await _staffService.CreateStaffAsync(staffDto);
            MessageBox.Show("Staff member added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            await _staffService.UpdateStaffAsync(staffDto);
            MessageBox.Show("Staff member updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        DialogResult = true;
        Close();
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(txtEmployeeId.Text))
        {
            MessageBox.Show("Employee ID is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmployeeId.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtFirstName.Text))
        {
            MessageBox.Show("First name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtFirstName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtLastName.Text))
        {
            MessageBox.Show("Last name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtLastName.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtPhoneNumber.Text))
        {
            MessageBox.Show("Phone number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPhoneNumber.Focus();
            return false;
        }

        var phoneNumber = txtPhoneNumber.Text.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, @"^[6-9]\d{9}$"))
        {
            MessageBox.Show("Please enter a valid 10-digit Indian mobile number starting with 6-9.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPhoneNumber.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtAddress.Text))
        {
            MessageBox.Show("Address is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtAddress.Focus();
            return false;
        }

        if (!dpJoiningDate.SelectedDate.HasValue)
        {
            MessageBox.Show("Joining date is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            dpJoiningDate.Focus();
            return false;
        }

        if (dpJoiningDate.SelectedDate.Value > DateTime.Today)
        {
            MessageBox.Show("Joining date cannot be in the future.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            dpJoiningDate.Focus();
            return false;
        }

        if (!decimal.TryParse(txtMonthlySalary.Text.Trim(), out var salary) || salary <= 0)
        {
            MessageBox.Show("Please enter a valid monthly salary greater than zero.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtMonthlySalary.Focus();
            txtMonthlySalary.SelectAll();
            return false;
        }

        if (string.IsNullOrWhiteSpace(cmbPosition.Text))
        {
            MessageBox.Show("Position is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            cmbPosition.Focus();
            return false;
        }

        // Optional field validations
        if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text.Trim()))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmail.Focus();
            return false;
        }

        if (!string.IsNullOrWhiteSpace(txtAadharNumber.Text))
        {
            var aadhaar = txtAadharNumber.Text.Trim().Replace("-", "").Replace(" ", "");
            if (!System.Text.RegularExpressions.Regex.IsMatch(aadhaar, @"^\d{12}$"))
            {
                MessageBox.Show("Aadhaar number must be 12 digits (dashes optional).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtAadharNumber.Focus();
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(txtPANNumber.Text))
        {
            var pan = txtPANNumber.Text.Trim().ToUpper();
            if (!System.Text.RegularExpressions.Regex.IsMatch(pan, @"^[A-Z]{5}[0-9]{4}[A-Z]{1}$"))
            {
                MessageBox.Show("PAN number must be in format: 5 letters, 4 digits, 1 letter (e.g., ABCDE1234F).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPANNumber.Focus();
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(txtBankAccount.Text))
        {
            var bankAccount = txtBankAccount.Text.Trim();
            if (!System.Text.RegularExpressions.Regex.IsMatch(bankAccount, @"^\d{9,18}$"))
            {
                MessageBox.Show("Bank account number must be 9 to 18 digits.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtBankAccount.Focus();
                return false;
            }
        }

        return true;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}