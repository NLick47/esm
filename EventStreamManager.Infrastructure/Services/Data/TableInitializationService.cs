using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Services.Data;

public class TableInitializationService : ITableInitializationService
{
    private readonly ILogger<TableInitializationService> _logger;
    private readonly ISqlSugarContext _sqlSugarContext;

    public TableInitializationService(
        ILogger<TableInitializationService> logger,
        ISqlSugarContext sqlSugarContext)
    {
        _logger = logger;
        _sqlSugarContext = sqlSugarContext;
    }

    public async Task<InitializeTablesResult> InitializeTablesAsync(DatabaseConfig config)
    {
        var result = new InitializeTablesResult
        {
            Success = false,
            CreatedTables = new List<string>(),
            TableResults = new Dictionary<string, string>()
        };

        ISqlSugarClient? sqlSugarClient = null;

        try
        {
            if (string.IsNullOrWhiteSpace(config.ConnectionString))
            {
                result.Message = "连接字符串不能为空";
                return result;
            }

            _logger.LogInformation("开始初始化表结构 - 驱动类型: {Driver}, 配置名称: {ConfigName}",
                config.Driver, config.Name);


            sqlSugarClient = await _sqlSugarContext.GetClientAsync(config);


            var entityTypes = new[]
            {
                typeof(Event),
                typeof(EventHandle),
                typeof(EventHandleLog)
            };

            var successMessages = new List<string>();
            var errorMessages = new List<string>();

            foreach (var entityType in entityTypes)
            {
                try
                {
                    var entityInfo = sqlSugarClient.EntityMaintenance.GetEntityInfo(entityType);
                    var tableName = entityInfo.DbTableName;

                    // 检查表是否存在
                    var isTableExists = sqlSugarClient.DbMaintenance.IsAnyTable(tableName);

                    // 使用 SqlSugar 的 CodeFirst 功能创建或更新表
                    sqlSugarClient.CodeFirst.InitTables(entityType);

                    if (!isTableExists)
                    {
                        result.CreatedTables.Add(tableName);
                        var message = $"表 {tableName} 创建成功";
                        successMessages.Add(message);
                        result.TableResults[tableName] = "Created";
                        _logger.LogInformation(message);
                    }
                    else
                    {
                        var message = $"表 {tableName} 已存在，结构已同步";
                        successMessages.Add(message);
                        result.TableResults[tableName] = "Updated";
                        _logger.LogInformation(message);
                    }
                }
                catch (Exception ex)
                {
                    var tableName = GetTableNameSafe(sqlSugarClient, entityType);
                    var errorMessage = $"表 {tableName} 初始化失败: {ex.Message}";
                    errorMessages.Add(errorMessage);
                    result.TableResults[tableName] = $"Failed: {ex.Message}";
                    _logger.LogError(ex, "表 {TableName} 初始化失败", tableName);
                }
            }

            // 构建结果消息
            result.Message = BuildResultMessage(successMessages, errorMessages);
            result.Success = !errorMessages.Any();

            _logger.LogInformation(
                result.Success ? "表结构初始化完成" : "表结构初始化部分失败",
                result.Success);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化表结构失败");
            result.Message = $"初始化表结构失败: {ex.Message}";
            return result;
        }
        finally
        {
            sqlSugarClient?.Dispose();
        }
    }


    private string GetTableNameSafe(ISqlSugarClient client, Type entityType)
    {
        try
        {
            // 尝试获取实体信息中的表名
            var entityInfo = client.EntityMaintenance.GetEntityInfo(entityType);
            return entityInfo.DbTableName;
        }
        catch (Exception ex)
        {
            // 记录警告日志，但不要抛出异常
            _logger.LogDebug(ex, "获取表名失败，使用实体类型名称: {EntityType}", entityType.Name);
            return entityType.Name;
        }
    }


    private string BuildResultMessage(List<string> successMessages, List<string> errorMessages)
    {
        var parts = new List<string>();

        // 添加成功消息
        if (successMessages.Any())
        {
            parts.Add(successMessages.Count == 1
                ? $"成功: {successMessages[0]}"
                : $"成功 ({successMessages.Count} 个表): {string.Join("; ", successMessages)}");
        }

        // 添加失败消息
        if (errorMessages.Any())
        {
            parts.Add(errorMessages.Count == 1
                ? $"失败: {errorMessages[0]}"
                : $"失败 ({errorMessages.Count} 个表): {string.Join("; ", errorMessages)}");
        }

        // 如果没有任何消息（理论上不会发生）
        if (!parts.Any())
        {
            return "没有表需要初始化";
        }

        return string.Join(" | ", parts);
    }


    
}