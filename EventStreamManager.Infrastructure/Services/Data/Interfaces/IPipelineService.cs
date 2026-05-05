using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IPipelineService
{
    Task<List<ProcessorPipeline>> GetAllAsync();
    Task<ProcessorPipeline?> GetByIdAsync(string id);
    Task<ProcessorPipeline?> GetMatchingAsync(string eventCode, string databaseType);
    Task<ProcessorPipeline> CreateAsync(ProcessorPipeline pipeline);
    Task<bool> UpdateAsync(string id, ProcessorPipeline pipeline);
    Task<bool> DeleteAsync(string id);
    Task<bool> ToggleAsync(string id);
}
