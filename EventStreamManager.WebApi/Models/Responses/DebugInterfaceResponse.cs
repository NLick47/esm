namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 接口调试响应
/// </summary>
public class DebugInterfaceResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<DebugLogEntryResponse> Logs { get; set; } = new();
    public long ExecutionTimeMs { get; set; }
    public long? ProcessorExecutionTime { get; set; }
    public long? InterfaceExecutionTime { get; set; }
    public ProcessResultResponse? ProcessorResult { get; set; }
    public RequestInfoResponse? RequestInfo { get; set; }
    public ResponseInfoResponse? ResponseInfo { get; set; }
}
