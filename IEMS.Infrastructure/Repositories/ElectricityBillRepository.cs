using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;

namespace IEMS.Infrastructure.Repositories;

public class ElectricityBillRepository : IElectricityBillRepository
{
    private readonly ApplicationDbContext _context;

    public ElectricityBillRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ElectricityBill?> GetByIdAsync(int id)
    {
        return await _context.ElectricityBills.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<ElectricityBill>> GetAllAsync()
    {
        return await _context.ElectricityBills.OrderByDescending(e => e.BillYear).ThenByDescending(e => e.BillMonth).ToListAsync();
    }

    public async Task<ElectricityBill> AddAsync(ElectricityBill entity)
    {
        _context.ElectricityBills.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(ElectricityBill entity)
    {
        _context.ElectricityBills.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bill = await _context.ElectricityBills.FindAsync(id);
        if (bill != null)
        {
            _context.ElectricityBills.Remove(bill);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<ElectricityBill?> GetByBillNumberAsync(string billNumber)
    {
        return await _context.ElectricityBills
            .FirstOrDefaultAsync(e => e.BillNumber == billNumber);
    }

    public async Task<ElectricityBill?> GetByMonthYearAsync(int month, int year)
    {
        return await _context.ElectricityBills
            .FirstOrDefaultAsync(e => e.BillMonth == month && e.BillYear == year);
    }

    public async Task<IEnumerable<ElectricityBill>> GetUnpaidBillsAsync()
    {
        return await _context.ElectricityBills
            .Where(e => !e.IsPaid)
            .OrderBy(e => e.DueDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ElectricityBill>> GetBillsByYearAsync(int year)
    {
        return await _context.ElectricityBills
            .Where(e => e.BillYear == year)
            .OrderBy(e => e.BillMonth)
            .ToListAsync();
    }

    public async Task<IEnumerable<ElectricityBill>> GetBillsByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.ElectricityBills
            .Where(e => e.DueDate >= fromDate.Date && e.DueDate < toDate.Date.AddDays(1))
            .OrderBy(e => e.DueDate)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalAmountByYearAsync(int year)
    {
        var bills = await _context.ElectricityBills
            .Where(e => e.BillYear == year)
            .ToListAsync();
        return bills.Sum(e => e.Amount);
    }

    public async Task<decimal> GetTotalAmountByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        var bills = await _context.ElectricityBills
            .Where(e => e.DueDate >= fromDate.Date && e.DueDate < toDate.Date.AddDays(1))
            .ToListAsync();
        return bills.Sum(e => e.Amount);
    }
}