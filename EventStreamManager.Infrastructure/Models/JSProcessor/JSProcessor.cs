using System.Text.Json.Serialization;

namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class JsProcessor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    
    public List<string> DatabaseTypes { get; set; } = new();
    
    public List<string> EventCodes { get; set; } = new();
    
    /// <summary>
    /// SQL模板ID 系统模板或自定义模板的ID
    /// </summary>
    public SqlTemplateType SqlTemplateType { get; set; } = SqlTemplateType.System;
    
    
    public string SqlTemplateId { get; set; } = string.Empty;
    
    /// <summary>
    /// sql内容 仅用于内部存储，不对外序列化
    /// </summary>
    [JsonIgnore]
    public string SqlTemplate { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
}