using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Application.DTOs;

namespace IEMS.Application.Services;

public class StaffService
{
    private readonly IStaffRepository _staffRepository;

    public StaffService(IStaffRepository staffRepository)
    {
        _staffRepository = staffRepository;
    }

    public async Task<IEnumerable<StaffDto>> GetAllStaffAsync()
    {
        var staff = await _staffRepository.GetAllAsync();
        return staff.Select(MapToDto);
    }

    public async Task<StaffDto?> GetStaffByIdAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        return staff != null ? MapToDto(staff) : null;
    }

    public async Task<StaffDto?> GetStaffByEmployeeIdAsync(string employeeId)
    {
        var staff = await _staffRepository.GetStaffByEmployeeIdAsync(employeeId);
        return staff != null ? MapToDto(staff) : null;
    }


    public async Task<IEnumerable<StaffDto>> GetStaffByPositionAsync(string position)
    {
        var staff = await _staffRepository.GetStaffByPositionAsync(position);
        return staff.Select(MapToDto);
    }

    public async Task<StaffDto> CreateStaffAsync(StaffDto staffDto)
    {
        // Check if employee ID already exists
        var existingStaff = await _staffRepository.GetStaffByEmployeeIdAsync(staffDto.EmployeeId);

        if (existingStaff != null)
        {
            throw new ArgumentException($"Employee ID '{staffDto.EmployeeId}' already exists.");
        }

        var staff = new Staff
        {
            EmployeeId = staffDto.EmployeeId,
            FirstName = staffDto.FirstName,
            LastName = staffDto.LastName,
            PhoneNumber = staffDto.PhoneNumber,
            Address = staffDto.Address,
            JoiningDate = staffDto.JoiningDate,
            MonthlySalary = staffDto.MonthlySalary,
            Position = staffDto.Position,
            Email = staffDto.Email,
            BankAccountNumber = staffDto.BankAccountNumber,
            AadharNumber = staffDto.AadharNumber,
            PANNumber = staffDto.PANNumber,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _staffRepository.AddAsync(staff);
        return MapToDto(staff);
    }

    public async Task<StaffDto> UpdateStaffAsync(StaffDto staffDto)
    {
        var staff = await _staffRepository.GetByIdAsync(staffDto.Id);
        if (staff == null)
        {
            throw new ArgumentException($"Staff member with ID {staffDto.Id} not found.");
        }

        // Check if employee ID already exists for a different staff member
        var existingStaff = await _staffRepository.GetStaffByEmployeeIdAsync(staffDto.EmployeeId);
        if (existingStaff != null && existingStaff.Id != staffDto.Id)
        {
            throw new ArgumentException($"Employee ID '{staffDto.EmployeeId}' already exists.");
        }

        staff.EmployeeId = staffDto.EmployeeId;
        staff.FirstName = staffDto.FirstName;
        staff.LastName = staffDto.LastName;
        staff.PhoneNumber = staffDto.PhoneNumber;
        staff.Address = staffDto.Address;
        staff.JoiningDate = staffDto.JoiningDate;
        staff.MonthlySalary = staffDto.MonthlySalary;
        staff.Position = staffDto.Position;
        staff.Email = staffDto.Email;
        staff.BankAccountNumber = staffDto.BankAccountNumber;
        staff.AadharNumber = staffDto.AadharNumber;
        staff.PANNumber = staffDto.PANNumber;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);
        return MapToDto(staff);
    }

    public async Task DeleteStaffAsync(int id)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        if (staff == null)
        {
            throw new ArgumentException($"Staff member with ID {id} not found.");
        }

        await _staffRepository.DeleteAsync(id);
    }


    public async Task<IEnumerable<string>> GetPositionsAsync()
    {
        return await _staffRepository.GetPositionsAsync();
    }

    public async Task<int> GetTotalStaffCountAsync()
    {
        var allStaff = await _staffRepository.GetAllAsync();
        return allStaff.Count();
    }

    public async Task<bool> IsEmployeeIdUniqueAsync(string employeeId, int? excludeId = null)
    {
        var existingStaff = await _staffRepository.GetStaffByEmployeeIdAsync(employeeId);
        return existingStaff == null || (excludeId.HasValue && existingStaff.Id == excludeId.Value);
    }

    public async Task<IEnumerable<Staff>> GetAllAsync()
    {
        return await _staffRepository.GetAllAsync();
    }

    /// <summary>Full entity (incl. Photo + BloodGroup) for the ID card.</summary>
    public async Task<Staff?> GetStaffEntityByIdAsync(int id)
    {
        return await _staffRepository.GetByIdAsync(id);
    }

    /// <summary>
    /// Saves only the ID-card fields (photo + blood group) for one staff member, leaving every
    /// other field untouched (merge-update logs just the changed columns).
    /// </summary>
    public async Task UpdateStaffCardInfoAsync(int id, byte[]? photo, string? bloodGroup)
    {
        var staff = await _staffRepository.GetByIdAsync(id);
        if (staff == null) return;

        staff.Photo = photo;
        staff.BloodGroup = string.IsNullOrWhiteSpace(bloodGroup) ? null : bloodGroup;
        staff.UpdatedAt = DateTime.UtcNow;

        await _staffRepository.UpdateAsync(staff);
    }


    private static StaffDto MapToDto(Staff staff)
    {
        return new StaffDto
        {
            Id = staff.Id,
            EmployeeId = staff.EmployeeId,
            FirstName = staff.FirstName,
            LastName = staff.LastName,
            PhoneNumber = staff.PhoneNumber,
            Address = staff.Address,
            JoiningDate = staff.JoiningDate,
            MonthlySalary = staff.MonthlySalary,
            Position = staff.Position,
            Email = staff.Email,
            BankAccountNumber = staff.BankAccountNumber,
            AadharNumber = staff.AadharNumber,
            PANNumber = staff.PANNumber
        };
    }
}