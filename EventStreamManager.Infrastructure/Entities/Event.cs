using SqlSugar;

namespace EventStreamManager.Infrastructure.Entities;

/// <summary>
/// 事件表
/// </summary>
[SugarTable("tblEvent")]
public class Event
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "Id", ColumnDescription = "主键")]
    public int Id { get; set; }

  
    [SugarColumn(ColumnName = "IntHospitalID",IsNullable = false, ColumnDescription = "医院ID")]
    public long IntHospitalId { get; set; }
    
    /// <summary>
    /// 事件类型
    /// </summary>
    [SugarColumn(ColumnName = "EventType", Length = 50, IsNullable = false, ColumnDescription = "事件类型")]
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// 事件引用ID
    /// </summary>
    [SugarColumn(ColumnName = "StrEventReferenceId", Length = 100, IsNullable = false, ColumnDescription = "事件引用ID")]
    public string StrEventReferenceId { get; set; } = string.Empty;

    /// <summary>
    /// 事件名称
    /// </summary>
    [SugarColumn(ColumnName = "EventName",  Length = 200, IsNullable = false, ColumnDescription = "事件名称")]
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// 事件代码
    /// </summary>
    [SugarColumn(ColumnName = "EventCode",Length = 50, IsNullable = false, ColumnDescription = "事件代码")]
    public string EventCode { get; set; } = string.Empty;

    /// <summary>
    /// 操作员名称
    /// </summary>
    [SugarColumn(ColumnName = "OperatorName", Length = 100, IsNullable = false, ColumnDescription = "操作员名称")]
    public string OperatorName { get; set; } = string.Empty;

    /// <summary>
    /// 操作员代码
    /// </summary>
    [SugarColumn(ColumnName = "OperatorCode",  Length = 50, IsNullable = false, ColumnDescription = "操作员代码")]
    public string OperatorCode { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "CreateDatetime", ColumnDescription = "创建时间")]
    public DateTime CreateDatetime { get; set; }

    /// <summary>
    /// 扩展数据
    /// </summary>
    [SugarColumn(ColumnName = "ExtenData", Length = int.MaxValue, IsNullable = true, ColumnDescription = "扩展数据")]
    public string? ExtenData { get; set; }

    /// <summary>
    /// 创建方式
    /// </summary>
    [SugarColumn(ColumnName = "CreateWay", ColumnDescription = "创建方式")]
    public byte CreateWay { get; set; }
}