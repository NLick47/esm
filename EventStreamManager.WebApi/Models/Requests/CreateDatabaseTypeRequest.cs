using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 创建数据库类型请求
/// </summary>
public class CreateDatabaseTypeRequest
{
    [Required(ErrorMessage = "类型标识不能为空")]
    [StringLength(50, ErrorMessage = "类型标识长度不能超过50个字符")]
    public string Value { get; set; } = string.Empty;

    [Required(ErrorMessage = "显示名称不能为空")]
    [StringLength(100, ErrorMessage = "显示名称长度不能超过100个字符")]
    public string Label { get; set; } = string.Empty;
}
