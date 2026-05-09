using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.Infrastructure.Repositories.Interfaces;

public interface IEventHandleLogDetailRepository
{
    Task<EventHandleLogDetail> CreateAsync(string databaseType, EventHandleLogDetail detail);
}
