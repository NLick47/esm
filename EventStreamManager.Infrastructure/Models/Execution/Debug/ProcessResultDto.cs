namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class ProcessResultDto
{
    /// <summary>
    /// 是否需要发送到API
    /// </summary>
    public bool NeedToSend { get; set; }
    
    /// <summary>
    /// 原因
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// 异常信息
    /// </summary>
    public object? Error { get; set; }
    
    /// <summary>
    /// 请求信息
    /// </summary>
    public object? RequestInfo { get; set; }
}