namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class ResponseInfo
{
    public int StatusCode { get; set; }
    public string? StatusMessage { get; set; } = string.Empty;
    public string? Body { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}