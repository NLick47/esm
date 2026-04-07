using System.Diagnostics;
using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Services.Data;

public class DatabaseConnectionService : IDatabaseConnectionService
{
    private readonly ILogger<DatabaseConnectionService> _logger;

    public DatabaseConnectionService(ILogger<DatabaseConnectionService> logger)
    {
        _logger = logger;
    }

    public async Task<ConnectionTestResponse> TestConnectionAsync(ConnectionTestRequest request)
    {
        var response = new ConnectionTestResponse();
        
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("开始测试数据库连接 - 类型: {DatabaseType}", request.Driver);

            // 验证连接字符串
            if (string.IsNullOrWhiteSpace(request.ConnectionString))
            {
                response.Success = false;
                response.Message = "连接字符串不能为空";
                return response;
            }

            // 创建SqlSugar连接配置
            var connectionConfig = new ConnectionConfig
            {
                ConnectionString = request.ConnectionString,
                DbType = request.Driver.ToSqlSugarDbType(),
                IsAutoCloseConnection = true,
                InitKeyType = InitKeyType.Attribute,
                MoreSettings = new ConnMoreSettings
                {
                    IsAutoRemoveDataCache = true,
                    IsWithNoLockQuery = true,
                }
            };
            
            using var sqlSugarClient = new SqlSugarClient(connectionConfig);

            sqlSugarClient.Aop.OnLogExecuting = (sql, _) =>
            {
                _logger.LogDebug("执行测试SQL: {sql}", sql);
            };
            
            var testQuery = request.Driver.GetTestQuery();
            
            await sqlSugarClient.Ado.GetIntAsync(testQuery);
            
            string? version = null;
            try
            {
                version = request.Driver switch
                {
                    DriverType.SqlServer => await GetSqlServerVersion(sqlSugarClient),
                    DriverType.MySql => await GetMySqlVersion(sqlSugarClient),
                    DriverType.PostgreSql => await GetPostgreSqlVersion(sqlSugarClient),
                    DriverType.Oracle => await GetOracleVersion(sqlSugarClient),
                    DriverType.SqLite => await GetSqLiteVersion(sqlSugarClient),
                    _ => null
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "获取数据库版本信息失败");
            }

            response.Success = true;
            response.Message = "连接成功";
            response.DatabaseVersion = version;
            
            _logger.LogInformation("数据库连接测试成功 - 类型: {DatabaseType}, 耗时: {ElapsedMs}ms", 
                request.Driver, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库连接测试失败: {DatabaseType}", request.Driver);
            response.Success = false;
            response.Message = $"连接失败: {ex.Message}";
        }
        finally
        {
            stopwatch.Stop();
            response.ResponseTime = stopwatch.ElapsedMilliseconds;
        }

        return response;
    }

    public async Task<List<DriverType>> GetSupportedDatabaseTypesAsync()
    {
        return await Task.FromResult(Enum.GetValues<DriverType>().ToList());
    }

    public async Task<Dictionary<DriverType, string>> GetConnectionStringTemplatesAsync()
    {
        var templates = new Dictionary<DriverType, string>
        {
            [DriverType.SqlServer] = "Server=localhost;Database=YourDB;User Id=sa;Password=yourpassword;TrustServerCertificate=True;",
            [DriverType.MySql] = "Server=localhost;Database=YourDB;User=root;Password=yourpassword;",
            [DriverType.PostgreSql] = "Host=localhost;Database=YourDB;Username=postgres;Password=yourpassword;",
            [DriverType.Oracle] = "Data Source=localhost:1521/ORCL;User Id=system;Password=yourpassword;",
            [DriverType.SqLite] = "Data Source=app.db"
        };

        return await Task.FromResult(templates);
    }

    #region 获取数据库版本信息

    private async Task<string?> GetSqlServerVersion(ISqlSugarClient client)
    {
        var result = await client.Ado.GetStringAsync("SELECT @@VERSION");
        return result?.Split('\n').FirstOrDefault()?.Trim();
    }

    private static async Task<string?> GetMySqlVersion(ISqlSugarClient client)
    {
        var result = await client.Ado.GetStringAsync("SELECT VERSION()");
        return result;
    }

    private static async Task<string?> GetPostgreSqlVersion(ISqlSugarClient client)
    {
        var result = await client.Ado.GetStringAsync("SELECT VERSION()");
        return result?.Split(',').FirstOrDefault()?.Trim();
    }

    private async Task<string?> GetOracleVersion(ISqlSugarClient client)
    {
        var result = await client.Ado.GetStringAsync("SELECT * FROM V$VERSION WHERE ROWNUM = 1");
        return result;
    }

    private static async Task<string?> GetSqLiteVersion(ISqlSugarClient client)
    {
        var result = await client.Ado.GetStringAsync("SELECT sqlite_version()");
        return $"SQLite {result}";
    }

    #endregion

   
}