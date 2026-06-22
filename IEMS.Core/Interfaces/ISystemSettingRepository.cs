using IEMS.Core.Entities;

namespace IEMS.Core.Interfaces
{
    /// <summary>
    /// Data access for system settings. Lets the application-layer SystemSettingsService work
    /// against an abstraction in Core instead of injecting the EF Core ApplicationDbContext
    /// directly (which had Application depending on Infrastructure).
    /// </summary>
    public interface ISystemSettingRepository
    {
        Task<List<SystemSetting>> GetAllAsync();
        Task<List<SystemSetting>> GetByCategoryAsync(string category);
        Task<SystemSetting?> GetByKeyAsync(string key);
        Task<List<string>> GetCategoriesAsync();

        /// <summary>Persists a single already-loaded/modified setting. Returns false on failure.</summary>
        Task<bool> UpdateAsync(SystemSetting setting);

        /// <summary>Applies value updates for the given keys atomically (one transaction). Skips read-only keys.</summary>
        Task<bool> UpdateValuesAsync(IEnumerable<SystemSetting> settings);

        /// <summary>
        /// Returns settings that can be reset to a default: not read-only and with a non-empty DefaultValue.
        /// Pass null for <paramref name="category"/> to span all categories.
        /// </summary>
        Task<List<SystemSetting>> GetResettableAsync(string? category);
    }
}
