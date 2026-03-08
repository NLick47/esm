using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class JSProcessor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    
    [MinLength(1, ErrorMessage = "至少选择一个数据库类型")]
    public List<string> DatabaseTypes { get; set; } = new();
    
    [MinLength(1, ErrorMessage = "至少需要提供一个事件码")]
    public List<string> EventCodes { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
    public string Code { get; set; }
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
}