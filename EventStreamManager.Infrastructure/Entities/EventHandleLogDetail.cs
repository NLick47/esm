using SqlSugar;

namespace EventStreamManager.Infrastructure.Entities;

/// <summary>
///  事件处理日志详情
/// </summary>
[SugarTable("tblEventProcessLogDetail")]
public class EventHandleLogDetail
{
     /// <summary>
     /// 对应 tblEventProcessLog.Id
     /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "LogId", ColumnDescription = "日志ID")]
    public int LogId { get; set; }

    /// <summary>
    /// 异常堆栈
    /// </summary>
    [SugarColumn(ColumnName = "ErrorStack", Length = int.MaxValue, IsNullable = true, ColumnDescription = "完整异常堆栈")]
    public string? ErrorStack { get; set; }

    /// <summary>
    /// 脚本 console 输出
    /// </summary>
    [SugarColumn(ColumnName = "ConsoleOutput", Length = int.MaxValue, IsNullable = true, ColumnDescription = "脚本控制台输出")]
    public string? ConsoleOutput { get; set; }

    /// <summary>
    /// JS 报错行号
    /// </summary>
    [SugarColumn(ColumnName = "ErrorLineNumber", IsNullable = true, ColumnDescription = "错误行号")]
    public int? ErrorLineNumber { get; set; }

    /// <summary>
    /// JS 报错列号
    /// </summary>
    [SugarColumn(ColumnName = "ErrorColumn", IsNullable = true, ColumnDescription = "错误列号")]
    public int? ErrorColumn { get; set; }

    /// <summary>
    /// JavaScript 引擎层面的堆栈
    /// </summary>
    [SugarColumn(ColumnName = "ErrorJavaScriptStackTrace", Length = int.MaxValue, IsNullable = true, ColumnDescription = "JS 堆栈")]
    public string? ErrorJavaScriptStackTrace { get; set; }

    /// <summary>
    /// 出错源码上下文
    /// </summary>
    [SugarColumn(ColumnName = "ErrorSourceContext", Length = int.MaxValue, IsNullable = true, ColumnDescription = "源码上下文")]
    public string? ErrorSourceContext { get; set; }

    /// <summary>
    /// 当时的处理器代码
    /// </summary>
    [SugarColumn(ColumnName = "ScriptSnapshot", Length = int.MaxValue, IsNullable = true, ColumnDescription = "处理器代码快照")]
    public string? ScriptSnapshot { get; set; }

    /// <summary>
    /// 当时的输入数据
    /// </summary>
    [SugarColumn(ColumnName = "InputDataSnapshot", Length = int.MaxValue, IsNullable = true, ColumnDescription = "输入数据快照")]
    public string? InputDataSnapshot { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [SugarColumn(ColumnName = "CreateDatetime", IsNullable = false, ColumnDescription = "创建时间")]
    public DateTime CreateDatetime { get; set; } = DateTime.Now;
}
