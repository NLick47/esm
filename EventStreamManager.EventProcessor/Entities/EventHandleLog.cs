namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 事件处理日志
/// </summary>
public class EventHandleLog
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 处理记录ID
    /// </summary>
    public int EventHandleId { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    public int EventId { get; set; }

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
    /// 是否需要发送
    /// </summary>
    public bool NeedToSend { get; set; }

    /// <summary>
    /// 请求信息
    /// </summary>
    public string? RequestData { get; set; }

    /// <summary>
    /// 响应信息
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// 发送是否成功
    /// </summary>
    public bool? SendSuccess { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 状态：Success/Fail/Exception
    /// </summary>
    public string Status { get; set; } = HandleStatus.Unhandled;

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime HandleDatetime { get; set; } = DateTime.Now;
}