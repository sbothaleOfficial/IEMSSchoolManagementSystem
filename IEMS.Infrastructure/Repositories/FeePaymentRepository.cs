using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IEMS.Infrastructure.Repositories;

public class FeePaymentRepository : IFeePaymentRepository
{
    private readonly ApplicationDbContext _context;
    private static readonly SemaphoreSlim _receiptGenerationSemaphore = new(1, 1);

    public FeePaymentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<FeePayment>> GetAllAsync()
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    public async Task<FeePayment?> GetByIdAsync(int id)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .FirstOrDefaultAsync(fp => fp.Id == id);
    }

    public async Task<FeePayment?> GetByReceiptNumberAsync(string receiptNumber)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .FirstOrDefaultAsync(fp => fp.ReceiptNumber == receiptNumber);
    }

    public async Task<IEnumerable<FeePayment>> GetByStudentIdAsync(int studentId)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .Where(fp => fp.StudentId == studentId)
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FeePayment>> GetByStudentIdAndFeeTypeAsync(int studentId, FeeType feeType)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .Where(fp => fp.StudentId == studentId && fp.FeeType == feeType)
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    // NEW: Methods using AcademicYearId foreign key
    public async Task<IEnumerable<FeePayment>> GetByAcademicYearIdAsync(int academicYearId)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .Include(fp => fp.AcademicYear)
            .Where(fp => fp.AcademicYearId == academicYearId)
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    // DEPRECATED: String-based methods kept for backward compatibility during migration
    [Obsolete("Use GetByAcademicYearIdAsync instead. This method will be removed in a future version.")]
    public async Task<IEnumerable<FeePayment>> GetByAcademicYearAsync(string academicYear)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .Include(fp => fp.AcademicYear)
#pragma warning disable CS0618 // Type or member is obsolete
            .Where(fp => fp.AcademicYearString == academicYear)
#pragma warning restore CS0618 // Type or member is obsolete
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<FeePayment>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.FeePayments
            .Include(fp => fp.Student)
                .ThenInclude(s => s.Class)
            .Where(fp => fp.PaymentDate.Date >= fromDate.Date && fp.PaymentDate.Date <= toDate.Date)
            .OrderByDescending(fp => fp.PaymentDate)
            .ToListAsync();
    }

    public async Task<FeePayment> AddAsync(FeePayment feePayment)
    {
        _context.FeePayments.Add(feePayment);
        await _context.SaveChangesAsync();
        return feePayment;
    }

    public async Task UpdateAsync(FeePayment feePayment)
    {
        feePayment.UpdatedAt = DateTime.UtcNow;
        await _context.MergeUpdateAsync(feePayment, feePayment.Id);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var feePayment = await GetByIdAsync(id);
        if (feePayment != null)
        {
            _context.FeePayments.Remove(feePayment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<string> GenerateReceiptNumberAsync()
    {
        // Use semaphore to ensure thread-safe receipt number generation
        await _receiptGenerationSemaphore.WaitAsync();
        try
        {
            // Use a database transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // Base the next number on the highest *numeric* receipt number, not the
                // highest Id (legacy seed receipts like "REC001" aren't numeric and the
                // Id order doesn't track the receipt sequence).
                var existingReceipts = await _context.FeePayments
                    .Select(fp => fp.ReceiptNumber)
                    .ToListAsync();

                int maxNumber = 0;
                foreach (var r in existingReceipts)
                {
                    if (int.TryParse(r, out int n) && n > maxNumber)
                        maxNumber = n;
                }

                int nextNumber = maxNumber + 1;
                var receiptNumber = nextNumber.ToString("D6");

                // Loop until genuinely unique (handles legacy collisions / races)
                var taken = new HashSet<string>(existingReceipts);
                while (taken.Contains(receiptNumber))
                {
                    nextNumber++;
                    receiptNumber = nextNumber.ToString("D6");
                }

                await transaction.CommitAsync();
                return receiptNumber;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        finally
        {
            _receiptGenerationSemaphore.Release();
        }
    }

    public async Task<decimal> GetTotalPaidAmountByStudentAsync(int studentId, FeeType feeType)
    {
        var payments = await _context.FeePayments
            .Where(fp => fp.StudentId == studentId && fp.FeeType == feeType)
            .ToListAsync();

        return payments.Sum(fp => fp.AmountPaid);
    }

    public async Task<decimal> GetPendingAmountByStudentAsync(int studentId, FeeType feeType)
    {
        var lastPayment = await _context.FeePayments
            .Where(fp => fp.StudentId == studentId && fp.FeeType == feeType)
            .OrderByDescending(fp => fp.PaymentDate)
            .ThenByDescending(fp => fp.Id)
            .FirstOrDefaultAsync();

        return lastPayment?.RemainingBalance ?? 0;
    }
}