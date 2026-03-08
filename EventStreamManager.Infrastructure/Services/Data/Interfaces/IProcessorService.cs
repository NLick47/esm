using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IProcessorService
{
    Task<List<JSProcessor>> GetAllAsync();
    Task<JSProcessor?> GetByIdAsync(string id);
    Task<JSProcessor> CreateAsync(JSProcessor processor);
    Task<bool> UpdateAsync(string id, JSProcessor processor);
    Task<bool> DeleteAsync(string id);
    Task<JSProcessor?> ToggleAsync(string id);
    Task<string> GetDefaultTemplateAsync();
}