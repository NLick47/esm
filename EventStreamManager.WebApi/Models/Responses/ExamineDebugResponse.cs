namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// Examine调试响应
/// </summary>
public class ExamineDebugResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public long ExecutionTimeMs { get; set; }
    public object? RawData { get; set; }
    public ProcessResultResponse? Result { get; set; }
    public List<DebugLogEntryResponse> Logs { get; set; } = new();
    public CodeValidationResponse? CodeValidation { get; set; }
}
