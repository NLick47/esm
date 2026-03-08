namespace EventStreamManager.EventProcessor.Entities;

public class ProcessorStatistics
{
    /// <summary>
    /// 总数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 成功数
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// 失败数
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// 待处理数
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 处理中数
    /// </summary>
    public int ProcessingCount { get; set; }
}