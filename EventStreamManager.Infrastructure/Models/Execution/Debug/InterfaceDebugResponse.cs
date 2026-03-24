namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class InterfaceDebugResponse : IDebugResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    
    /// <summary>
    /// 执行日志
    /// </summary>
    public List<DebugLogEntry> Logs { get; set; } = new();
    
    
    /// <summary>
    /// 总执行时间(ms)
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    
    /// <summary>
    /// 处理器执行时间(ms)
    /// </summary>
    public long? ProcessorExecutionTime { get; set; }
    
    
    /// <summary>
    /// 接口执行时间(ms)
    /// </summary>
    public long? InterfaceExecutionTime { get; set; }
    
    
    /// <summary>
    /// 处理器执行结果
    /// </summary>
    public ProcessResultDto? ProcessorResult { get; set; }

    
    /// <summary>
    /// 请求信息
    /// </summary>
    public RequestInfo? RequestInfo { get; set; }
    
    
    
    /// <summary>
    /// 响应信息
    /// </summary>
    public ResponseInfo? ResponseInfo { get; set; }
}