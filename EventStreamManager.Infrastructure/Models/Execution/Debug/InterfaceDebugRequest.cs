namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class InterfaceDebugRequest
{
    /// <summary>
    /// 接口配置ID
    /// </summary>
    public string InterfaceConfigId { get; set; } = string.Empty;

    
    /// <summary>
    /// 处理器ID
    /// </summary>
    public string ProcessorId { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DatabaseType { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 事件码
    /// </summary>
    public string? EventCode { get; set; } = string.Empty;
    
    
    
    /// <summary>
    /// 事件ID
    /// </summary>
    public string? EventId { get; set; }
}