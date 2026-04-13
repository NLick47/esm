using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventListener;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IEventScanner
{
    Task<List<Event>> ScanAsync(string databaseType, EventConfig config, List<string>? eventCodes, List<string> processorIds);
}