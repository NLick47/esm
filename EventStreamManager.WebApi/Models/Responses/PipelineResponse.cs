namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 处理器管道响应
/// </summary>
public class PipelineResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EventCodes { get; set; } = new();
    public List<string> DatabaseTypes { get; set; } = new();
    public List<PipelineStageResponse> Stages { get; set; } = new();
    public bool Enabled { get; set; }
    public int MaxRetryCount { get; set; }
}
