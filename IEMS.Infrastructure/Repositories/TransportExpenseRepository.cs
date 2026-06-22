using IEMS.Core.Entities;
using IEMS.Core.Enums;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IEMS.Infrastructure.Repositories;

public class TransportExpenseRepository : ITransportExpenseRepository
{
    private readonly ApplicationDbContext _context;

    public TransportExpenseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TransportExpense>> GetAllExpensesAsync()
    {
        return await _context.TransportExpenses
            .Include(e => e.Vehicle)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransportExpense>> GetExpensesByVehicleIdAsync(int vehicleId)
    {
        return await _context.TransportExpenses
            .Include(e => e.Vehicle)
            .Where(e => e.VehicleId == vehicleId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransportExpense>> GetExpensesByCategoryAsync(ExpenseCategory category)
    {
        return await _context.TransportExpenses
            .Include(e => e.Vehicle)
            .Where(e => e.Category == category)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<TransportExpense>> GetExpensesByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        return await _context.TransportExpenses
            .Include(e => e.Vehicle)
            .Where(e => e.ExpenseDate >= fromDate.Date && e.ExpenseDate < toDate.Date.AddDays(1))
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync();
    }

    public async Task<TransportExpense?> GetExpenseByIdAsync(int id)
    {
        return await _context.TransportExpenses
            .Include(e => e.Vehicle)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<TransportExpense> CreateExpenseAsync(TransportExpense expense)
    {
        expense.CreatedAt = DateTime.UtcNow;
        expense.UpdatedAt = DateTime.UtcNow;

        _context.TransportExpenses.Add(expense);
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task<TransportExpense> UpdateExpenseAsync(TransportExpense expense)
    {
        expense.UpdatedAt = DateTime.UtcNow;

        _context.Entry(expense).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return expense;
    }

    public async Task DeleteExpenseAsync(int id)
    {
        var expense = await _context.TransportExpenses.FindAsync(id);
        if (expense != null)
        {
            _context.TransportExpenses.Remove(expense);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<decimal> GetTotalExpensesByVehicleAsync(int vehicleId)
    {
        var expenses = await _context.TransportExpenses
            .Where(e => e.VehicleId == vehicleId)
            .Select(e => e.Amount)
            .ToListAsync();

        return expenses.Sum();
    }

    public async Task<decimal> GetTotalExpensesByCategoryAsync(ExpenseCategory category)
    {
        var expenses = await _context.TransportExpenses
            .Where(e => e.Category == category)
            .Select(e => e.Amount)
            .ToListAsync();

        return expenses.Sum();
    }

    public async Task<decimal> GetMonthlyExpensesByVehicleAsync(int vehicleId, int year, int month)
    {
        var expenses = await _context.TransportExpenses
            .Where(e => e.VehicleId == vehicleId &&
                       e.ExpenseDate.Year == year &&
                       e.ExpenseDate.Month == month)
            .Select(e => e.Amount)
            .ToListAsync();

        return expenses.Sum();
    }
}