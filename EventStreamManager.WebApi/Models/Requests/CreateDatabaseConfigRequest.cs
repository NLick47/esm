using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using EventStreamManager.Infrastructure.Models.DataBase;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 创建数据库配置请求
/// </summary>
public class CreateDatabaseConfigRequest
{
    [Required(ErrorMessage = "配置名称不能为空")]
    [StringLength(100, ErrorMessage = "配置名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "连接字符串不能为空")]
    [StringLength(1000, ErrorMessage = "连接字符串长度不能超过1000个字符")]
    public string ConnectionString { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DriverType Driver { get; set; }

    [Range(1, 300, ErrorMessage = "超时时间必须在1-300秒之间")]
    public int Timeout { get; set; } = 30;
}
