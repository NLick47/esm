using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// HTTP请求头项请求
/// </summary>
public class HeaderItemRequest
{
    [Required(ErrorMessage = "请求头键名不能为空")]
    [StringLength(200, ErrorMessage = "请求头键名长度不能超过200个字符")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "请求头值不能为空")]
    [StringLength(2000, ErrorMessage = "请求头值长度不能超过2000个字符")]
    public string Value { get; set; } = string.Empty;
}
