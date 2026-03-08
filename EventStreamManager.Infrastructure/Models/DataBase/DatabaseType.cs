using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.Infrastructure.Models.DataBase;

public class DatabaseType
{
   
    [Required(ErrorMessage = "类型标识不能为空")]
    public string Value { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "显示名称不能为空")]
    public string Label { get; set; } = string.Empty;
}