using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Repositories;

public class FeeStructureRepository : IFeeStructureRepository
{
    private readonly ApplicationDbContext _context;

    public FeeStructureRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeeStructure>> GetAllAsync()
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Where(fs => fs.IsActive)
            .OrderBy(fs => fs.Class.Name)
            .ThenBy(fs => fs.FeeType)
            .ToListAsync();
    }

    public async Task<FeeStructure?> GetByIdAsync(int id)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .FirstOrDefaultAsync(fs => fs.Id == id);
    }

    public async Task<IEnumerable<FeeStructure>> GetByClassIdAsync(int classId)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Where(fs => fs.ClassId == classId && fs.IsActive)
            .OrderBy(fs => fs.FeeType)
            .ToListAsync();
    }

    // NEW: Methods using AcademicYearId foreign key
    public async Task<IEnumerable<FeeStructure>> GetByClassIdAndAcademicYearIdAsync(int classId, int academicYearId)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
            .Where(fs => fs.ClassId == classId && fs.AcademicYearId == academicYearId && fs.IsActive)
            .OrderBy(fs => fs.FeeType)
            .ToListAsync();
    }

    public async Task<FeeStructure?> GetByClassIdFeeTypeAndAcademicYearIdAsync(int classId, FeeType feeType, int academicYearId)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
            .FirstOrDefaultAsync(fs => fs.ClassId == classId && fs.FeeType == feeType && fs.AcademicYearId == academicYearId && fs.IsActive);
    }

    public async Task<IEnumerable<FeeStructure>> GetByAcademicYearIdAsync(int academicYearId)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
            .Where(fs => fs.AcademicYearId == academicYearId && fs.IsActive)
            .OrderBy(fs => fs.Class.Name)
            .ThenBy(fs => fs.FeeType)
            .ToListAsync();
    }

    // Note: intentionally does NOT filter on IsActive, so callers can find a soft-deleted
    // row occupying the unique (ClassId, FeeType, AcademicYearId) slot and reactivate it.
    public async Task<FeeStructure?> GetByClassFeeTypeYearIncludingInactiveAsync(int classId, FeeType feeType, int academicYearId)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
            .FirstOrDefaultAsync(fs => fs.ClassId == classId && fs.FeeType == feeType && fs.AcademicYearId == academicYearId);
    }

    // DEPRECATED: String-based methods kept for backward compatibility during migration
    [Obsolete("Use GetByClassIdAndAcademicYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<IEnumerable<FeeStructure>> GetByClassIdAndAcademicYearAsync(int classId, string academicYear)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
#pragma warning disable CS0618 // Type or member is obsolete
            .Where(fs => fs.ClassId == classId && fs.AcademicYearString == academicYear && fs.IsActive)
#pragma warning restore CS0618 // Type or member is obsolete
            .OrderBy(fs => fs.FeeType)
            .ToListAsync();
    }

    [Obsolete("Use GetByClassIdFeeTypeAndAcademicYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<FeeStructure?> GetByClassIdFeeTypeAndAcademicYearAsync(int classId, FeeType feeType, string academicYear)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
#pragma warning disable CS0618 // Type or member is obsolete
            .FirstOrDefaultAsync(fs => fs.ClassId == classId && fs.FeeType == feeType && fs.AcademicYearString == academicYear && fs.IsActive);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("Use GetByAcademicYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<IEnumerable<FeeStructure>> GetByAcademicYearAsync(string academicYear)
    {
        return await _context.FeeStructures
            .Include(fs => fs.Class)
            .Include(fs => fs.AcademicYear)
#pragma warning disable CS0618 // Type or member is obsolete
            .Where(fs => fs.AcademicYearString == academicYear && fs.IsActive)
#pragma warning restore CS0618 // Type or member is obsolete
            .OrderBy(fs => fs.Class.Name)
            .ThenBy(fs => fs.FeeType)
            .ToListAsync();
    }

    public async Task<FeeStructure> AddAsync(FeeStructure feeStructure)
    {
        _context.FeeStructures.Add(feeStructure);
        await _context.SaveChangesAsync();
        return feeStructure;
    }

    public async Task UpdateAsync(FeeStructure feeStructure)
    {
        feeStructure.UpdatedAt = DateTime.UtcNow;
        _context.FeeStructures.Update(feeStructure);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var feeStructure = await GetByIdAsync(id);
        if (feeStructure != null)
        {
            feeStructure.IsActive = false;
            feeStructure.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}