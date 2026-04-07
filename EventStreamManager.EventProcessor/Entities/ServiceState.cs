namespace EventStreamManager.EventProcessor.Entities;

public class ServiceState
{
    public bool IsEnabled { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Version { get; init; } = "1.0";
}