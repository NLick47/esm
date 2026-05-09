namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 带激活配置的数据库类型响应
/// </summary>
public class DatabaseTypeWithActiveConfigResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public DatabaseConfigResponse? ActiveConfig { get; set; }
}
