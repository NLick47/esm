using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Repositories;

public class EventHandleRepository : IEventHandleRepository
{
    private readonly ISqlSugarContext _db;

    public EventHandleRepository(ISqlSugarContext db)
    {
        _db = db;
    }

    public async Task<EventHandle?> GetByIdAsync(string databaseType, int id)
    {
        var client = await _db.GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.Id == id)
            .FirstAsync();
    }

    public async Task<EventHandle?> GetAsync(string databaseType, int eventId, string processorId)
    {
        var client = await _db.GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId && h.ProcessorId == processorId)
            .FirstAsync();
    }

    public async Task<EventHandle> CreateAsync(string databaseType, EventHandle handle)
    {
        var client = await _db.GetClientAsync(databaseType);
        handle.Id = await client.Insertable(handle).ExecuteReturnIdentityAsync();
        return handle;
    }

    public async Task<List<EventHandle>> GetByEventIdAsync(string databaseType, int eventId)
    {
        var client = await _db.GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId)
            .ToListAsync();
    }

    public async Task<EventHandleLog> CreateLogAsync(string databaseType, EventHandleLog log)
    {
        var client = await _db.GetClientAsync(databaseType);
        log.Id = await client.Insertable(log).ExecuteReturnIdentityAsync();
        return log;
    }

    public async Task UpdateAsync(string databaseType, EventHandle handle)
    {
        var client = await _db.GetClientAsync(databaseType);
        await client.Updateable(handle).ExecuteCommandAsync();
    }
}
