using System.Windows;
using System.Windows.Controls;
using IEMS.Application.Services;
using IEMS.Application.DTOs;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

public partial class AddEditClassWindow : Window
{
    private readonly ClassService _classService;
    private readonly TeacherService _teacherService;
    private readonly ClassDto? _classToEdit;
    private readonly bool _isEditMode;

    public AddEditClassWindow(ClassService classService, TeacherService teacherService, ClassDto? classToEdit = null)
    {
        InitializeComponent();
        _classService = classService;
        _teacherService = teacherService;
        _classToEdit = classToEdit;
        _isEditMode = classToEdit != null;

        Title = _isEditMode ? "Edit Class" : "Add Class";
        LoadClassNames();
        // Populate the edit fields only AFTER the teacher list has loaded, otherwise the
        // async load finishes later and resets cmbTeacher.SelectedValue to 0 (teacher lost).
        AsyncHelper.SafeFireAndForget(InitializeAsync, "Load Teachers Error");
    }

    private async Task InitializeAsync()
    {
        await LoadTeachersAsync();

        if (_isEditMode && _classToEdit != null)
        {
            PopulateFields();
        }
    }

    private void LoadClassNames()
    {
        var classNames = new List<string>
        {
            "-- Select Class --",
            "Nursery",
            "KG1",
            "KG2",
            "Class 1",
            "Class 2",
            "Class 3",
            "Class 4",
            "Class 5",
            "Class 6",
            "Class 7",
            "Class 8",
            "Class 9",
            "Class 10"
        };

        cmbClassName.ItemsSource = classNames;
        cmbClassName.SelectedIndex = 0;
    }

    private async Task LoadTeachersAsync()
    {
        var teachers = await _classService.GetAvailableTeachersAsync();
        var teacherList = teachers.Select(t => new
        {
            Id = t.Id,
            DisplayName = $"{t.FirstName} {t.LastName} (ID: {t.EmployeeId})"
        }).ToList();

        teacherList.Insert(0, new { Id = 0, DisplayName = "-- Select Teacher --" });

        cmbTeacher.ItemsSource = teacherList;
        cmbTeacher.SelectedValue = 0;
    }

    private void PopulateFields()
    {
        if (_classToEdit == null) return;

        cmbClassName.SelectedItem = _classToEdit.Name;
        txtSection.Text = _classToEdit.Section;
        cmbTeacher.SelectedValue = _classToEdit.TeacherId;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateInput()) return;

        AsyncHelper.SafeFireAndForget(SaveClassAsync, "Save Class Error");
    }

    private async Task SaveClassAsync()
    {
        var classDto = new ClassDto
        {
            Id = _isEditMode ? _classToEdit!.Id : 0,
            Name = cmbClassName.SelectedItem?.ToString() ?? "",
            Section = string.IsNullOrWhiteSpace(txtSection.Text) ? "" : txtSection.Text.Trim(),
            TeacherId = (int)cmbTeacher.SelectedValue
        };

        var isUnique = await _classService.IsClassNameSectionUniqueAsync(
            classDto.Name,
            classDto.Section,
            _isEditMode ? classDto.Id : null);

        if (!isUnique)
        {
            ShowValidationError("A class with this name and section already exists.");
            return;
        }

        if (_isEditMode)
        {
            await _classService.UpdateClassAsync(classDto);
            MessageBox.Show("Class updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            await _classService.AddClassAsync(classDto);
            MessageBox.Show("Class added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        DialogResult = true;
        Close();
    }

    private bool ValidateInput()
    {
        if (cmbClassName.SelectedIndex <= 0 || cmbClassName.SelectedItem?.ToString() == "-- Select Class --")
        {
            ShowValidationError("Please select a class name.");
            cmbClassName.Focus();
            return false;
        }

        if (cmbTeacher.SelectedValue == null || (int)cmbTeacher.SelectedValue == 0)
        {
            ShowValidationError("Please select a teacher.");
            cmbTeacher.Focus();
            return false;
        }

        HideValidationError();
        return true;
    }

    private void ShowValidationError(string message)
    {
        lblValidation.Text = message;
        lblValidation.Visibility = Visibility.Visible;
    }

    private void HideValidationError()
    {
        lblValidation.Visibility = Visibility.Collapsed;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}