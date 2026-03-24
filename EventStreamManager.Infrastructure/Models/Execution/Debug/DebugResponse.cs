namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class DebugResponse : IDebugResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    
    /// <summary>
    /// 调试输出日志
    /// </summary>
    public List<DebugLogEntry> Logs { get; set; } = new();
    
    /// <summary>
    /// 处理结果
    /// </summary>
    public ProcessResultDto? Result { get; set; }
    
    
    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    
    /// <summary>
    /// 原始数据
    /// </summary>
    public object? RawData { get; set; }
}