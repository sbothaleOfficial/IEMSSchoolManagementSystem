using Microsoft.EntityFrameworkCore;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using IEMS.Infrastructure.Data;

namespace IEMS.Infrastructure.Repositories
{
    public class SystemSettingRepository : ISystemSettingRepository
    {
        private readonly ApplicationDbContext _context;

        public SystemSettingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SystemSetting>> GetAllAsync()
        {
            return await _context.SystemSettings
                .OrderBy(s => s.Category)
                .ThenBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<List<SystemSetting>> GetByCategoryAsync(string category)
        {
            return await _context.SystemSettings
                .Where(s => s.Category == category)
                .OrderBy(s => s.Key)
                .ToListAsync();
        }

        public async Task<SystemSetting?> GetByKeyAsync(string key)
        {
            return await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _context.SystemSettings
                .Select(s => s.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<bool> UpdateAsync(SystemSetting setting)
        {
            try
            {
                await _context.MergeUpdateAsync(setting, setting.Key);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateValuesAsync(IEnumerable<SystemSetting> settings)
        {
            // Apply all updates atomically so a partial failure doesn't leave settings half-saved.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var keys = settings.Select(s => s.Key).ToList();
                var existing = await _context.SystemSettings
                    .Where(s => keys.Contains(s.Key))
                    .ToDictionaryAsync(s => s.Key);

                foreach (var update in settings)
                {
                    if (existing.TryGetValue(update.Key, out var row) && !row.IsReadOnly)
                    {
                        row.Value = update.Value;
                        row.ModifiedAt = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<SystemSetting>> GetResettableAsync(string? category)
        {
            var query = _context.SystemSettings
                .Where(s => !s.IsReadOnly && s.DefaultValue != null && s.DefaultValue != "");

            if (!string.IsNullOrEmpty(category))
                query = query.Where(s => s.Category == category);

            return await query.ToListAsync();
        }
    }
}
