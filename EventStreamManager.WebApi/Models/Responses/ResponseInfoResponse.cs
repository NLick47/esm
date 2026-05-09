namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// HTTP响应信息响应
/// </summary>
public class ResponseInfoResponse
{
    public int StatusCode { get; set; }
    public string? StatusMessage { get; set; } = string.Empty;
    public string? Body { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
}
