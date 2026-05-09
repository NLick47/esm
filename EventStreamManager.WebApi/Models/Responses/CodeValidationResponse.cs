namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 代码验证结果响应
/// </summary>
public class CodeValidationResponse
{
    public bool HasProcessFunction { get; set; }
    public bool SyntaxValid { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
