namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 处理器执行结果响应
/// </summary>
public class ProcessResultResponse
{
    public bool NeedToSend { get; set; }
    public string? Reason { get; set; }
    public object? Error { get; set; }
    public object? RequestInfo { get; set; }
}
