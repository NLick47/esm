using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IProcessorService
{
    Task<List<JsProcessor>> GetAllAsync();
    Task<JsProcessor?> GetByIdAsync(string id);
    Task<JsProcessor> CreateAsync(JsProcessor processor);
    Task<bool> UpdateAsync(string id, JsProcessor processor);
    Task<bool> DeleteAsync(string id);
    Task<JsProcessor?> ToggleAsync(string id);
    Task<string> GetDefaultTemplateAsync();
}