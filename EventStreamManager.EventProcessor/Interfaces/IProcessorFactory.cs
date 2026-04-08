using EventStreamManager.EventProcessor.Processors;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IProcessorFactory
{
    DatabaseTypeProcessor Create(string databaseType);
    Task<List<string>> GetConfiguredTypesAsync();
}