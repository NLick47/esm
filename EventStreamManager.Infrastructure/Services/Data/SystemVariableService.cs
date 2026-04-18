using EventStreamManager.Infrastructure.Models.SystemVariable;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

/// <summary>
/// 系统变量服务，基于JSON文件持久化
/// </summary>
public class SystemVariableService : ISystemVariableService
{
    private readonly IDataService _dataService;
    private const string FileName = "system-variables.json";

    public SystemVariableService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<SystemVariable>> GetAllAsync()
    {
        return await _dataService.ReadAsync<SystemVariable>(FileName);
    }

    public async Task<SystemVariable?> GetByKeyAsync(string key)
    {
        var list = await _dataService.ReadAsync<SystemVariable>(FileName);
        return list.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<SystemVariable?> GetByIdAsync(string id)
    {
        var list = await _dataService.ReadAsync<SystemVariable>(FileName);
        return list.FirstOrDefault(x => x.Id == id);
    }

    public async Task<SystemVariable> SetAsync(string key, string value, string description = "", string category = "General")
    {
        var list = await _dataService.ReadAsync<SystemVariable>(FileName);
        var existing = list.FirstOrDefault(x => x.Key.Equals(key, StringComparison.OrdinalIgnoreCase));

        if (existing != null)
        {
            existing.Value = value;
            existing.Description = description;
            existing.Category = string.IsNullOrWhiteSpace(category) ? existing.Category : category;
            existing.UpdatedAt = DateTime.Now;
        }
        else
        {
            existing = new SystemVariable
            {
                Id = Guid.NewGuid().ToString(),
                Key = key,
                Value = value,
                Description = description,
                Category = string.IsNullOrWhiteSpace(category) ? "General" : category,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            list.Add(existing);
        }

        await _dataService.WriteAsync(FileName, list);
        return existing;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var list = await _dataService.ReadAsync<SystemVariable>(FileName);
        var newList = list.Where(x => x.Id != id).ToList();
        if (newList.Count == list.Count) return false;

        await _dataService.WriteAsync(FileName, newList);
        return true;
    }

    public async Task<bool> DeleteByKeyAsync(string key)
    {
        var list = await _dataService.ReadAsync<SystemVariable>(FileName);
        var newList = list.Where(x => !x.Key.Equals(key, StringComparison.OrdinalIgnoreCase)).ToList();
        if (newList.Count == list.Count) return false;

        await _dataService.WriteAsync(FileName, newList);
        return true;
    }
}
