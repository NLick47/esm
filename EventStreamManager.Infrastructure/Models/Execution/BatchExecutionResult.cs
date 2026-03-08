namespace EventStreamManager.Infrastructure.Models.Execution;

public class BatchExecutionResult
{
    /// <summary>
    /// 总体是否成功
    /// </summary>
    public bool Success => Errors.Count == 0;
    
    
    /// <summary>
    /// 成功执行的数量
    /// </summary>
    public int SuccessCount { get; set; }

    
    /// <summary>
    /// 失败的数量
    /// </summary>
    public int FailedCount => Errors.Count;
    
    
    /// <summary>
    /// 所有执行结果
    /// </summary>
    public List<ExecutionResult> Results { get; set; } = new();
    
    
    /// <summary>
    /// 错误列表
    /// </summary>
    public List<BatchError> Errors { get; set; } = new();

    /// <summary>
    /// 总执行时间（毫秒）
    /// </summary>
    public long TotalExecutionTimeMs { get; set; }

    /// <summary>
    /// 平均执行时间（毫秒）
    /// </summary>
    public double AverageExecutionTimeMs => Results.Count > 0 ? Results.Average(r => r.ExecutionTimeMs) : 0;
}