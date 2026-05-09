namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 处理器引用状态响应
/// </summary>
public class ProcessorReferenceStatusResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsReferenced { get; set; }
    public string? ReferencedByConfigId { get; set; }
    public string? ReferencedByConfigName { get; set; }
}
