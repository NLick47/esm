using System.ComponentModel.DataAnnotations;
using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.WebApi.Models.Requests;

public class ProcessorRequest
{
    [Required(ErrorMessage = "处理器名称不能为空")]
    [StringLength(20, MinimumLength = 1, ErrorMessage = "处理器名称长度必须在1-20之间")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "数据库类型不能为空")]
    public List<string> DatabaseTypes { get; set; } = new();
    
    [Required(ErrorMessage = "事件代码不能为空")]
    public List<string> EventCodes { get; set; } = new();
    
    [Required(ErrorMessage = "SQL模板类型不能为空")]
    public SqlTemplateType SqlTemplateType { get; set; }
    
    [Required(ErrorMessage = "SQL模板ID不能为空")]
    public string SqlTemplateId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "处理器代码不能为空")]
    public string Code { get; set; } = string.Empty;
    
    public bool Enabled { get; set; } = true;
    
    [StringLength(500, ErrorMessage = "描述长度不能超过500")]
    public string Description { get; set; } = string.Empty;
}