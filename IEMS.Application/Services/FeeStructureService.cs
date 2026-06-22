using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.Application.DTOs;
using System;

namespace IEMS.Application.Services;

public class FeeStructureService
{
    private readonly IFeeStructureRepository _feeStructureRepository;
    private readonly IClassRepository _classRepository;
    private readonly IAcademicYearRepository _academicYearRepository;

    public FeeStructureService(
        IFeeStructureRepository feeStructureRepository,
        IClassRepository classRepository,
        IAcademicYearRepository academicYearRepository)
    {
        _feeStructureRepository = feeStructureRepository;
        _classRepository = classRepository;
        _academicYearRepository = academicYearRepository;
    }

    public async Task<IEnumerable<FeeStructureDto>> GetAllFeeStructuresAsync()
    {
        var feeStructures = await _feeStructureRepository.GetAllAsync();
        return feeStructures.Select(MapToDto);
    }

    public async Task<FeeStructureDto?> GetFeeStructureByIdAsync(int id)
    {
        var feeStructure = await _feeStructureRepository.GetByIdAsync(id);
        return feeStructure != null ? MapToDto(feeStructure) : null;
    }

    public async Task<IEnumerable<FeeStructureDto>> GetFeeStructuresByClassIdAsync(int classId)
    {
        var feeStructures = await _feeStructureRepository.GetByClassIdAsync(classId);
        return feeStructures.Select(MapToDto);
    }

    // NEW: Methods using AcademicYearId foreign key
    public async Task<IEnumerable<FeeStructureDto>> GetFeeStructuresByAcademicYearIdAsync(int academicYearId)
    {
        var feeStructures = await _feeStructureRepository.GetByAcademicYearIdAsync(academicYearId);
        return feeStructures.Select(MapToDto);
    }

    public async Task<FeeStructureDto?> GetFeeStructureByClassFeeTypeAndYearIdAsync(int classId, FeeType feeType, int academicYearId)
    {
        var feeStructure = await _feeStructureRepository.GetByClassIdFeeTypeAndAcademicYearIdAsync(classId, feeType, academicYearId);
        return feeStructure != null ? MapToDto(feeStructure) : null;
    }

