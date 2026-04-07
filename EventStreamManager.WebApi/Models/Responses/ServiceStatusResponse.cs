namespace EventStreamManager.WebApi.Models.Responses;

public class ServiceStatusResponse
{
    public bool IsEnabled { get; init; }
    public bool IsRunning { get; init; }
    public DateTime StartTime { get; init; }
    public TimeSpan RunningDuration { get; init; }
    public int ProcessorCount { get; init; }
    public int ActiveProcessorCount { get; init; }
    public IReadOnlyList<ProcessorStatusResponse> Processors { get; init; } = Array.Empty<ProcessorStatusResponse>();
}