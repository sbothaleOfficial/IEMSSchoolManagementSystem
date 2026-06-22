using System;

namespace IEMS.Core.Entities;

public class Student
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

    // NEW: Foreign key to AcademicYear table for admission year
    public int? AdmissionAcademicYearId { get; set; }
    public virtual AcademicYear? AdmissionAcademicYear { get; set; }

    // DEPRECATED: Legacy string field
    [Obsolete("Use AdmissionAcademicYearId instead. This field will be removed in a future version.")]
    public string? AdmissionAcademicYearString { get; set; }

    public int ClassId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Class? Class { get; set; }
    public virtual ICollection<FeePayment> FeePayments { get; set; } = new List<FeePayment>();

    public string FullName => $"{FirstName} {Surname}".Trim();
    public string ClassWithDivision => !string.IsNullOrEmpty(ClassDivision)
        ? $"{Standard} ({ClassDivision})"
        : Standard;
    public string FormattedDateOfBirth => DateOfBirth.ToString("dd/MM/yyyy");
    public string FormattedAdmissionDate => AdmissionDate.ToString("dd/MM/yyyy");
}