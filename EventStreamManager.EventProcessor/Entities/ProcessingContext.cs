namespace EventStreamManager.EventProcessor.Entities;

public class ProcessingContext
{
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DatabaseType { get; set; } = string.Empty;

    /// <summary>
    /// 事件数据
    /// </summary>
    public Event Event { get; set; } = new();

    /// <summary>
    /// 处理记录
    /// </summary>
    public EventHandle? EventHandle { get; set; }

    /// <summary>
    /// 扩展数据（SQL查询结果）
    /// </summary>
    public Dictionary<string, object>? ExtendedData { get; set; }
}