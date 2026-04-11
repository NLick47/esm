using EventStreamManager.EventProcessor.Entities;

namespace EventStreamManager.EventProcessor.Services;

public interface IProcessorManagerService
{
    bool IsRunning { get; }
    int ProcessorCount { get; }
    int ActiveProcessorCount { get; }
    
    Task InitializeAsync(CancellationToken ct);
    Task StartProcessorAsync(string dbType, CancellationToken ct);
    Task StopProcessorAsync(string dbType);
    Task StopAllAsync();
    Task RefreshConfigurationAsync(CancellationToken ct);
    Task TriggerScanAsync(string dbType, CancellationToken ct = default);
    
    IReadOnlyList<ProcessorStatus> GetAllStatus();
    ProcessorStatus? GetStatus(string databaseType);
    void StartBackgroundRefresh(TimeSpan refreshInterval, CancellationToken ct = default);
}