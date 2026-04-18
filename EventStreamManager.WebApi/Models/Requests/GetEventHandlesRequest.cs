namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 获取事件处理记录列表请求
/// </summary>
public class GetEventHandlesRequest
{
    /// <summary>
    /// 数据库类型（必填）
    /// </summary>
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
    /// 页码（默认1）
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// 每页大小（默认20）
    /// </summary>
    public int PageSize { get; set; } = 20;
}
