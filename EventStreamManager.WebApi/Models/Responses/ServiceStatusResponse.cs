namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 服务状态响应
/// </summary>
public class ServiceStatusResponse
{
    public bool IsEnabled { get; init; }
    public bool IsRunning { get; init; }
    public DateTime StartTime { get; init; }
    public TimeSpan RunningDuration { get; init; }
    public int TotalProcessorCount { get; init; }
    public int ActiveProcessorCount { get; init; }
    public IReadOnlyList<ProcessorStatusResponse> Processors { get; init; } = Array.Empty<ProcessorStatusResponse>();
}
