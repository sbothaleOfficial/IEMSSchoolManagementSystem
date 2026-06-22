using IEMS.Core.Entities;
using IEMS.Core.Enums;
using System;

namespace IEMS.Core.Interfaces;

public interface IFeeStructureRepository
{
    Task<IEnumerable<FeeStructure>> GetAllAsync();
    Task<FeeStructure?> GetByIdAsync(int id);
    Task<IEnumerable<FeeStructure>> GetByClassIdAsync(int classId);

    // NEW: Methods using AcademicYearId foreign key
    Task<IEnumerable<FeeStructure>> GetByClassIdAndAcademicYearIdAsync(int classId, int academicYearId);
    Task<FeeStructure?> GetByClassIdFeeTypeAndAcademicYearIdAsync(int classId, FeeType feeType, int academicYearId);
    Task<IEnumerable<FeeStructure>> GetByAcademicYearIdAsync(int academicYearId);

    // Includes soft-deleted (IsActive = false) rows — used to reactivate a previously
    // deleted fee structure instead of inserting a duplicate that violates the unique index.
    Task<FeeStructure?> GetByClassFeeTypeYearIncludingInactiveAsync(int classId, FeeType feeType, int academicYearId);

    // DEPRECATED: String-based methods kept for backward compatibility during migration
    [Obsolete("Use GetByClassIdAndAcademicYearIdAsync instead. This method will be removed in a future version.")]
    Task<IEnumerable<FeeStructure>> GetByClassIdAndAcademicYearAsync(int classId, string academicYear);
    [Obsolete("Use GetByClassIdFeeTypeAndAcademicYearIdAsync instead. This method will be removed in a future version.")]
    Task<FeeStructure?> GetByClassIdFeeTypeAndAcademicYearAsync(int classId, FeeType feeType, string academicYear);
    [Obsolete("Use GetByAcademicYearIdAsync instead. This method will be removed in a future version.")]
    Task<IEnumerable<FeeStructure>> GetByAcademicYearAsync(string academicYear);

    Task<FeeStructure> AddAsync(FeeStructure feeStructure);
    Task UpdateAsync(FeeStructure feeStructure);
    Task DeleteAsync(int id);
}