namespace EventStreamManager.Infrastructure.Models.Execution;

public class ExecutionOptions
{
    /// <summary>
    /// 超时时间（秒）
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
    
    
    /// <summary>
    /// 最大语句数
    /// </summary>
    public int MaxStatements { get; set; } = 1000000;
    
    /// <summary>
    /// 最大递归深度
    /// </summary>
    public int MaxRecursionDepth { get; set; } = 64;

    
    
    /// <summary>
    /// 是否捕获控制台输出
    /// </summary>
    public bool CaptureConsoleOutput { get; set; } = true;
    
    
    /// <summary>
    /// 是否返回原始值
    /// </summary>
    public bool ReturnRawValue { get; set; } = false;
    
    
    /// <summary>
    /// 内存限制（MB）
    /// </summary>
    public int? MemoryLimitMb { get; set; }
}