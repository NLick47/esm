
using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IHandleRecorder
{
    Task<EventHandle> GetOrCreateAsync(string databaseType, int eventId, 
        string processorId, string processorName);
    Task<EventHandleLog> LogAsync(string databaseType, EventHandle handle, ExecutionResult result);
    Task MarkFinishedAsync(string databaseType, int handleId, string status, int logId);
    Task MarkFailedAsync(string databaseType, EventHandle handle, string status, int logId);
}