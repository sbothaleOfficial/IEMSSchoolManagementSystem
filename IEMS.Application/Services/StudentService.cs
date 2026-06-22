using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Application.Services;

public class StudentService
{
    private readonly IStudentRepository _studentRepository;
    private readonly IClassRepository _classRepository;

    public StudentService(IStudentRepository studentRepository, IClassRepository classRepository)
    {
        _studentRepository = studentRepository;
        _classRepository = classRepository;
    }

    public async Task<IEnumerable<StudentDto>> GetAllStudentsAsync()
    {
        var students = await _studentRepository.GetAllAsync();

        // Use LINQ projection to avoid N+1 queries - Class is already loaded via Include()
        return students.Select(student => new StudentDto
        {
            Id = student.Id,
            SerialNo = student.SerialNo,
            Standard = student.Standard,
            ClassDivision = student.ClassDivision,
            FirstName = student.FirstName,
            FatherName = student.FatherName,
            Surname = student.Surname,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            MotherName = student.MotherName,
            StudentNumber = student.StudentNumber,
            AdmissionDate = student.AdmissionDate,
            CasteCategory = student.CasteCategory,
            Religion = student.Religion,
            IsBPL = student.IsBPL,
            IsSemiEnglish = student.IsSemiEnglish,
            Address = student.Address,
            CityVillage = student.CityVillage,
            ParentMobileNumber = student.ParentMobileNumber,
            AadhaarNumber = student.AadhaarNumber,
            ClassId = student.ClassId,
            ClassName = student.Class?.Name ?? "Unknown"
        }).ToList();
    }

    public async Task<StudentDto?> GetStudentByIdAsync(int id)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student == null) return null;

        // Class is already loaded via Include() in repository - no need for separate query
        return new StudentDto
        {
            Id = student.Id,
            SerialNo = student.SerialNo,
            Standard = student.Standard,
            ClassDivision = student.ClassDivision,
            FirstName = student.FirstName,
            FatherName = student.FatherName,
            Surname = student.Surname,
            DateOfBirth = student.DateOfBirth,
            Gender = student.Gender,
            MotherName = student.MotherName,
            StudentNumber = student.StudentNumber,
            AdmissionDate = student.AdmissionDate,
            CasteCategory = student.CasteCategory,
            Religion = student.Religion,
            IsBPL = student.IsBPL,
            IsSemiEnglish = student.IsSemiEnglish,
            Address = student.Address,
            CityVillage = student.CityVillage,
            ParentMobileNumber = student.ParentMobileNumber,
            AadhaarNumber = student.AadhaarNumber,
            BloodGroup = student.BloodGroup,
            Photo = student.Photo,
            ClassId = student.ClassId,
            ClassName = student.Class?.Name ?? "Unknown"
        };
    }

    public async Task<Student> AddStudentAsync(StudentDto studentDto)
    {
        var student = new Student
        {
            SerialNo = studentDto.SerialNo,
            Standard = studentDto.Standard,
            ClassDivision = studentDto.ClassDivision,
            FirstName = studentDto.FirstName,
            FatherName = studentDto.FatherName,
            Surname = studentDto.Surname,
            DateOfBirth = studentDto.DateOfBirth,
            Gender = studentDto.Gender,
            MotherName = studentDto.MotherName,
            StudentNumber = studentDto.StudentNumber,
            AdmissionDate = studentDto.AdmissionDate,
            CasteCategory = studentDto.CasteCategory,
            Religion = studentDto.Religion,
            IsBPL = studentDto.IsBPL,
            IsSemiEnglish = studentDto.IsSemiEnglish,
            Address = studentDto.Address,
            CityVillage = studentDto.CityVillage,
            ParentMobileNumber = studentDto.ParentMobileNumber,
            AadhaarNumber = studentDto.AadhaarNumber,
            BloodGroup = studentDto.BloodGroup,
            Photo = studentDto.Photo,
            ClassId = studentDto.ClassId,
            AdmissionAcademicYearId = studentDto.AdmissionAcademicYearId
        };

        return await _studentRepository.AddAsync(student);
    }

    public async Task UpdateStudentAsync(StudentDto studentDto)
    {
        var student = await _studentRepository.GetByIdAsync(studentDto.Id);
        if (student != null)
        {
            student.SerialNo = studentDto.SerialNo;
            student.Standard = studentDto.Standard;
            student.ClassDivision = studentDto.ClassDivision;
            student.FirstName = studentDto.FirstName;
            student.FatherName = studentDto.FatherName;
            student.Surname = studentDto.Surname;
            student.DateOfBirth = studentDto.DateOfBirth;
            student.Gender = studentDto.Gender;
            student.MotherName = studentDto.MotherName;
            student.StudentNumber = studentDto.StudentNumber;
            student.AdmissionDate = studentDto.AdmissionDate;
            student.CasteCategory = studentDto.CasteCategory;
            student.Religion = studentDto.Religion;
            student.IsBPL = studentDto.IsBPL;
            student.IsSemiEnglish = studentDto.IsSemiEnglish;
            student.Address = studentDto.Address;
            student.CityVillage = studentDto.CityVillage;
            student.ParentMobileNumber = studentDto.ParentMobileNumber;
            student.AadhaarNumber = studentDto.AadhaarNumber;
            student.BloodGroup = studentDto.BloodGroup;
            student.Photo = studentDto.Photo;
            student.ClassId = studentDto.ClassId;
            student.AdmissionAcademicYearId = studentDto.AdmissionAcademicYearId;
            student.UpdatedAt = DateTime.UtcNow;

            await _studentRepository.UpdateAsync(student);
        }
    }

    public async Task DeleteStudentAsync(int id)
    {
        try
        {
            await _studentRepository.DeleteAsync(id);
        }
        catch (DbUpdateException)
        {
            // FeePayment and StudentPromotionHistory reference Student with DeleteBehavior.Restrict,
            // so a student with fee payments or promotion history can't be deleted. Surface a clear
            // message instead of a raw database error (consistent with Teacher/Class delete guards).
            throw new InvalidOperationException(
                "Cannot delete this student because related records exist (fee payments or promotion history). " +
                "Please remove those records first, or keep the student for historical reference.");
        }
    }

    public async Task<Student?> GetStudentEntityByIdAsync(int id)
    {
        return await _studentRepository.GetByIdAsync(id);
    }
}