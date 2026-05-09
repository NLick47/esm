namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// HTTP请求信息响应
/// </summary>
public class RequestInfoResponse
{
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; } = string.Empty;
}
