using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 接口调试请求
/// </summary>
public class DebugInterfaceRequest
{
    /// <summary>
    /// 接口配置ID
    /// </summary>
    [Required(ErrorMessage = "接口配置ID不能为空")]
    public string InterfaceConfigId { get; set; } = string.Empty;

    /// <summary>
    /// 处理器ID
    /// </summary>
    [Required(ErrorMessage = "处理器ID不能为空")]
    public string ProcessorId { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    [Required(ErrorMessage = "数据库类型不能为空")]
    public string DatabaseType { get; set; } = string.Empty;

    /// <summary>
    /// 事件码
    /// </summary>
    public string? EventCode { get; set; } = string.Empty;

    /// <summary>
    /// 事件ID
    /// </summary>
    public string? EventId { get; set; }
}
