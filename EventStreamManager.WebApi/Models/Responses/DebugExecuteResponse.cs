namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 调试执行响应
/// </summary>
public class DebugExecuteResponse
{
    public bool Success { get; set; }
    public List<DebugLogEntryResponse> Logs { get; set; } = new();
    public ProcessResultResponse? Result { get; set; }
    public long ExecutionTimeMs { get; set; }
    public string? ErrorMessage { get; set; }
    public object? RawData { get; set; }
}
