namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 数据库连接测试响应
/// </summary>
public class ConnectionTestResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? ResponseTime { get; set; }
    public string? DatabaseVersion { get; set; }
}
