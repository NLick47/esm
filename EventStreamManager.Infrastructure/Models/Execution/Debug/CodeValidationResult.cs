namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class CodeValidationResult
{
    /// <summary>
    /// 是否有process函数
    /// </summary>
    public bool HasProcessFunction { get; set; }

    /// <summary>
    /// 语法是否有效
    /// </summary>
    public bool SyntaxValid { get; set; }

    /// <summary>
    /// 警告列表
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// 错误列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
}