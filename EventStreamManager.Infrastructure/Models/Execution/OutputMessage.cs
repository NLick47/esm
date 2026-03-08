namespace EventStreamManager.Infrastructure.Models.Execution;

public class OutputMessage
{
    /// <summary>
    /// 输出类型
    /// </summary>
    public string Type { get; set; } = "log";
    
    
    /// <summary>
    /// 消息内容
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
    
    
    /// <summary>
    /// 数据（如果是对象输出）
    /// </summary>
    public object? Data { get; set; }
}