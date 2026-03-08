namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 事件处理记录
/// </summary>
public class EventHandle
{
    /// <summary>
    /// 主键
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 事件Id
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// 处理类型
    /// </summary>
    public string HandleType { get; set; } = string.Empty;

    /// <summary>
    /// 处理类型说明
    /// </summary>
    public string? HandleTypeDes { get; set; }

    /// <summary>
    /// 处理次数
    /// </summary>
    public int HandleTimes { get; set; }

    /// <summary>
    /// 是否已完成
    /// </summary>
    public bool IsFinished { get; set; }

    /// <summary>
    /// 最后处理状态
    /// </summary>
    public string? LastHandleStatus { get; set; }

    /// <summary>
    /// 最后处理时间
    /// </summary>
    public DateTime? LastHandleDatetime { get; set; }

    /// <summary>
    /// 最后处理日志ID
    /// </summary>
    public int? LastHandleLogId { get; set; }
}