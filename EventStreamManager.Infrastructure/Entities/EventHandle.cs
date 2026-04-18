using SqlSugar;

namespace EventStreamManager.Infrastructure.Entities;

/// <summary>
/// 事件处理记录
/// </summary>
[SugarTable("tblEventProcess")]
public class EventHandle
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Id", ColumnDescription = "主键")]
    public int Id { get; set; }

    /// <summary>
    /// 事件Id
    /// </summary>
    [SugarColumn(ColumnName = "EventId", IsNullable = false, ColumnDescription = "事件Id")]
    public int EventId { get; set; }

    /// <summary>
    /// 处理器ID
    /// </summary>
    [SugarColumn(ColumnName = "ProcessorId", Length = 100, IsNullable = false, ColumnDescription = "处理器ID")]
    public string ProcessorId { get; set; } = string.Empty;

    /// <summary>
    /// 处理器名称
    /// </summary>
    [SugarColumn(ColumnName = "ProcessorName", Length = 200, IsNullable = false, ColumnDescription = "处理器名称")]
    public string ProcessorName { get; set; } = string.Empty;

    /// <summary>
    /// 处理次数
    /// </summary>
    [SugarColumn(ColumnName = "HandleTimes", IsNullable = false, ColumnDescription = "处理次数")]
    public int HandleTimes { get; set; }

    /// <summary>
    /// 是否已完成
    /// </summary>
    [SugarColumn(ColumnName = "IsFinished", IsNullable = false, ColumnDescription = "是否已完成")]
    public bool IsFinished { get; set; }

    /// <summary>
    /// 最后处理状态：Success/Fail/Exception
    /// </summary>
    [SugarColumn(ColumnName = "LastHandleStatus", Length = 20, IsNullable = true, ColumnDescription = "最后处理状态")]
    public string? LastHandleStatus { get; set; }

    /// <summary>
    /// 最后处理时间
    /// </summary>
    [SugarColumn(ColumnName = "LastHandleDatetime", IsNullable = true, ColumnDescription = "最后处理时间")]
    public DateTime? LastHandleDatetime { get; set; }

    /// <summary>
    /// 最后处理日志ID
    /// </summary>
    [SugarColumn(ColumnName = "LastHandleLogId", IsNullable = true, ColumnDescription = "最后处理日志ID")]
    public int? LastHandleLogId { get; set; }

    /// <summary>
    /// 是否已进入死信（超过最大重试次数）
    /// </summary>
    [SugarColumn(ColumnName = "IsDeadLetter", IsNullable = false, ColumnDescription = "是否死信")]
    public bool IsDeadLetter { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "CreateDatetime", IsNullable = false, ColumnDescription = "创建时间")]
    public DateTime CreateDatetime { get; set; } = DateTime.Now;
}