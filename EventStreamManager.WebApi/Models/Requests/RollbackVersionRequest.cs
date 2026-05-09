namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 回滚处理器版本请求
/// </summary>
public class RollbackVersionRequest
{
    public bool RestoreCode { get; set; } = true;
    public bool RestoreSqlTemplate { get; set; } = true;
    public bool RestoreEventCodes { get; set; } = true;
    public bool RestoreDatabaseTypes { get; set; } = true;
    public bool RestoreMetadata { get; set; } = true;
}
