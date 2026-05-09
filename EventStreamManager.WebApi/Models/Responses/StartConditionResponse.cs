namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 监听起始条件响应
/// </summary>
public class StartConditionResponse
{
    public string Type { get; set; } = string.Empty;
    public string TimeValue { get; set; } = string.Empty;
    public string IdValue { get; set; } = string.Empty;
}
