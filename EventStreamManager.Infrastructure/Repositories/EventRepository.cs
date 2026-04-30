using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ISqlSugarContext _db;

    public EventRepository(ISqlSugarContext db)
    {
        _db = db;
    }

    public async Task<List<Event>> ScanUnprocessedAsync(
        string databaseType,
        string tableName,
        int batchSize,
        int? startId,
        DateTime? startTime,
        List<string>? eventCodes,
        List<string> processorIds)
    {
        var client = await _db.GetClientAsync(databaseType);

        var query = client.Queryable<Event>()
            .AS(tableName)
            .WhereIF(processorIds.Count > 0, e =>
                    SqlFunc.Subqueryable<EventHandle>()
                    .Where(h => h.EventId == e.Id && (h.IsFinished || h.IsDeadLetter) && processorIds.Contains(h.ProcessorId))
                    .Count() < processorIds.Count)
            .WhereIF(startId.HasValue, e => e.Id >= startId!.Value)
            .WhereIF(startTime.HasValue, e => e.CreateDatetime >= startTime!.Value)
            .WhereIF(eventCodes != null && eventCodes.Count > 0,
                e => eventCodes!.Contains(e.EventCode))
            .OrderBy(e => e.Id)
            .Take(batchSize);

        return await query.ToListAsync();
    }
}
