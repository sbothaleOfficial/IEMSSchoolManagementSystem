using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using IEMS.Core.Entities;
using IEMS.WPF.Helpers;

namespace IEMS.WPF;

/// <summary>
/// The "ID Cards" tab of the Staff &amp; Teachers window. Reuses the same card generator,
/// photo capture (file / scanner / phone) and size selector as the student ID cards. The only
/// employee-specific touch is a "Designation" line on the card front (teachers print "Teacher";
/// staff print their Position) plus a Blood Group the operator can set here.
/// </summary>
public partial class StaffManagementWindow
{
    /// <summary>One row in the ID-card people grid (teacher or staff, shown uniformly).</summary>
    private sealed class IdcPerson
    {
        public int Id { get; init; }
        public string EmployeeId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Designation { get; init; } = string.Empty;
        public bool IsTeacher { get; init; }
    }

    private static readonly string[] BloodGroups = { "—", "A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-" };

    private const string IdcCustomSizeLabel = "Custom size…";
    private IEMS.WPF.Pdf.IdCardSize _idcLastSize = IEMS.WPF.Pdf.IdCardSize.StandardCr80;
    private IEMS.WPF.Pdf.IdCardSize? _idcCustomSize;
    private bool _idcSuppressSizeChange;

    private IdcPerson? _idcSelected;
    private byte[]? _idcPhotoBytes;
    private bool _idcSuppressBloodChange;

    private bool ShowingTeachers => (cmbIdcType?.SelectedItem as ComboBoxItem)?.Content as string != "Other Staff";

    /// <summary>Called once teachers + staff are loaded; wires up the size list and fills the grid.</summary>
    private void InitIdCardTab()
    {
        if (cmbIdcSize.Items.Count == 0)
        {
            foreach (var sz in IEMS.WPF.Pdf.IdCardSize.Presets)
                cmbIdcSize.Items.Add(sz);
            cmbIdcSize.Items.Add(IdcCustomSizeLabel);
            cmbIdcSize.SelectedIndex = 0;
            _idcLastSize = IEMS.WPF.Pdf.IdCardSize.StandardCr80;
        }

        if (cmbIdcBloodGroup.Items.Count == 0)
            foreach (var bg in BloodGroups) cmbIdcBloodGroup.Items.Add(bg);

        RefreshIdcPeople();
    }

    private void RefreshIdcPeople()
    {
        if (dgIdcPeople == null) return;

        IEnumerable<IdcPerson> people = ShowingTeachers
            ? _allTeachers.Select(t => new IdcPerson
            {
                Id = t.Id, EmployeeId = t.EmployeeId, Name = t.FullName, Designation = "Teacher", IsTeacher = true
            })
            : _allStaff.Select(s => new IdcPerson
            {
                Id = s.Id, EmployeeId = s.EmployeeId, Name = s.FullName,
                Designation = string.IsNullOrWhiteSpace(s.Position) ? "Staff" : s.Position, IsTeacher = false
            });

        var search = txtIdcSearch?.Text?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLowerInvariant();
            people = people.Where(p => p.Name.ToLowerInvariant().Contains(q)
                                    || p.EmployeeId.ToLowerInvariant().Contains(q)
                                    || p.Designation.ToLowerInvariant().Contains(q));
        }

