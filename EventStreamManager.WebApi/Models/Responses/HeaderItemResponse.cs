namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// HTTP请求头项响应
/// </summary>
public class HeaderItemResponse
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
