using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface ISqlTemplateService
{
    Task<List<SystemSqlTemplate>> GetSystemTemplatesAsync();
    Task<List<CustomSqlTemplate>> GetCustomTemplatesAsync();
    Task<CustomSqlTemplate> CreateCustomAsync(CustomSqlTemplate template);
    Task<bool> UpdateCustomAsync(string id, CustomSqlTemplate template);
    Task<bool> DeleteCustomAsync(string id);
    
    
    Task<string?> GetCustomSqlAsync(string templateId, Dictionary<string, object> parameters);
}