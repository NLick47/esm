using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.Infrastructure.Models.Interface;

public class InterfaceConfig
{
    public string Id { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "配置名称不能为空")]
    public string Name { get; set; } = string.Empty;
   
    [Required(ErrorMessage = "请至少选择一个关联的处理器")]
    [MinLength(1, ErrorMessage = "请至少选择一个关联的处理器")]
    public List<string> ProcessorIds { get; set; } = new();
    public List<string> ProcessorNames { get; set; } = new();
    
    
    [Required(ErrorMessage = "接口URL不能为空")]
    [Url(ErrorMessage = "请输入有效的URL地址")]
    [StringLength(500, ErrorMessage = "URL长度不能超过500个字符")]
    public string Url { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "请求方法不能为空")]
    [RegularExpression("^(GET|POST|PUT|DELETE|PATCH|HEAD|OPTIONS)$", 
        ErrorMessage = "请求方法必须是 GET、POST、PUT、DELETE、PATCH、HEAD 或 OPTIONS")]
    public string Method { get; set; } = "POST";
    public List<HeaderItem> Headers { get; set; } = new();
    
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int Timeout { get; set; } = 30;
    
    
    [Range(0, 10, ErrorMessage = "重试次数必须在0-10之间")]
    public int RetryCount { get; set; } = 3;
  
    [Range(1, 60, ErrorMessage = "重试间隔必须在1-60秒之间")]
    public int RetryInterval { get; set; } = 5;
    public bool Enabled { get; set; }
    
    [Required(ErrorMessage = "请求模板不能为空")]
    public string RequestTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}