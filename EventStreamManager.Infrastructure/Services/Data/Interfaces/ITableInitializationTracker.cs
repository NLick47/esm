namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

/// <summary>
/// 表初始化状态跟踪器（进程级单例，防止并发重复初始化）
/// </summary>
public interface ITableInitializationTracker
{
    /// <summary>
    /// 尝试标记指定数据库类型为正在初始化。返回 true 表示当前线程获得了初始化权。
    /// </summary>
    bool TryMarkInitializing(string databaseType);

    /// <summary>
    /// 标记初始化失败，允许后续重试。
    /// </summary>
    void MarkFailed(string databaseType);
}
