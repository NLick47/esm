namespace EventStreamManager.JSFunction.Runtime;

public class EnhancedQueryData
{
    /// <summary>
    /// 查询结果行
    /// </summary>
    public List<Dictionary<string, object>> Rows { get; set; } = new();
    
    /// <summary>
    /// 数据库信息
    /// </summary>
    public DatabaseInfo Database { get; set; } = new();
    
    
    /// <summary>
    /// 上下文信息
    /// </summary>
    public ContextInfo Context { get; set; } = new();
    
    /// <summary>
    /// 处理器信息（如果有）
    /// </summary>
    public ProcessorInfo? Processor { get; set; }
}
