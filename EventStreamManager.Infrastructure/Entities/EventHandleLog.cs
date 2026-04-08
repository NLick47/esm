using SqlSugar;

namespace EventStreamManager.Infrastructure.Entities;

/// <summary>
/// 事件处理日志
/// </summary>
[SugarTable("tblEventProcessLog")]
public class EventHandleLog
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Id", ColumnDescription = "主键")]
    public int Id { get; set; }

    /// <summary>
    /// 处理记录ID
    /// </summary>
    [SugarColumn(ColumnName = "EventHandleId", IsNullable = false, ColumnDescription = "处理记录ID")]
    public int EventHandleId { get; set; }

    /// <summary>
    /// 事件ID
    /// </summary>
    [SugarColumn(ColumnName = "EventId", IsNullable = false, ColumnDescription = "事件ID")]
    public int EventId { get; set; }

    /// <summary>
    /// 处理器ID
    /// </summary>
    [SugarColumn(ColumnName = "ProcessorId", ColumnDataType = "nvarchar", Length = 100, IsNullable = false, ColumnDescription = "处理器ID")]
    public string ProcessorId { get; set; } = string.Empty;

    /// <summary>
    /// 处理器名称
    /// </summary>
    [SugarColumn(ColumnName = "ProcessorName", ColumnDataType = "nvarchar", Length = 200, IsNullable = false, ColumnDescription = "处理器名称")]
    public string ProcessorName { get; set; } = string.Empty;

    /// <summary>
    /// 处理次数
    /// </summary>
    [SugarColumn(ColumnName = "HandleTimes", IsNullable = false, ColumnDescription = "处理次数")]
    public int HandleTimes { get; set; }

    /// <summary>
    /// 是否需要发送
    /// </summary>
    [SugarColumn(ColumnName = "NeedToSend", IsNullable = false, ColumnDescription = "是否需要发送")]
    public bool NeedToSend { get; set; }

    /// <summary>
    /// 请求信息
    /// </summary>
    [SugarColumn(ColumnName = "RequestData", ColumnDataType = "nvarchar", Length = int.MaxValue, IsNullable = true, ColumnDescription = "请求信息")]
    public string? RequestData { get; set; }

    /// <summary>
    /// 响应信息
    /// </summary>
    [SugarColumn(ColumnName = "ResponseData", ColumnDataType = "nvarchar", Length = int.MaxValue, IsNullable = true, ColumnDescription = "响应信息")]
    public string? ResponseData { get; set; }

    /// <summary>
    /// 发送是否成功
    /// </summary>
    [SugarColumn(ColumnName = "SendSuccess", IsNullable = true, ColumnDescription = "发送是否成功")]
    public bool? SendSuccess { get; set; }

    /// <summary>
    /// 异常信息
    /// </summary>
    [SugarColumn(ColumnName = "ExceptionMessage", ColumnDataType = "nvarchar", Length = int.MaxValue, IsNullable = true, ColumnDescription = "异常信息")]
    public string? ExceptionMessage { get; set; }

    /// <summary>
    /// 状态：Success/Fail/Exception
    /// </summary>
    [SugarColumn(ColumnName = "Status", ColumnDataType = "nvarchar", Length = 20, IsNullable = false, ColumnDescription = "状态")]
    public string Status { get; set; } = "Unhandled";

    /// <summary>
    /// 执行耗时（毫秒）
    /// </summary>
    [SugarColumn(ColumnName = "ExecutionTimeMs", IsNullable = false, ColumnDescription = "执行耗时")]
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 处理时间
    /// </summary>
    [SugarColumn(ColumnName = "HandleDatetime", IsNullable = false, ColumnDescription = "处理时间")]
    public DateTime HandleDatetime { get; set; } = DateTime.Now;
    
    
    
    /// <summary>
    /// 不发送原因
    /// </summary>
    [SugarColumn(ColumnName = "Reason", Length = 500, IsNullable = true, ColumnDescription = "不发送原因")]
    public string? Reason { get; set; }
}
