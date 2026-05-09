using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 导出事件处理记录请求
/// </summary>
public class ExportEventHandlesRequest
{
    /// <summary>
    /// 数据库类型（必填）
    /// </summary>
    [Required(ErrorMessage = "数据库类型不能为空")]
    public string DatabaseType { get; set; } = string.Empty;

    /// <summary>
    /// 事件ID
    /// </summary>
    public int? EventId { get; set; }

    /// <summary>
    /// 事件引用ID
    /// </summary>
    public string? StrEventReferenceId { get; set; }

    /// <summary>
    /// 处理器ID
    /// </summary>
    public string? ProcessorId { get; set; }

    /// <summary>
    /// 处理状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 事件代码
    /// </summary>
    public string? EventCode { get; set; }

    /// <summary>
    /// 开始日期
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// 结束日期
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// 最大导出行数（默认10000）
    /// </summary>
    [Range(1, 100000, ErrorMessage = "最大导出行数必须在1-100000之间")]
    public int MaxRows { get; set; } = 10000;
}