        dgIdcPeople.ItemsSource = people.OrderBy(p => p.Name).ToList();
    }

    private void CmbIdcType_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshIdcPeople();
    private void TxtIdcSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshIdcPeople();

    private void DgIdcPeople_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var person = dgIdcPeople.SelectedItem as IdcPerson;
        if (person == null)
        {
            ResetIdcPanel();
            return;
        }

        AsyncHelper.SafeFireAndForget(async () =>
        {
            byte[]? photo; string? bloodGroup;
            if (person.IsTeacher)
            {
                var t = await _teacherService.GetTeacherEntityByIdAsync(person.Id);
                if (t == null) { ResetIdcPanel(); return; }
                photo = t.Photo; bloodGroup = t.BloodGroup;
            }
            else
            {
                var s = await _staffService.GetStaffEntityByIdAsync(person.Id);
                if (s == null) { ResetIdcPanel(); return; }
                photo = s.Photo; bloodGroup = s.BloodGroup;
            }

            _idcSelected = person;
            _idcPhotoBytes = photo;
            lblIdcName.Text = person.Name;
            lblIdcDesignation.Text = $"{person.Designation}   •   {person.EmployeeId}";
            imgIdcPhoto.Source = PhotoHelper.Decode(photo);

            _idcSuppressBloodChange = true;
            cmbIdcBloodGroup.SelectedItem = string.IsNullOrWhiteSpace(bloodGroup) ? BloodGroups[0] : bloodGroup;
            if (cmbIdcBloodGroup.SelectedItem == null) cmbIdcBloodGroup.SelectedItem = BloodGroups[0];
            _idcSuppressBloodChange = false;

            cmbIdcBloodGroup.IsEnabled = true;
            btnIdcChoosePhoto.IsEnabled = true;
            btnIdcScanPhoto.IsEnabled = true;
            btnIdcPhonePhoto.IsEnabled = true;
            btnIdcRemovePhoto.IsEnabled = photo != null && photo.Length > 0;
        }, "Load Employee Error");
    }

    private void ResetIdcPanel()
    {
        _idcSelected = null;
        _idcPhotoBytes = null;
        lblIdcName.Text = "Select a person";
        lblIdcDesignation.Text = string.Empty;
        imgIdcPhoto.Source = null;
        _idcSuppressBloodChange = true;
        cmbIdcBloodGroup.SelectedItem = BloodGroups[0];
        _idcSuppressBloodChange = false;
        cmbIdcBloodGroup.IsEnabled = false;
        btnIdcChoosePhoto.IsEnabled = false;
        btnIdcScanPhoto.IsEnabled = false;
        btnIdcPhonePhoto.IsEnabled = false;
        btnIdcRemovePhoto.IsEnabled = false;
    }

    private string? SelectedBloodGroup
    {
        get
        {
            var bg = cmbIdcBloodGroup.SelectedItem as string;
            return string.IsNullOrWhiteSpace(bg) || bg == BloodGroups[0] ? null : bg;
        }
    }

    private void CmbIdcBloodGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_idcSuppressBloodChange || _idcSelected == null) return;
        SaveIdcCardInfo();
    }

    private void BtnIdcChoosePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idcSelected == null) return;
        try
        {
            var bytes = PhotoHelper.Pick();
            if (bytes == null) return;
            _idcPhotoBytes = PhotoHelper.NormalizeForCard(bytes);
            imgIdcPhoto.Source = PhotoHelper.Decode(_idcPhotoBytes);
            SaveIdcCardInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Invalid Image", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnIdcScanPhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idcSelected == null) return;
        try
        {
            var bytes = PhotoHelper.ScanFromScanner();
            if (bytes == null) return;
            _idcPhotoBytes = PhotoHelper.NormalizeForCard(bytes);
            imgIdcPhoto.Source = PhotoHelper.Decode(_idcPhotoBytes);
            SaveIdcCardInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Scanner", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnIdcPhonePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idcSelected == null) return;
        try
        {
            var file = IEMS.WPF.Services.PhoneTransfer.Capture(this, _idcSelected.Name, documentMode: false);
            if (file == null) return;
            _idcPhotoBytes = PhotoHelper.NormalizeForCard(file.Data);
            imgIdcPhoto.Source = PhotoHelper.Decode(_idcPhotoBytes);
            SaveIdcCardInfo();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not receive the phone photo: {ex.Message}", "Upload from Phone",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void BtnIdcRemovePhoto_Click(object sender, RoutedEventArgs e)
    {
        if (_idcSelected == null) return;
        _idcPhotoBytes = null;
        imgIdcPhoto.Source = null;
        btnIdcRemovePhoto.IsEnabled = false;
        SaveIdcCardInfo();
    }

    /// <summary>Persists the current photo + blood group for the selected person.</summary>
    private void SaveIdcCardInfo()
    {
        var person = _idcSelected;
        if (person == null) return;
        var photo = _idcPhotoBytes;
        var bloodGroup = SelectedBloodGroup;

        AsyncHelper.SafeFireAndForget(async () =>
        {
            if (person.IsTeacher)
                await _teacherService.UpdateTeacherCardInfoAsync(person.Id, photo, bloodGroup);
            else
                await _staffService.UpdateStaffCardInfoAsync(person.Id, photo, bloodGroup);

            btnIdcRemovePhoto.IsEnabled = photo != null && photo.Length > 0;
            lblStatus.Text = $"Saved ID-card details for {person.Name}.";
        }, "Save Card Info Error");
    }

    private IEMS.WPF.Pdf.IdCardSize GetSelectedIdcSize()
        => cmbIdcSize.SelectedItem as IEMS.WPF.Pdf.IdCardSize ?? IEMS.WPF.Pdf.IdCardSize.StandardCr80;

    private void CmbIdcSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_idcSuppressSizeChange) return;

        if (cmbIdcSize.SelectedItem is IEMS.WPF.Pdf.IdCardSize sz)
        {
            _idcLastSize = sz;
            return;
        }

        if (cmbIdcSize.SelectedItem as string == IdcCustomSizeLabel)
        {
            var dlg = new CustomCardSizeWindow(_idcLastSize.WidthMm, _idcLastSize.HeightMm) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                var custom = new IEMS.WPF.Pdf.IdCardSize(
                    $"Custom — {dlg.WidthMm:0.#} × {dlg.HeightMm:0.#} mm", (float)dlg.WidthMm, (float)dlg.HeightMm);

                _idcSuppressSizeChange = true;
                if (_idcCustomSize != null) cmbIdcSize.Items.Remove(_idcCustomSize);
                _idcCustomSize = custom;
                cmbIdcSize.Items.Insert(cmbIdcSize.Items.IndexOf(IdcCustomSizeLabel), custom);
                cmbIdcSize.SelectedItem = custom;
                _idcLastSize = custom;
                _idcSuppressSizeChange = false;
            }
            else
            {
                _idcSuppressSizeChange = true;
                cmbIdcSize.SelectedItem = _idcLastSize;
                _idcSuppressSizeChange = false;
            }
        }
    }

    private void BtnIdcGenerateSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = dgIdcPeople.SelectedItems.OfType<IdcPerson>().ToList();
        if (selected.Count == 0)
        {
            MessageBox.Show("Select one or more people (Ctrl/Shift-click) to print ID cards.",
                "No selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        GenerateIdcCards(selected, suggestedName: null);
    }

    private void BtnIdcGenerateAll_Click(object sender, RoutedEventArgs e)
    {
        var all = (dgIdcPeople.ItemsSource as IEnumerable<IdcPerson>)?.ToList() ?? new List<IdcPerson>();
        if (all.Count == 0)
        {
            MessageBox.Show("There is no one in the list to print.", "Nothing to print",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }
        var what = ShowingTeachers ? "Teachers" : "Staff";
        GenerateIdcCards(all, suggestedName: $"IDCards_{what}");
    }

    private void GenerateIdcCards(IReadOnlyList<IdcPerson> people, string? suggestedName)
    {
        var size = GetSelectedIdcSize();
        bool includeBack = chkIdcIncludeBack.IsChecked == true;

        AsyncHelper.SafeFireAndForget(async () =>
        {
            // Load full entities (with photos) off the UI thread.
            var raw = new List<IEMS.WPF.Pdf.IdCardData>();
            foreach (var p in people)
            {
                if (p.IsTeacher)
                {
                    var t = await _teacherService.GetTeacherEntityByIdAsync(p.Id);
                    if (t != null) raw.Add(BuildEmployeeCard(t.FullName, t.EmployeeId, "Teacher",
                        t.BloodGroup, t.PhoneNumber, t.Address, t.Photo));
                }
                else
                {
                    var s = await _staffService.GetStaffEntityByIdAsync(p.Id);
                    if (s != null) raw.Add(BuildEmployeeCard(s.FullName, s.EmployeeId,
                        string.IsNullOrWhiteSpace(s.Position) ? "Staff" : s.Position,
                        s.BloodGroup, s.PhoneNumber, s.Address, s.Photo));
                }
            }

            if (raw.Count == 0)
            {
                Dispatcher.Invoke(() => MessageBox.Show("Could not load the selected people.",
                    "ID Cards", MessageBoxButton.OK, MessageBoxImage.Warning));
                return;
            }

            var suggested = suggestedName
                ?? (raw.Count == 1 ? $"IDCard_{raw[0].StudentName.Replace(' ', '_')}" : $"IDCards_{raw.Count}_employees");

            // All image rendering (WPF) + PDF generation must run on the UI thread.
            Dispatcher.Invoke(() =>
            {
                var frontBg = IEMS.WPF.Pdf.CardArt.RenderFront(size.WidthMm, size.HeightMm);
                var backBg = includeBack ? IEMS.WPF.Pdf.CardArt.RenderBack(size.WidthMm, size.HeightMm) : null;
                var logo = BonafideCertificateWindow.LoadSchoolLogoBytes();
                var school = IEMS.WPF.Pdf.SchoolCardInfo.Default;

                var cards = new List<IEMS.WPF.Pdf.IdCardData>();
                foreach (var r in raw)
                {
                    byte[]? norm = null, rounded = null;
                    try
                    {
                        if (r.Photo != null && r.Photo.Length > 0)
                        {
                            norm = PhotoHelper.NormalizeForCard(r.Photo);
                            rounded = IEMS.WPF.Pdf.CardArt.RoundPhoto(norm);
                        }
                    }
                    catch { /* a bad photo shouldn't block the card */ }

                    cards.Add(r with { Photo = norm, PhotoRounded = rounded });
                }

                var document = new IEMS.WPF.Pdf.IdCardDocument(cards, school, logo, frontBg, backBg, includeBack, size);
                IEMS.WPF.Pdf.PdfExporter.SaveAndOpen(document, suggested);
            });
        }, "ID Card Error");
    }

    private static IEMS.WPF.Pdf.IdCardData BuildEmployeeCard(string name, string employeeId, string designation,
        string? bloodGroup, string? phone, string? address, byte[]? photo) => new()
    {
        StudentName = name,
        IdLabel = "Employee ID",
        StudentNumber = string.IsNullOrWhiteSpace(employeeId) ? "-" : employeeId,
        Designation = designation,
        BloodGroup = bloodGroup ?? string.Empty,
        Phone = phone ?? string.Empty,
        Address = address ?? string.Empty,
        Photo = photo
    };
}
