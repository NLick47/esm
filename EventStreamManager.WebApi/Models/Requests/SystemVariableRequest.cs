using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 保存系统变量请求
/// </summary>
public class SystemVariableRequest
{
    [Required(ErrorMessage = "变量键名不能为空")]
    [StringLength(100, ErrorMessage = "键名长度不能超过100个字符")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "变量值不能为空")]
    public string Value { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述长度不能超过500个字符")]
    public string? Description { get; set; }

    [StringLength(50, ErrorMessage = "分类长度不能超过50个字符")]
    public string? Category { get; set; }
}
