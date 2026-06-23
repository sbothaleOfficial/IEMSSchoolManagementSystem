namespace IEMS.Core.Entities;

public class Staff
{
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; } = DateTime.UtcNow;
    public decimal MonthlySalary { get; set; } = 0;
    public string Position { get; set; } = string.Empty;

    public string? Email { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }

    // For the ID card: a passport photo (stored as a JPEG/PNG BLOB) and blood group.
    public byte[]? Photo { get; set; }
    public string? BloodGroup { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}";
}