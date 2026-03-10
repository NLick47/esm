namespace EventStreamManager.Infrastructure.Services;

public enum DebugType
{
    /// <summary>
    /// 普通调试（执行处理器，不发送）
    /// </summary>
    Normal,
        
    /// <summary>
    /// 接口调试（执行处理器并发送到接口）
    /// </summary>
    Interface
}