using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;
using System.Text;

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
            
            return CreateSqlSugarClient(activeConfig.Driver,activeConfig.ConnectionString,activeConfig.Timeout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建数据库客户端失败 - 数据库类型: {DatabaseType}", databaseType);
            throw;
        }
    }
    
    
    public Task<ISqlSugarClient> GetClientAsync(DatabaseConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
    
        return Task.FromResult(CreateSqlSugarClient(config.Driver, config.ConnectionString, config.Timeout));
    }
    
    
    private ISqlSugarClient CreateSqlSugarClient(DriverType driver, string connectionString, int timeout)
    {
        var client = new SqlSugarClient(new ConnectionConfig
        {
            ConnectionString = connectionString,
            DbType = GetDbType(driver),
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings
            {
                IsAutoRemoveDataCache = true,
                IsWithNoLockQuery = true
            }
        });
    
       
        if (timeout > 0)
        {
            client.Ado.CommandTimeOut = timeout;
        }
    
        SetupAopLogging(client);
    
        return client;
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
            
            // 在 DEBUG 模式下打印 SQL
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                PrintSqlWithParameters("ExecuteQueryAsync", query, parameters);
            }
            
            var dataTable = await client.Ado.GetDataTableAsync(query, parameters);
            
            var result = new List<Dictionary<string, object>>();
            foreach (System.Data.DataRow row in dataTable.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (System.Data.DataColumn col in dataTable.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                result.Add(dict);
            }
            
            _logger.LogDebug("查询返回 {RowCount} 行数据", result.Count);
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
            
            // 在 DEBUG 模式下打印 SQL
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                PrintSqlWithParameters($"ExecuteQueryAsync<{typeof(T).Name}>", query, parameters);
            }
            
            var result = await client.Ado.SqlQueryAsync<T>(query, parameters);
            
            _logger.LogDebug("泛型查询返回 {RowCount} 行数据", result?.Count ?? 0);
            return result ?? new List<T>();
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
            
            // 在 DEBUG 模式下打印 SQL
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                PrintSqlWithParameters("ExecuteCommandAsync", sql, parameters);
            }
            
            var result = await client.Ado.ExecuteCommandAsync(sql, parameters);
            
            _logger.LogDebug("命令执行影响行数: {AffectedRows}", result);
            return result;
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
            
            // 在 DEBUG 模式下打印 SQL
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                PrintSqlWithParameters($"ExecuteScalarAsync<{typeof(T).Name}>", sql, parameters);
            }
            
            var result = await client.Ado.SqlQuerySingleAsync<T>(sql, parameters);
            
            _logger.LogDebug("标量查询返回: {Result}", result);
            return result;
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
            DriverType.SqlServer => DbType.SqlServer,
            DriverType.MySql => DbType.MySql,
            DriverType.PostgreSql => DbType.PostgreSQL,
            DriverType.Oracle => DbType.Oracle,
            DriverType.SqLite => DbType.Sqlite,
            _ => throw new NotSupportedException($"不支持的数据库驱动: {driver}")
        };
    }

    // 设置 AOP 日志
    private void SetupAopLogging(ISqlSugarClient client)
    {
        client.Aop.OnLogExecuting = (sql, parameters) =>
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var formattedSql = FormatSql(sql, parameters);
                _logger.LogDebug("[AOP] 执行SQL: {Sql}", formattedSql);
            }
        };
        
        client.Aop.OnLogExecuted = (_, _) =>
        {
            
        };
        
        client.Aop.OnError = (ex) =>
        {
            _logger.LogError(ex, "[AOP] SQL执行错误");
        };
    }

    // 格式化 SQL 和参数
    private string FormatSql(string sql, SugarParameter[]? parameters)
    {
        if (parameters == null || parameters.Length == 0)
            return sql;

        var formattedSql = sql;
        
        // 替换参数为实际值（仅用于日志显示）
        foreach (var param in parameters)
        {
            var paramValue = GetParameterValue(param);
            formattedSql = formattedSql.Replace(param.ParameterName, paramValue);
        }

        return formattedSql;
    }

    // 获取参数值字符串表示
    private string GetParameterValue(SugarParameter param)
    {
        if (param.Value == null || param.Value == DBNull.Value)
            return "NULL";

        return param.Value switch
        {
            string str => $"'{str}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
            bool b => b ? "1" : "0",
            byte[] bytes => $"0x{BitConverter.ToString(bytes).Replace("-", "")}",
            _ => param.Value.ToString() ?? "NULL"
        };
    }

    // 打印 SQL 和参数（用于手动执行的方法）
    private void PrintSqlWithParameters(string methodName, string sql, object? parameters)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine($"【{methodName}】执行的 SQL:");
            sb.AppendLine(sql);

            if (parameters != null)
            {
                sb.AppendLine("参数:");
                
                // 处理不同类型的参数对象
                if (parameters is IEnumerable<KeyValuePair<string, object>> dictParams)
                {
                    foreach (var param in dictParams)
                    {
                        sb.AppendLine($"  {param.Key} = {FormatParameterValue(param.Value)}");
                    }
                }
                else if (parameters is IEnumerable<SugarParameter> sugarParams)
                {
                    foreach (var param in sugarParams)
                    {
                        sb.AppendLine($"  {param.ParameterName} = {GetParameterValue(param)}");
                    }
                }
                else
                {
                    // 如果是匿名对象，使用反射获取属性
                    var properties = parameters.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        var value = prop.GetValue(parameters);
                        sb.AppendLine($"  {prop.Name} = {FormatParameterValue(value)}");
                    }
                }
            }
            else
            {
                sb.AppendLine("参数: 无");
            }

            _logger.LogDebug(sb.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "打印 SQL 参数时出错");
            // 出错时至少打印原始 SQL
            _logger.LogDebug("SQL: {Sql}", sql);
        }
    }

    // 格式化参数值
    private string FormatParameterValue(object? value)
    {
        if (value == null || value == DBNull.Value)
            return "NULL";

        return value switch
        {
            string str => $"'{str}'",
            DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss.fff}'",
            bool b => b ? "true" : "false",
            byte[] bytes => $"byte[{bytes.Length}]",
            _ => value.ToString() ?? "NULL"
        };
    }
}