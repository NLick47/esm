namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class DebugLogEntry
{
    /// <summary>
    /// 日志类型 (info, warn, error, debug, output)
    /// </summary>
    public string Type { get; set; } = "info";
    
    /// <summary>
    /// 日志内容
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}