namespace EventStreamManager.Infrastructure.Models.Interface;

/// <summary>
/// 处理器引用状态
/// </summary>
public class ProcessorReferenceStatus
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsReferenced { get; set; }
    public string? ReferencedByConfigId { get; set; }
    public string? ReferencedByConfigName { get; set; }
}
