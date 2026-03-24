using System.ComponentModel.DataAnnotations;
using EventStreamManager.Infrastructure.Services.Validators.Attributes;

namespace EventStreamManager.WebApi.Models.Requests;

public class CustomSqlTemplateRequest
{
    [SqlTemplateName]
    public string Name { get; set; } = string.Empty;
    
    [SafeSql(true,"SQL模板验证失败")]
    public string SqlTemplate { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "描述不能超过500个字符")]
    public string Description { get; set; } = string.Empty;
}