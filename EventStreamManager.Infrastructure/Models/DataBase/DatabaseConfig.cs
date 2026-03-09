using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EventStreamManager.Infrastructure.Models.DataBase;

public class DatabaseConfig
{
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "配置名称不能为空")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "连接字符串不能为空")]
    public string ConnectionString { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DriverType Driver { get; set; }
    
    public bool IsActive { get; set; } = false;
    
    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int Timeout { get; set; } = 30;
}