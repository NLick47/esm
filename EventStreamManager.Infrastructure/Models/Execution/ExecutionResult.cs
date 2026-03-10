namespace EventStreamManager.Infrastructure.Models.Execution;

public class ExecutionResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 返回值
    /// </summary>
    public object? ReturnValue { get; set; }
    
    
    /// <summary>
    /// 返回值类型
    /// </summary>
    public string? ReturnType { get; set; }
    
    
    /// <summary>
    /// 控制台输出列表
    /// </summary>
    public List<OutputMessage> Output { get; set; } = new();
    
    
    /// <summary>
    /// 控制台输出文本
    /// </summary>
    public string ConsoleOutput => string.Join(Environment.NewLine, Output.Select(o => $"[{o.Type}] {o.Message}"));

    
    /// <summary>
    /// 执行时间（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    
    /// <summary>
    /// 内存使用（字节）
    /// </summary>
    public long MemoryUsed { get; set; }
    
    
    /// <summary>
    /// 错误信息（如果失败）
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    
    /// <summary>
    /// 错误堆栈
    /// </summary>
    public string? ErrorStack { get; set; }
    
    
    /// <summary>
    /// 错误行号
    /// </summary>
    public int? ErrorLineNumber { get; set; }
    
    
    /// <summary>
    /// 错误列号
    /// </summary>
    public int? ErrorColumn { get; set; }

    
    
    /// <summary>
    /// 执行时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// 输入数据
    /// </summary>
    public object? InputData { get; set; }

    /// <summary>
    /// 标志位：是否需要发送到API
    /// </summary>
    public bool NeedToSend { get; set; }

    /// <summary>
    /// 原因：如果不需要发送，说明原因
    /// </summary>
    public string Reason { get; set; } = string.Empty;
    
    /// <summary>
    /// 异常信息：如果try catch捕获到异常
    /// </summary>
    public string? ProcessError { get; set; }
    
    /// <summary>
    /// 请求信息：正常时的请求数据
    /// </summary>
    public string? RequestInfo { get; set; }
}