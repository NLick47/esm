using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.JSFunction.Runtime;
using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.Infrastructure.Services;

public interface IEventDataBuilderService
{
    Task<EnhancedQueryData> BuildEnhancedDataAsync(
        string databaseType,
        Event eventData,
        JsProcessor processor,
        CancellationToken ct = default);
    
    
    Task<List<Dictionary<string, object>>> ExecuteProcessorQueryAsync(
        string databaseType,
        string sqlTemplate,
        Event eventData,
        CancellationToken ct = default);


    Task<EnhancedQueryData> BuildEnhancedDataForExamineAsync(
        string databaseType,
        string sqlTemplate,
        string examineId,
        JsProcessor? processor,
        CancellationToken ct = default);
}