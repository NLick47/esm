using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.Infrastructure.Repositories.Interfaces;

public interface IEventRepository
{
    Task<List<Event>> ScanUnprocessedAsync(
        string databaseType,
        string tableName,
        int batchSize,
        int? startId,
        DateTime? startTime,
        List<string>? eventCodes,
        List<string> processorIds);
}
