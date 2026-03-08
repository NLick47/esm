using SqlSugar;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface ISqlSugarContext
{
    
    /// <summary>
    /// 获取指定数据库类型的 SqlSugar 客户端（每次调用都创建新连接）
    /// </summary>
    Task<ISqlSugarClient> GetClientAsync(string databaseType);
    
    /// <summary>
    /// 执行查询并返回字典列表
    /// </summary>
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(string databaseType, string query, object? parameters = null);
   
    
    /// <summary>
    /// 执行查询并返回指定类型的对象列表
    /// </summary>
    Task<List<T>> ExecuteQueryAsync<T>(string databaseType, string query, object? parameters = null) where T : class, new();
    
    /// <summary>
    /// 执行非查询命令
    /// </summary>
    Task<int> ExecuteCommandAsync(string databaseType, string sql, object? parameters = null);
    
    
    /// <summary>
    /// 执行查询并返回单个值
    /// </summary>
    Task<T> ExecuteScalarAsync<T>(string databaseType, string sql, object? parameters = null);
}