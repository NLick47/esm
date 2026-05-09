namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 自定义SQL模板响应
/// </summary>
public class CustomSqlTemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
