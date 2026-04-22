using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IProcessorVersionService
{
    Task<List<JsProcessorVersion>> GetVersionsAsync(string processorId);
    Task<JsProcessorVersion?> GetVersionAsync(string versionId);
    Task<JsProcessorVersion?> CommitAsync(string processorId, string commitMessage);
    Task<RollbackResult?> RollbackAsync(string processorId, string versionId, RollbackOptions? options = null);
}
