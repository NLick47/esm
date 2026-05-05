namespace EventStreamManager.Infrastructure.Models.Interface;

/// <summary>
/// 处理器引用冲突验证结果
/// </summary>
public class ReferenceValidationResult
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
