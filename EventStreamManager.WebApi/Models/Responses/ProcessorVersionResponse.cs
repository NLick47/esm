namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 处理器版本响应
/// </summary>
public class ProcessorVersionResponse
{
    public string Id { get; set; } = string.Empty;
    public string ProcessorId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string CommitMessage { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> DatabaseTypes { get; set; } = new();
    public List<string> EventCodes { get; set; } = new();
    public string Code { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public string SqlTemplateId { get; set; } = string.Empty;
    public string SqlTemplateType { get; set; } = string.Empty;
    public string SqlTemplateName { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
