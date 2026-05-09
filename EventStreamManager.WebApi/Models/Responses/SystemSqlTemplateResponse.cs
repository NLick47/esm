namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 系统SQL模板响应
/// </summary>
public class SystemSqlTemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EventCodes { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
}
