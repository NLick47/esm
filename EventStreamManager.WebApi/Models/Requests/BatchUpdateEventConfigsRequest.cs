namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 批量更新事件监听配置请求
/// </summary>
public class BatchUpdateEventConfigsRequest
{
    public Dictionary<string, UpdateEventConfigRequest> Updates { get; set; } = new();
}
