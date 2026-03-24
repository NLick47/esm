namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class RequestInfo
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
}