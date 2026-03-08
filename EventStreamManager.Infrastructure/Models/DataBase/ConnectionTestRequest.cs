using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.Infrastructure.Models.DataBase;

public class ConnectionTestRequest
{
    [Required(ErrorMessage = "连接字符串不能为空")]
    public string ConnectionString { get; set; } = string.Empty;
    public DriverType Driver { get; set; }
}