using SqlSugar;

namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 事件处理记录
/// </summary>
[SugarTable("tblEventHandle")]
public class EventHandle
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public int Id { get; set; }

    /// <summary>
    /// 事件Id
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
    /// 是否已完成
    /// </summary>
    public bool IsFinished { get; set; }

    /// <summary>
    /// 最后处理状态：Success/Fail/Exception
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

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateDatetime { get; set; } = DateTime.Now;
}