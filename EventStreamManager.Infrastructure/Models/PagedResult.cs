namespace EventStreamManager.Infrastructure.Models;

public class PagedResult<T>
{
    public List<T>? List { get; set; }
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}