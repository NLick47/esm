namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class EditorDebugRequest
{
    /// <summary>
    /// 处理器ID（如果是已存在的处理器）
    /// </summary>
    public string? ProcessorId { get; set; }

    /// <summary>
    /// JavaScript代码（如果是新建或临时调试）
    /// </summary>
    public string? JavaScriptCode { get; set; }

    /// <summary>
    /// ExamineID
    /// </summary>
    public string ExamineId { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DatabaseType { get; set; }

    /// <summary>
    /// SQL模板（可选）
    /// </summary>
    public string? SqlTemplate { get; set; }

    /// <summary>
    /// 是否验证代码
    /// </summary>
    public bool ValidateCode { get; set; } = true;
}