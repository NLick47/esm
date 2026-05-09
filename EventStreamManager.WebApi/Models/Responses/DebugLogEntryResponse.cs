namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 调试日志条目响应
/// </summary>
public class DebugLogEntryResponse
{
    public string Type { get; set; } = "info";
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
