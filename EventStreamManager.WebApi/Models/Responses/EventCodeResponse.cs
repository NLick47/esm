namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 事件代码响应
/// </summary>
public class EventCodeResponse
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
}
