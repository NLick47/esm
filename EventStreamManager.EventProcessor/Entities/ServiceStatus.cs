namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 服务状态概览
/// </summary>
public class ServiceStatus
{
    /// <summary>
    /// 服务是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 服务是否正在运行（有处理器在运行）
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// 服务启动时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 运行时长（扣除暂停时间）
    /// </summary>
    public TimeSpan RunningDuration { get; set; }

    /// <summary>
    /// 处理器总数
    /// </summary>
    public int ProcessorCount { get; set; }

    /// <summary>
    /// 活动处理器数量
    /// </summary>
    public int ActiveProcessorCount { get; set; }

    /// <summary>
    /// 各处理器状态
    /// </summary>
    public List<ProcessorStatus> Processors { get; set; } = new();
}