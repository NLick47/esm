using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Services.Data;

public class SqlSugarContext : ISqlSugarContext
{
    private readonly IDatabaseSchemeService _databaseSchemeService;
    private readonly ILogger<SqlSugarContext> _logger;

    public SqlSugarContext(
        IDatabaseSchemeService databaseSchemeService,
        ILogger<SqlSugarContext> logger)
    {
        _databaseSchemeService = databaseSchemeService;
        _logger = logger;
    }

    public async Task<ISqlSugarClient> GetClientAsync(string databaseType)
    {
        if (string.IsNullOrWhiteSpace(databaseType))
            throw new ArgumentException("数据库类型不能为空", nameof(databaseType));

        try
        {
            // 每次调用都从配置服务获取最新的激活配置
            var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(databaseType);
            
            if (activeConfig == null)
            {
                throw new InvalidOperationException($"未找到数据库类型 '{databaseType}' 的激活配置");
            }

            _logger.LogDebug("创建数据库连接 - 类型: {DatabaseType}, 配置: {ConfigName}, 驱动: {Driver}", 
                databaseType, activeConfig.Name, activeConfig.Driver);

            var dbType = GetDbType(activeConfig.Driver);
            
            // 创建新的客户端连接
            var client = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = activeConfig.ConnectionString,
                DbType = dbType,
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true,
                    IsWithNoLockQuery = true
                }
            });

            // 可以添加一些全局的 AOP 事件
            client.Aop.OnLogExecuting = (sql, parameters) =>
            {
                _logger.LogDebug("执行SQL: {Sql}", sql);
            };

            client.Aop.OnError = (ex) =>
            {
                _logger.LogError(ex, "SQL执行错误");
            };

            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建数据库客户端失败 - 数据库类型: {DatabaseType}", databaseType);
            throw;
        }
    }

    public async Task<List<Dictionary<string, object>>> ExecuteQueryAsync(
        string databaseType, 
        string query, 
        object? parameters = null)
    {
        ISqlSugarClient? client = null;
        try
        {
            client = await GetClientAsync(databaseType);
            
            var dataTable = await client.Ado.GetDataTableAsync(query, parameters);
            
            var result = new List<Dictionary<string, object>>();
            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col] ?? DBNull.Value;
                }
                result.Add(dict);
            }
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行查询失败 - 数据库类型: {DatabaseType}, 查询: {Query}", 
                databaseType, query);
            throw;
        }
        finally
        {
            // 确保连接被释放
            if (client != null)
            {
                 client.Dispose();
            }
        }
    }

    public async Task<List<T>> ExecuteQueryAsync<T>(
        string databaseType, 
        string query, 
        object? parameters = null) where T : class, new()
    {
        ISqlSugarClient? client = null;
        try
        {
            client = await GetClientAsync(databaseType);
            return await client.Ado.SqlQueryAsync<T>(query, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行泛型查询失败 - 数据库类型: {DatabaseType}, 类型: {Type}, 查询: {Query}", 
                databaseType, typeof(T).Name, query);
            throw;
        }
        finally
        {
            if (client != null)
            {
                 client.Dispose();
            }
        }
    }

    public async Task<int> ExecuteCommandAsync(
        string databaseType, 
        string sql, 
        object? parameters = null)
    {
        ISqlSugarClient? client = null;
        try
        {
            client = await GetClientAsync(databaseType);
            return await client.Ado.ExecuteCommandAsync(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行命令失败 - 数据库类型: {DatabaseType}, SQL: {Sql}", 
                databaseType, sql);
            throw;
        }
        finally
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }

    public async Task<T> ExecuteScalarAsync<T>(
        string databaseType, 
        string sql, 
        object? parameters = null)
    {
        ISqlSugarClient? client = null;
        try
        {
            client = await GetClientAsync(databaseType);
            return await client.Ado.SqlQuerySingleAsync<T>(sql, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行标量查询失败 - 数据库类型: {DatabaseType}, SQL: {Sql}", 
                databaseType, sql);
            throw;
        }
        finally
        {
            if (client != null)
            {
                client.Dispose();
            }
        }
    }

    // 根据驱动名称获取对应的 DbType
    private DbType GetDbType(DriverType driver)
    {
        return driver switch
        {
            DriverType.SqlServer => SqlSugar.DbType.SqlServer,
            DriverType.MySql => SqlSugar.DbType.MySql,
            DriverType.PostgreSql => SqlSugar.DbType.PostgreSQL,
            DriverType.Oracle => SqlSugar.DbType.Oracle,
            DriverType.SQLite => SqlSugar.DbType.Sqlite,
            _ => throw new NotSupportedException($"不支持的数据库驱动: {driver}")
        };
    }
}