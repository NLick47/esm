namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 接口配置响应
/// </summary>
public class InterfaceConfigResponse
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> ProcessorIds { get; set; } = new();
    public List<string> ProcessorNames { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public List<HeaderItemResponse> Headers { get; set; } = new();
    public int Timeout { get; set; }
    public int RetryCount { get; set; }
    public int RetryInterval { get; set; }
    public bool Enabled { get; set; }
    public string RequestTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
