using System.Windows;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AddEditTeacherWindow : Window
{
    private readonly TeacherService _teacherService;
    private readonly TeacherDto? _teacherToEdit;

    public AddEditTeacherWindow(TeacherService teacherService, TeacherDto? teacherToEdit = null)
    {
        InitializeComponent();
        _teacherService = teacherService;
        _teacherToEdit = teacherToEdit;

        LoadTeacherData();

        Title = teacherToEdit == null ? "Add Class Teacher" : "Edit Class Teacher";
    }

    private void LoadTeacherData()
    {
        if (_teacherToEdit != null)
        {
            txtEmployeeId.Text = _teacherToEdit.EmployeeId;
            txtFirstName.Text = _teacherToEdit.FirstName;
            txtLastName.Text = _teacherToEdit.LastName;
            txtPhoneNumber.Text = _teacherToEdit.PhoneNumber;
            txtAddress.Text = _teacherToEdit.Address;
            dpJoiningDate.SelectedDate = _teacherToEdit.JoiningDate;
            txtMonthlySalary.Text = _teacherToEdit.MonthlySalary.ToString("F2");

            txtEmail.Text = _teacherToEdit.Email ?? "";
            txtBankAccount.Text = _teacherToEdit.BankAccountNumber ?? "";
            txtAadharNumber.Text = _teacherToEdit.AadharNumber ?? "";
            txtPANNumber.Text = _teacherToEdit.PANNumber ?? "";
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

        AsyncHelper.SafeFireAndForget(SaveTeacherAsync, "Save Teacher Error");
    }

    private async Task SaveTeacherAsync()
    {
        var teacherDto = new TeacherDto
        {
            Id = _teacherToEdit?.Id ?? 0,
            EmployeeId = txtEmployeeId.Text.Trim(),
            FirstName = txtFirstName.Text.Trim(),
            LastName = txtLastName.Text.Trim(),
            PhoneNumber = txtPhoneNumber.Text.Trim(),
            Address = txtAddress.Text.Trim(),
            JoiningDate = dpJoiningDate.SelectedDate ?? DateTime.Today,
            MonthlySalary = decimal.TryParse(txtMonthlySalary.Text.Trim(), out var salary) ? salary : 0,
            Email = string.IsNullOrWhiteSpace(txtEmail.Text) ? null : txtEmail.Text.Trim(),
            BankAccountNumber = string.IsNullOrWhiteSpace(txtBankAccount.Text) ? null : txtBankAccount.Text.Trim(),
            AadharNumber = string.IsNullOrWhiteSpace(txtAadharNumber.Text) ? null : txtAadharNumber.Text.Trim(),
            PANNumber = string.IsNullOrWhiteSpace(txtPANNumber.Text) ? null : txtPANNumber.Text.Trim()
        };

        // Check for unique employee ID
        if (!await _teacherService.IsEmployeeIdUniqueAsync(teacherDto.EmployeeId, teacherDto.Id == 0 ? null : teacherDto.Id))
        {
            MessageBox.Show("Employee ID already exists. Please choose a different one.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmployeeId.Focus();
            return;
        }

        if (_teacherToEdit == null)
        {
            await _teacherService.AddTeacherAsync(teacherDto);
            MessageBox.Show("Class Teacher added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            await _teacherService.UpdateTeacherAsync(teacherDto);
            MessageBox.Show("Class Teacher updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        // Optional field validations
        if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text.Trim()))
        {
            MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEmail.Focus();
            return false;
        }

        if (!string.IsNullOrWhiteSpace(txtAadharNumber.Text))
        {
            // Accept dashes/spaces in the entered value (the data is stored/seeded as 1234-5678-9012)
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
            // Bank account numbers may include a bank/branch alpha prefix (e.g. SBI1234567890)
            if (!System.Text.RegularExpressions.Regex.IsMatch(bankAccount, @"^[A-Za-z0-9]{9,18}$"))
            {
                MessageBox.Show("Bank account number must be 9 to 18 letters or digits.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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