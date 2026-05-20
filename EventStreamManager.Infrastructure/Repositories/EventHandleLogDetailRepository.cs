using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Repositories;

public class EventHandleLogDetailRepository : BaseRepository, IEventHandleLogDetailRepository
{
    public EventHandleLogDetailRepository(ISqlSugarContext db) : base(db)
    {
    }

    public async Task<EventHandleLogDetail> CreateAsync(string databaseType, EventHandleLogDetail detail)
    {
        using var client = await GetClientAsync(databaseType);
        await client.Insertable(detail).ExecuteCommandAsync();
        return detail;
    }
}
