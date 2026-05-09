namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 处理器状态响应
/// </summary>
public class ProcessorStatusResponse
{
    public string DatabaseType { get; init; } = string.Empty;
    public bool IsRunning { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? LastScanTime { get; init; }
    public int? LastProcessedEventId { get; init; }
    public int TotalProcessedCount { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public int CurrentBatchCount { get; init; }
    public string? LastError { get; init; }
    public DateTime? LastErrorTime { get; init; }
}
