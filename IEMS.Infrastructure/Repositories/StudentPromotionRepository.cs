using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;

namespace IEMS.Infrastructure.Repositories
{
    public class StudentPromotionRepository : IStudentPromotionRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentPromotionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task PromoteAsync(IEnumerable<Student> studentsToUpdate, IEnumerable<StudentPromotionHistory> historyToAdd)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Students.UpdateRange(studentsToUpdate);
                _context.StudentPromotionHistory.AddRange(historyToAdd);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<StudentPromotionHistory>> GetHistoryAsync(int fromClassId, int toClassId)
        {
            return await _context.StudentPromotionHistory
                .Where(h => h.FromClassId == fromClassId && h.ToClassId == toClassId)
                .ToListAsync();
        }

        public async Task RollbackAsync(IEnumerable<Student> studentsToRevert, IEnumerable<StudentPromotionHistory> historyToRemove)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Students.UpdateRange(studentsToRevert);
                _context.StudentPromotionHistory.RemoveRange(historyToRemove);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
