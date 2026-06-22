using IEMS.Application.Interfaces;
using IEMS.Core.Entities;
using IEMS.Core.Interfaces;
using System.ComponentModel;
using System.Globalization;

namespace IEMS.Application.Services
{
    public class SystemSettingsService : ISystemSettingsService
    {
        private readonly ISystemSettingRepository _repository;

        public SystemSettingsService(ISystemSettingRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<SystemSetting>> GetAllSettingsAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<IEnumerable<SystemSetting>> GetSettingsByCategoryAsync(string category)
        {
            return await _repository.GetByCategoryAsync(category);
        }

        public async Task<SystemSetting?> GetSettingAsync(string key)
        {
            return await _repository.GetByKeyAsync(key);
        }

        public async Task<string?> GetSettingValueAsync(string key)
        {
            var setting = await GetSettingAsync(key);
            return setting?.Value;
        }

        public async Task<T?> GetSettingValueAsync<T>(string key)
        {
            var value = await GetSettingValueAsync(key);
            if (string.IsNullOrEmpty(value))
                return default(T);

            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    return (T?)converter.ConvertFromString(null, CultureInfo.InvariantCulture, value);
                }

                if (typeof(T) == typeof(bool)) return (T)(object)bool.Parse(value);
                if (typeof(T) == typeof(int)) return (T)(object)int.Parse(value);
                if (typeof(T) == typeof(decimal)) return (T)(object)decimal.Parse(value, CultureInfo.InvariantCulture);
                if (typeof(T) == typeof(DateTime)) return (T)(object)DateTime.Parse(value, CultureInfo.InvariantCulture);

                return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error converting setting '{key}' to type {typeof(T).Name}: {ex.Message}");
                return default(T);
            }
        }

        public async Task<bool> UpdateSettingAsync(string key, string value)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            var setting = await _repository.GetByKeyAsync(key);
            if (setting == null || setting.IsReadOnly)
                return false;

            setting.Value = value;
            setting.ModifiedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(setting);
        }

        public async Task<bool> UpdateSettingsAsync(IEnumerable<SystemSetting> settings)
        {
            return await _repository.UpdateValuesAsync(settings);
        }

        public async Task<bool> ResetSettingToDefaultAsync(string key)
        {
            var setting = await _repository.GetByKeyAsync(key);
            if (setting == null || setting.IsReadOnly || string.IsNullOrEmpty(setting.DefaultValue))
                return false;

            setting.Value = setting.DefaultValue;
            setting.ModifiedAt = DateTime.UtcNow;
            return await _repository.UpdateAsync(setting);
        }

        public async Task<bool> ResetCategoryToDefaultAsync(string category)
        {
            var resettable = await _repository.GetResettableAsync(category);
            if (!resettable.Any())
                return false;

            foreach (var setting in resettable)
                setting.Value = setting.DefaultValue!;

            return await _repository.UpdateValuesAsync(resettable);
        }

        public async Task<bool> ResetAllToDefaultAsync()
        {
            var resettable = await _repository.GetResettableAsync(null);
            if (!resettable.Any())
                return false;

            foreach (var setting in resettable)
                setting.Value = setting.DefaultValue!;

            return await _repository.UpdateValuesAsync(resettable);
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _repository.GetCategoriesAsync();
        }
    }
}
