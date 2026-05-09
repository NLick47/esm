namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 数据库配置响应
/// </summary>
public class DatabaseConfigResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string Driver { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Timeout { get; set; }
}
