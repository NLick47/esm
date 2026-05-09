namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 所有事件监听配置响应
/// </summary>
public class EventListenerConfigsResponse
{
    public Dictionary<string, EventConfigResponse> Databases { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Version { get; set; } = string.Empty;
}
