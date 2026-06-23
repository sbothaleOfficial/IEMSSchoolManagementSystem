using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;

namespace IEMS.Infrastructure.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly ApplicationDbContext _context;

    public ClassRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Class?> GetByIdAsync(int id)
    {
        return await _context.Classes
            .Include(c => c.Teacher)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Class>> GetAllAsync()
    {
        return await _context.Classes
            .Include(c => c.Teacher)
            .ToListAsync();
    }

    public async Task<Class> AddAsync(Class entity)
    {
        _context.Classes.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Class entity)
    {
        await _context.MergeUpdateAsync(entity, entity.Id);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var classEntity = await _context.Classes.FindAsync(id);
        if (classEntity != null)
        {
            _context.Classes.Remove(classEntity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Class>> GetClassesByTeacherIdAsync(int teacherId)
    {
        return await _context.Classes
            .Include(c => c.Teacher)
            .Where(c => c.TeacherId == teacherId)
            .ToListAsync();
    }

    public async Task<Class?> GetClassWithStudentsAsync(int classId)
    {
        return await _context.Classes
            .Include(c => c.Teacher)
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == classId);
    }
}