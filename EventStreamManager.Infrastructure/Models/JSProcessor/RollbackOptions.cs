namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class RollbackOptions
{
    public bool RestoreCode { get; set; } = true;
    public bool RestoreSqlTemplate { get; set; } = true;
    public bool RestoreEventCodes { get; set; } = true;
    public bool RestoreDatabaseTypes { get; set; } = true;
    public bool RestoreMetadata { get; set; } = true;
}
