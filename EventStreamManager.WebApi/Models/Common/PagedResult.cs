namespace EventStreamManager.WebApi.Models.Common;

/// <summary>
/// 分页结果
/// </summary>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 总页数
    /// </summary>
    public int TotalPages { get; set; }
}
