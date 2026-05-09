using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 执行调试请求
/// </summary>
public class ExecuteDebugRequest
{
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
    [Required(ErrorMessage = "事件码不能为空")]
    public string EventCode { get; set; } = string.Empty;

    /// <summary>
    /// 事件ID（可选）
    /// </summary>
    public string? EventId { get; set; }
}
