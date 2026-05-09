namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 脚本验证结果响应
/// </summary>
public class ScriptValidationResponse
{
    public bool IsValid { get; set; }
    public string? Message { get; set; }
    public int? LineNumber { get; set; }
    public int? Column { get; set; }
    public string? Source { get; set; }
    public bool HasProcessFunction { get; set; }
}
