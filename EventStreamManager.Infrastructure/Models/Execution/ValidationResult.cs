namespace EventStreamManager.Infrastructure.Models.Execution;

public class ValidationResult
{
    
    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid { get; set; }
    
    
    /// <summary>
    /// 消息
    /// </summary>
    public string? Message { get; set; }
    
    
    /// <summary>
    /// 错误行号
    /// </summary>
    public int? LineNumber { get; set; }
    
    
    /// <summary>
    /// 错误列号
    /// </summary>
    public int? Column { get; set; }
    
    
    /// <summary>
    /// 错误来源
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// 是否包含process函数
    /// </summary>
    public bool HasProcessFunction { get; set; }
}