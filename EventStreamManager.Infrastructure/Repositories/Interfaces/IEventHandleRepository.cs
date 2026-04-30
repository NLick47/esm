using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.Infrastructure.Repositories.Interfaces;

public interface IEventHandleRepository
{
    Task<EventHandle?> GetByIdAsync(string databaseType, int id);

    Task<EventHandle?> GetAsync(string databaseType, int eventId, string processorId);

    Task<EventHandle> CreateAsync(string databaseType, EventHandle handle);

    Task<List<EventHandle>> GetByEventIdAsync(string databaseType, int eventId);

    Task<EventHandleLog> CreateLogAsync(string databaseType, EventHandleLog log);

    Task UpdateAsync(string databaseType, EventHandle handle);
}
