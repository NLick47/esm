namespace EventStreamManager.Infrastructure.Entities;

/// <summary>
/// 处理状态常量
/// </summary>
public class HandleStatus
{
    public const string Success = "Success";
    public const string Fail = "Fail";
    public const string Exception = "Exception";
    public const string Unhandled = "Unhandled";
}

/// <summary>
/// 发送结果
/// </summary>
public class SendResult
{
    public bool Success { get; set; }
    public int StatusCode { get; set; }
    public string? ResponseContent { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
}

/// <summary>
/// 执行结果
/// </summary>
public class ExecutionResult
{
    public string ProcessorId { get; set; } = string.Empty;
    public string ProcessorName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public bool NeedToSend { get; set; }
    public string? RequestInfo { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Reason { get; set; }
    public string? ConsoleOutput { get; set; }
    public long ExecutionTimeMs { get; set; }
    public SendResult? SendResult { get; set; }
}