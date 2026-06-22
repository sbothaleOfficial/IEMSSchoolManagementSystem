using System;

namespace IEMS.Application.DTOs;

public class StudentDto
{
    public int Id { get; set; }
    public int SerialNo { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string ClassDivision { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string FatherName { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MotherName { get; set; } = string.Empty;
    public string StudentNumber { get; set; } = string.Empty;
    public DateTime AdmissionDate { get; set; }

    // NEW: Foreign key to AcademicYear table for admission year (nullable)
    public int? AdmissionAcademicYearId { get; set; }
    public string? AdmissionAcademicYearDisplay { get; set; }

    public string CasteCategory { get; set; } = string.Empty;
    public string Religion { get; set; } = string.Empty;
    public bool IsBPL { get; set; }
    public bool IsSemiEnglish { get; set; }
    public string Address { get; set; } = string.Empty;
    public string CityVillage { get; set; } = string.Empty;
    public string ParentMobileNumber { get; set; } = string.Empty;
    public string? AadhaarNumber { get; set; }

    /// <summary>Blood group (e.g. "O+", "AB-"). Optional but important on an ID card for emergencies.</summary>
    public string? BloodGroup { get; set; }

    /// <summary>Optional passport-style photo (JPEG/PNG bytes) used on ID cards. Null = no photo.</summary>
    public byte[]? Photo { get; set; }

    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public decimal OutstandingFees { get; set; }
    public bool HasOutstandingFees { get; set; }

    public string FullName => $"{FirstName} {Surname}".Trim();
    public string FullNameWithAdmissionNumber => $"{FullName} - {StudentNumber}";
    public string ClassWithDivision => !string.IsNullOrEmpty(ClassDivision)
        ? $"{Standard} ({ClassDivision})"
        : Standard;
    public string FormattedDateOfBirth => DateOfBirth.ToString("dd/MM/yyyy");
    public string FormattedAdmissionDate => AdmissionDate.ToString("dd/MM/yyyy");
    public string FormattedOutstandingFees => HasOutstandingFees ? $"₹{OutstandingFees:N2}" : "₹0.00";
}