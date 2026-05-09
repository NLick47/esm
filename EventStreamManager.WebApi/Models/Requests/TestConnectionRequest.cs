using System.ComponentModel.DataAnnotations;
using EventStreamManager.Infrastructure.Models.DataBase;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 测试数据库连接请求
/// </summary>
public class TestConnectionRequest
{
    [Required(ErrorMessage = "连接字符串不能为空")]
    [StringLength(1000, ErrorMessage = "连接字符串长度不能超过1000个字符")]
    public string ConnectionString { get; set; } = string.Empty;

    public DriverType Driver { get; set; }
}
