using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Repositories;

public class EventHandleRepository : BaseRepository, IEventHandleRepository
{
    public EventHandleRepository(ISqlSugarContext db) : base(db)
    {
    }

    public async Task<EventHandle?> GetByIdAsync(string databaseType, int id)
    {
        using var client = await GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.Id == id)
            .FirstAsync();
    }

    public async Task<EventHandle?> GetAsync(string databaseType, int eventId, string processorId)
    {
        using var client = await GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId && h.ProcessorId == processorId)
            .FirstAsync();
    }

    public async Task<EventHandle> CreateAsync(string databaseType, EventHandle handle)
    {
        using var client = await GetClientAsync(databaseType);
        handle.Id = await client.Insertable(handle).ExecuteReturnIdentityAsync();
        return handle;
    }

    public async Task<List<EventHandle>> GetByEventIdAsync(string databaseType, int eventId)
    {
        using var client = await GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId)
            .ToListAsync();
    }

    public async Task<EventHandleLog> CreateLogAsync(string databaseType, EventHandleLog log)
    {
        using var client = await GetClientAsync(databaseType);
        log.Id = await client.Insertable(log).ExecuteReturnIdentityAsync();
        return log;
    }

    public async Task UpdateAsync(string databaseType, EventHandle handle)
    {
        using var client = await GetClientAsync(databaseType);
        await client.Updateable(handle).ExecuteCommandAsync();
    }
}