    // DEPRECATED: String-based methods kept for backward compatibility during migration
    [Obsolete("Use GetFeeStructuresByAcademicYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<IEnumerable<FeeStructureDto>> GetFeeStructuresByAcademicYearAsync(string academicYear)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var feeStructures = await _feeStructureRepository.GetByAcademicYearAsync(academicYear);
#pragma warning restore CS0618 // Type or member is obsolete
        return feeStructures.Select(MapToDto);
    }

    [Obsolete("Use GetFeeStructureByClassFeeTypeAndYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<FeeStructureDto?> GetFeeStructureByClassFeeTypeAndYearAsync(int classId, FeeType feeType, string academicYear)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var feeStructure = await _feeStructureRepository.GetByClassIdFeeTypeAndAcademicYearAsync(classId, feeType, academicYear);
#pragma warning restore CS0618 // Type or member is obsolete
        return feeStructure != null ? MapToDto(feeStructure) : null;
    }

    public async Task<FeeStructureDto> CreateFeeStructureAsync(CreateFeeStructureDto createDto)
    {
        // Validate Class exists
        var classEntity = await _classRepository.GetByIdAsync(createDto.ClassId);
        if (classEntity == null)
            throw new ArgumentException("Class not found");

        // NEW: Validate AcademicYear exists
        var academicYear = await _academicYearRepository.GetByIdAsync(createDto.AcademicYearId);
        if (academicYear == null)
            throw new ArgumentException($"Academic Year with ID {createDto.AcademicYearId} not found");

        // Check for duplicate using the new ID-based method
        var existingStructure = await _feeStructureRepository.GetByClassIdFeeTypeAndAcademicYearIdAsync(
            createDto.ClassId, createDto.FeeType, createDto.AcademicYearId);

        if (existingStructure != null)
            throw new InvalidOperationException("Fee structure already exists for this class, fee type, and academic year");

        // A previously soft-deleted row still occupies the unique (ClassId, FeeType,
        // AcademicYearId) slot at the DB level. Reactivate and update it instead of
        // inserting a duplicate that would fail the unique constraint with a cryptic error.
        var softDeleted = await _feeStructureRepository.GetByClassFeeTypeYearIncludingInactiveAsync(
            createDto.ClassId, createDto.FeeType, createDto.AcademicYearId);
        if (softDeleted != null)
        {
            softDeleted.IsActive = true;
            softDeleted.Amount = createDto.Amount;
#pragma warning disable CS0618 // Type or member is obsolete
            softDeleted.AcademicYearString = academicYear.Year;
#pragma warning restore CS0618 // Type or member is obsolete
            softDeleted.Description = createDto.Description;
            softDeleted.UpdatedAt = DateTime.UtcNow;
            await _feeStructureRepository.UpdateAsync(softDeleted);
            var reactivated = await _feeStructureRepository.GetByIdAsync(softDeleted.Id);
            return MapToDto(reactivated!);
        }

        var feeStructure = new FeeStructure
        {
            ClassId = createDto.ClassId,
            FeeType = createDto.FeeType,
            Amount = createDto.Amount,
            AcademicYearId = createDto.AcademicYearId,
#pragma warning disable CS0618 // Type or member is obsolete
            AcademicYearString = academicYear.Year, // Populate legacy field for backward compatibility
#pragma warning restore CS0618 // Type or member is obsolete
            Description = createDto.Description,
            IsActive = true
        };

        var createdStructure = await _feeStructureRepository.AddAsync(feeStructure);
        var createdStructureWithIncludes = await _feeStructureRepository.GetByIdAsync(createdStructure.Id);
        return MapToDto(createdStructureWithIncludes!);
    }

    public async Task<FeeStructureDto> UpdateFeeStructureAsync(int id, CreateFeeStructureDto updateDto)
    {
        var feeStructure = await _feeStructureRepository.GetByIdAsync(id);
        if (feeStructure == null)
            throw new ArgumentException("Fee structure not found");

        // Validate Class exists
        var classEntity = await _classRepository.GetByIdAsync(updateDto.ClassId);
        if (classEntity == null)
            throw new ArgumentException("Class not found");

        // NEW: Validate AcademicYear exists
        var academicYear = await _academicYearRepository.GetByIdAsync(updateDto.AcademicYearId);
        if (academicYear == null)
            throw new ArgumentException($"Academic Year with ID {updateDto.AcademicYearId} not found");

        // Check for duplicate using the new ID-based method
        var existingStructure = await _feeStructureRepository.GetByClassIdFeeTypeAndAcademicYearIdAsync(
            updateDto.ClassId, updateDto.FeeType, updateDto.AcademicYearId);

        if (existingStructure != null && existingStructure.Id != id)
            throw new InvalidOperationException("Fee structure already exists for this class, fee type, and academic year");

        feeStructure.ClassId = updateDto.ClassId;
        feeStructure.FeeType = updateDto.FeeType;
        feeStructure.Amount = updateDto.Amount;
        feeStructure.AcademicYearId = updateDto.AcademicYearId;
#pragma warning disable CS0618 // Type or member is obsolete
        feeStructure.AcademicYearString = academicYear.Year; // Populate legacy field for backward compatibility
#pragma warning restore CS0618 // Type or member is obsolete
        feeStructure.Description = updateDto.Description;
        feeStructure.UpdatedAt = DateTime.UtcNow;

        await _feeStructureRepository.UpdateAsync(feeStructure);

        var updatedStructureWithIncludes = await _feeStructureRepository.GetByIdAsync(id);
        return MapToDto(updatedStructureWithIncludes!);
    }

    public async Task DeleteFeeStructureAsync(int id)
    {
        await _feeStructureRepository.DeleteAsync(id);
    }

    private static FeeStructureDto MapToDto(FeeStructure feeStructure)
    {
        return new FeeStructureDto
        {
            Id = feeStructure.Id,
            ClassId = feeStructure.ClassId,
            ClassName = feeStructure.Class?.Name + " - " + feeStructure.Class?.Section ?? "",
            FeeType = feeStructure.FeeType,
            Amount = feeStructure.Amount,

            // NEW: Map AcademicYearId and display
            AcademicYearId = feeStructure.AcademicYearId,
            AcademicYearDisplay = feeStructure.AcademicYear?.Year ?? "",

            // DEPRECATED: Legacy string field
#pragma warning disable CS0618 // Type or member is obsolete
            AcademicYear = feeStructure.AcademicYearString ?? feeStructure.AcademicYear?.Year ?? "",
#pragma warning restore CS0618 // Type or member is obsolete

            Description = feeStructure.Description,
            IsActive = feeStructure.IsActive
        };
    }
}