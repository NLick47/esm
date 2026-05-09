using System.ComponentModel.DataAnnotations;
using EventStreamManager.Infrastructure.Services.Validators.Attributes;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 更新自定义SQL模板请求
/// </summary>
public class UpdateCustomSqlTemplateRequest
{
    [SqlTemplateName]
    public string Name { get; set; } = string.Empty;

    [SafeSql(true, "SQL模板验证失败")]
    public string SqlTemplate { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
    public string Description { get; set; } = string.Empty;
}
