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
    /// 处理次数
    /// </summary>
    public int HandleTimes { get; set; }

    /// <summary>
    /// 请求信息
    /// </summary>
    public string? RequestData { get; set; }

    /// <summary>
    /// 响应信息
    /// </summary>
    public string? ResponseData { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = HandleStatus.Unhandled;

    /// <summary>
    /// 处理时间
    /// </summary>
    public DateTime HandleDatetime { get; set; } = DateTime.Now;
}