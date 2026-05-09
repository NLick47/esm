using SqlSugar;

namespace EventStreamManager.Infrastructure.Entities;

// 事件处理日志详情表，专门存大文本，避免主日志表膨胀
[SugarTable("tblEventProcessLogDetail")]
public class EventHandleLogDetail
{
    // 对应 tblEventProcessLog.Id
    [SugarColumn(IsPrimaryKey = true, IsIdentity = false, ColumnName = "LogId", ColumnDescription = "日志ID")]
    public int LogId { get; set; }

    // 异常堆栈
    [SugarColumn(ColumnName = "ErrorStack", Length = int.MaxValue, IsNullable = true, ColumnDescription = "完整异常堆栈")]
    public string? ErrorStack { get; set; }

    // 脚本 console 输出
    [SugarColumn(ColumnName = "ConsoleOutput", Length = int.MaxValue, IsNullable = true, ColumnDescription = "脚本控制台输出")]
    public string? ConsoleOutput { get; set; }

    // JS 报错行号
    [SugarColumn(ColumnName = "ErrorLineNumber", IsNullable = true, ColumnDescription = "错误行号")]
    public int? ErrorLineNumber { get; set; }

    // JS 报错列号
    [SugarColumn(ColumnName = "ErrorColumn", IsNullable = true, ColumnDescription = "错误列号")]
    public int? ErrorColumn { get; set; }

    // 当时的处理器代码（复盘用）
    [SugarColumn(ColumnName = "ScriptSnapshot", Length = int.MaxValue, IsNullable = true, ColumnDescription = "处理器代码快照")]
    public string? ScriptSnapshot { get; set; }

    // 当时的输入数据
    [SugarColumn(ColumnName = "InputDataSnapshot", Length = int.MaxValue, IsNullable = true, ColumnDescription = "输入数据快照")]
    public string? InputDataSnapshot { get; set; }

    // 创建时间
    [SugarColumn(ColumnName = "CreateDatetime", IsNullable = false, ColumnDescription = "创建时间")]
    public DateTime CreateDatetime { get; set; } = DateTime.Now;
}
