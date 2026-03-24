using EventStreamManager.Infrastructure.Models.Execution.Parameter;

namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

/// <summary>
/// 编辑器调试结果
/// </summary>
public class EditorDebugResult : IDebugResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 执行时间(毫秒)
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// 原始数据
    /// </summary>
    public EnhancedQueryData RawData { get; set; } = new();

    /// <summary>
    /// 处理结果
    /// </summary>
    public ProcessResult? Result { get; set; }

    /// <summary>
    /// 调试日志
    /// </summary>
    public List<DebugLogEntry> Logs { get; set; } = new();

    /// <summary>
    /// 代码验证结果
    /// </summary>
    public CodeValidationResult? CodeValidation { get; set; }
}
