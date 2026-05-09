using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Repositories;

public class EventHandleLogDetailRepository : IEventHandleLogDetailRepository
{
    private readonly ISqlSugarContext _db;

    public EventHandleLogDetailRepository(ISqlSugarContext db)
    {
        _db = db;
    }

    public async Task<EventHandleLogDetail> CreateAsync(string databaseType, EventHandleLogDetail detail)
    {
        using var client = await _db.GetClientAsync(databaseType);
        await client.Insertable(detail).ExecuteCommandAsync();
        return detail;
    }
}
