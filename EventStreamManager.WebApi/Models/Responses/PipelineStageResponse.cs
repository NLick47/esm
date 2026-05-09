namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 管道阶段响应
/// </summary>
public class PipelineStageResponse
{
    public string ProcessorId { get; set; } = string.Empty;
    public string ProcessorName { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsSender { get; set; }
    public string OnFailure { get; set; } = string.Empty;
    public string? Condition { get; set; }
}
