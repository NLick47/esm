using EventStreamManager.EventProcessor.Entities;

namespace EventStreamManager.EventProcessor.Services;

public interface IStateManagerService
{
    bool IsEnabled { get; }
    DateTime StartTime { get; init; }
    TimeSpan GetRunningDuration();
    ServiceStateSnapshot GetSnapshot();
    
    Task EnableAsync();
    Task DisableAsync();
    Task<bool> ToggleAsync();
    
    Task LoadStateAsync();
    Task SaveStateAsync();
    Task UpdateStartTimeAsync(DateTime startTime);
}