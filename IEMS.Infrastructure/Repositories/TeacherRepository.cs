using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;

namespace IEMS.Infrastructure.Repositories;

public class TeacherRepository : ITeacherRepository
{
    private readonly ApplicationDbContext _context;

    public TeacherRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Teacher?> GetByIdAsync(int id)
    {
        return await _context.Teachers
            .Include(t => t.Classes)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Teacher>> GetAllAsync()
    {
        return await _context.Teachers
            .Include(t => t.Classes)
            .ToListAsync();
    }

    public async Task<Teacher> AddAsync(Teacher entity)
    {
        _context.Teachers.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Teacher entity)
    {
        await _context.MergeUpdateAsync(entity, entity.Id);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var teacher = await _context.Teachers.FindAsync(id);
        if (teacher != null)
        {
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<Teacher?> GetTeacherByEmployeeIdAsync(string employeeId)
    {
        return await _context.Teachers
            .Include(t => t.Classes)
            .FirstOrDefaultAsync(t => t.EmployeeId == employeeId);
    }

}