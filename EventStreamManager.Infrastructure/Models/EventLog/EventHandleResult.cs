namespace EventStreamManager.Infrastructure.Models.EventLog;

public class EventHandleResult
{
    /// <summary>
    /// 处理记录ID
    /// </summary>
    public int Id { get; set; }
    
    
    /// <summary>
    /// 事件ID
    /// </summary>
    public int EventId { get; set; }
    
    
    /// <summary>
    /// 事件码
    /// </summary>
    public string? EventCode { get; set; }
    
    /// <summary>
    /// 处理器ID
    /// </summary>
    public string ProcessorId { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 处理器名称
    /// </summary>
    public string ProcessorName { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 处理次数
    /// </summary>
    public int HandleTimes { get; set; }

    /// <summary>
    /// 最后处理状态
    /// </summary>
    public string LastHandleStatus { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 最后处理消息
    /// </summary>
    public string? LastHandleMessage { get; set; }
    
    
    /// <summary>
    /// 最后处理时间
    /// </summary>
    public DateTime? LastHandleDatetime { get; set; }

    
    
    /// <summary>
    /// 最后处理耗时（毫秒）
    /// </summary>
    public long? LastHandleElapsedMs { get; set; }
    
    
    /// <summary>
    /// 事件引用ID
    /// </summary>
    public string? StrEventReferenceId { get; set; }
    
    
    
    /// <summary>
    /// 请求数据（可考虑截断显示）
    /// </summary>
    public string? RequestData { get; set; }
    
    
    
    public string? ResponseData { get; set; }
    
    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsFinished { get; set; }

    /// <summary>
    /// 事件创建时间
    /// </summary>
    public DateTime CreateDatetime { get; set; }

    /// <summary>
    /// 事件名称
    /// </summary>
    public string? EventName { get; set; }
}