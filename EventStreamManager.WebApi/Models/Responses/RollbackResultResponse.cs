namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 版本回滚结果响应
/// </summary>
public class RollbackResultResponse
{
    public ProcessorVersionResponse Version { get; set; } = null!;
    public List<string> RecoveredTemplates { get; set; } = new();
    public List<string> MissingEventCodes { get; set; } = new();
    public bool HasWarnings { get; set; }
}
