using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Application.DTOs;
using System.Text.RegularExpressions;

namespace IEMS.Application.Services;

public class AcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;
    private static readonly Regex AcademicYearFormatRegex = new Regex(@"^\d{4}-\d{2}$", RegexOptions.Compiled);

    public AcademicYearService(IAcademicYearRepository academicYearRepository)
    {
        _academicYearRepository = academicYearRepository;
    }

    public async Task<IEnumerable<AcademicYearDto>> GetAllAcademicYearsAsync()
    {
        var academicYears = await _academicYearRepository.GetAllAsync();
        return academicYears.Select(MapToDto).OrderByDescending(ay => ay.StartDate);
    }

    public async Task<AcademicYearDto?> GetAcademicYearByIdAsync(int id)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(id);
        return academicYear != null ? MapToDto(academicYear) : null;
    }

    public async Task<AcademicYearDto?> GetCurrentAcademicYearAsync()
    {
        var currentAcademicYear = await _academicYearRepository.GetCurrentAcademicYearAsync();
        return currentAcademicYear != null ? MapToDto(currentAcademicYear) : null;
    }

    public async Task<IEnumerable<AcademicYearDto>> GetRecentAcademicYearsAsync(int count = 5)
    {
        var recentYears = await _academicYearRepository.GetRecentAcademicYearsAsync(count);
        return recentYears.Select(MapToDto);
    }

    public async Task<AcademicYearDto> AddAcademicYearAsync(AcademicYearDto academicYearDto)
    {
        // Validate academic year format
        ValidateAcademicYearFormat(academicYearDto.Year);

        // Validate date range
        ValidateDateRange(academicYearDto.StartDate, academicYearDto.EndDate);

        // Check for duplicate year
        await CheckDuplicateYear(academicYearDto.Year);

        // If setting as current, unset all other current years
        if (academicYearDto.IsCurrent)
        {
            await UnsetAllCurrentYears();
        }

        var academicYear = new AcademicYear
        {
            Year = academicYearDto.Year,
            StartDate = academicYearDto.StartDate,
            EndDate = academicYearDto.EndDate,
            IsCurrent = academicYearDto.IsCurrent
        };

        var addedAcademicYear = await _academicYearRepository.AddAsync(academicYear);
        return MapToDto(addedAcademicYear);
    }

    public async Task<AcademicYearDto> UpdateAcademicYearAsync(AcademicYearDto academicYearDto)
    {
        var existingAcademicYear = await _academicYearRepository.GetByIdAsync(academicYearDto.Id);
        if (existingAcademicYear == null)
        {
            throw new ArgumentException($"Academic year with ID {academicYearDto.Id} not found.");
        }

        // Validate academic year format
        ValidateAcademicYearFormat(academicYearDto.Year);

        // Validate date range
        ValidateDateRange(academicYearDto.StartDate, academicYearDto.EndDate);

        // Check for duplicate year (excluding current record)
        await CheckDuplicateYear(academicYearDto.Year, academicYearDto.Id);

        // If setting as current, unset all other current years
        if (academicYearDto.IsCurrent && !existingAcademicYear.IsCurrent)
        {
            await UnsetAllCurrentYears();
        }

        existingAcademicYear.Year = academicYearDto.Year;
        existingAcademicYear.StartDate = academicYearDto.StartDate;
        existingAcademicYear.EndDate = academicYearDto.EndDate;
        existingAcademicYear.IsCurrent = academicYearDto.IsCurrent;
        existingAcademicYear.UpdatedAt = DateTime.UtcNow;

        await _academicYearRepository.UpdateAsync(existingAcademicYear);
        return MapToDto(existingAcademicYear);
    }

    public async Task DeleteAcademicYearAsync(int id)
    {
        var academicYear = await _academicYearRepository.GetByIdAsync(id);
        if (academicYear == null)
            throw new ArgumentException($"Academic year with ID {id} not found.");

        // Don't allow deleting the active year — it would leave the system with no current year.
        if (academicYear.IsCurrent)
            throw new InvalidOperationException("Cannot delete the current academic year. Set another year as current first.");

        await _academicYearRepository.DeleteAsync(id);
    }

    public async Task SetCurrentAcademicYearAsync(int academicYearId)
    {
        await _academicYearRepository.SetCurrentAcademicYearAsync(academicYearId);
    }

    // Validation Methods
    private static void ValidateAcademicYearFormat(string year)
    {
        if (string.IsNullOrWhiteSpace(year))
        {
            throw new ArgumentException("Academic year cannot be empty.");
        }

        if (!AcademicYearFormatRegex.IsMatch(year))
        {
            throw new ArgumentException($"Invalid academic year format: '{year}'. Expected format: YYYY-YY (e.g., 2024-25)");
        }

        // Validate that the two year parts are consecutive
        var parts = year.Split('-');
        if (parts.Length == 2)
        {
            if (int.TryParse(parts[0], out int startYear) && int.TryParse(parts[1], out int endYearShort))
            {
                int expectedEndYear = (startYear + 1) % 100;
                if (endYearShort != expectedEndYear)
                {
                    throw new ArgumentException($"Invalid academic year: '{year}'. Years must be consecutive (e.g., 2024-25, not 2024-26)");
                }
            }
        }
    }

    private static void ValidateDateRange(DateTime startDate, DateTime endDate)
    {
        if (endDate <= startDate)
        {
            throw new ArgumentException($"End date ({endDate:yyyy-MM-dd}) must be after start date ({startDate:yyyy-MM-dd})");
        }

        // Validate that it's approximately one academic year (9-15 months)
        var duration = endDate - startDate;
        if (duration.TotalDays < 270 || duration.TotalDays > 450)
        {
            throw new ArgumentException($"Academic year duration must be between 9 and 15 months. Current duration: {duration.TotalDays:F0} days");
        }
    }

    private async Task CheckDuplicateYear(string year, int? excludeId = null)
    {
        var allYears = await _academicYearRepository.GetAllAsync();
        var duplicate = allYears.FirstOrDefault(ay => ay.Year == year && ay.Id != excludeId);

        if (duplicate != null)
        {
            throw new InvalidOperationException($"Academic year '{year}' already exists.");
        }
    }

    private async Task UnsetAllCurrentYears()
    {
        var allYears = await _academicYearRepository.GetAllAsync();
        foreach (var year in allYears.Where(y => y.IsCurrent))
        {
            year.IsCurrent = false;
            year.UpdatedAt = DateTime.UtcNow;
            await _academicYearRepository.UpdateAsync(year);
        }
    }

    private static AcademicYearDto MapToDto(AcademicYear academicYear)
    {
        return new AcademicYearDto
        {
            Id = academicYear.Id,
            Year = academicYear.Year,
            StartDate = academicYear.StartDate,
            EndDate = academicYear.EndDate,
            IsCurrent = academicYear.IsCurrent
        };
    }
}