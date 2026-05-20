namespace EventStreamManager.JSFunction.Runtime;

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
    /// JavaScript 堆栈（JS 引擎层面）
    /// </summary>
    public string? JavaScriptStackTrace { get; set; }

    /// <summary>
    /// 出错源码上下文（包含前后行）
    /// </summary>
    public string? SourceContext { get; set; }

    /// <summary>
    /// 是否包含process函数
    /// </summary>
    public bool HasProcessFunction { get; set; }
}
