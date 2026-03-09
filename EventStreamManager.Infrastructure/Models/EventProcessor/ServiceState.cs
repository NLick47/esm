namespace EventStreamManager.Infrastructure.Models.EventProcessor;

public class ServiceState
{
    /// <summary>
    /// 服务是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
    
    /// <summary>
    /// 服务启动时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// 版本信息
    /// </summary>
    public string Version { get; set; } = "1.0";
}