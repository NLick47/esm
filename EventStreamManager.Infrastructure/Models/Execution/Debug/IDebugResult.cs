namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

/// <summary>
/// 调试结果接口
/// </summary>
public interface IDebugResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    bool Success { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    string? ErrorMessage { get; set; }
    
    /// <summary>
    /// 日志列表
    /// </summary>
    List<DebugLogEntry> Logs { get; set; }
    
    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    long ExecutionTimeMs { get; set; }
}
