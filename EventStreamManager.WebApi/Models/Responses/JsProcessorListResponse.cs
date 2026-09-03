using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.WebApi.Models.Responses;

public class JsProcessorListResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> DatabaseTypes { get; set; } = new();
    public List<string> EventCodes { get; set; } = new();
    
    public SqlTemplateType SqlTemplateType { get; set; }
    public string SqlTemplateId { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 排序序号，数值越小越先执行
    /// </summary>
    public int SortOrder { get; set; }
}