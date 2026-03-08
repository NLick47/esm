namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 处理器状态
/// </summary>
public class ProcessorStatus
{
    public string DatabaseType { get; set; } = string.Empty;
    public bool IsRunning { get; set; }
    public bool IsEnabled { get; set; }
    public DateTime? LastScanTime { get; set; }
    public int? LastProcessedEventId { get; set; }
    public int TotalProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int CurrentBatchCount { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastErrorTime { get; set; }
}