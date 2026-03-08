namespace EventStreamManager.Infrastructure.Models.Execution;

public class BatchError
{
    /// <summary>
    /// 数据索引
    /// </summary>
    public int DataIndex { get; set; }

    /// <summary>
    /// 输入数据
    /// </summary>
    public object? InputData { get; set; }  
    
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }
}