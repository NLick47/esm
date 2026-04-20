namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class JsProcessorVersion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProcessorId { get; set; } = string.Empty;
    public int Version { get; set; }
    public string CommitMessage { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public string SqlTemplateId { get; set; } = string.Empty;
    public SqlTemplateType SqlTemplateType { get; set; } = SqlTemplateType.System;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
