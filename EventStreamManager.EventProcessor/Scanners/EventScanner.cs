using System.Text;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Scanners;

/// <summary>
/// 事件扫描器 - 从数据库扫描未被处理的新事件
/// </summary>
public class EventScanner
{
    private readonly ISqlSugarContext _db;
    private readonly IEventListenerConfigService _configService;
    private readonly ILogger<EventScanner> _logger;

    public EventScanner(
        ISqlSugarContext db,
        IEventListenerConfigService configService,
        ILogger<EventScanner> logger)
    {
        _db = db;
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// 扫描未被处理的新事件
    /// </summary>
    /// <param name="databaseType">数据库类型</param>
    /// <param name="config">事件监听配置</param>
    /// <param name="eventCodes">处理器关注的事件码列表（空表示不筛选）</param>
    public async Task<List<Event>> ScanAsync(string databaseType, EventConfig config, List<string>? eventCodes)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var sql = BuildScanSql(config, eventCodes);

            _logger.LogDebug("[{DatabaseType}] 执行扫描SQL: {Sql}", databaseType, sql);
            var events = await client.Ado.SqlQueryAsync<Event>(sql);

            _logger.LogInformation("[{DatabaseType}] 扫描完成: 发现 {Count} 条未处理事件",
                databaseType, events.Count);

            return events;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 扫描失败", databaseType);
            return new List<Event>();
        }
    }

    /// <summary>
    /// 更新扫描位置
    /// </summary>
    public async Task UpdatePositionAsync(string databaseType, int lastEventId)
    {
        try
        {
            var configs = await _configService.GetAllConfigsAsync();
            if (configs.Databases.TryGetValue(databaseType, out var config))
            {
                config.TotalEventsProcessed = lastEventId;
                await _configService.UpdateConfigAsync(databaseType, config);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 更新扫描位置失败", databaseType);
        }
    }

    /// <summary>
    /// 构建扫描SQL
    /// </summary>
    private string BuildScanSql(EventConfig config, List<string>? eventCodes)
    {
        var sb = new StringBuilder();
        
        // SELECT: 只查询未被处理的事件
        sb.AppendFormat(
            "SELECT TOP({0}) e.* FROM {1} e ",
            config.BatchSize,
            config.TableName);

        // LEFT JOIN: 关联处理记录表
        sb.Append("LEFT JOIN tblEventHandle h ON e.Id = h.EventId ");

       
        sb.Append("WHERE 1=1 ");
        
        if (config.StartCondition != null)
        {
            if (config.StartCondition.Type == "id" && 
                int.TryParse(config.StartCondition.IdValue, out var startId))
            {
                sb.AppendFormat("AND e.{0} >= {1} ", config.PrimaryKey, startId);
            }
            else if (config.StartCondition.Type == "time" && 
                     DateTime.TryParse(config.StartCondition.TimeValue, out var startTime))
            {
                sb.AppendFormat("AND e.{0} >= '{1:yyyy-MM-dd HH:mm:ss}' ",
                    config.TimestampField, startTime);
            }
        }

        // 事件码筛选
        if (eventCodes is { Count: > 0 })
        {
            var codes = string.Join("','", eventCodes.Select(c => c.Replace("'", "''")));
            sb.AppendFormat("AND e.EventCode IN ('{0}') ", codes);
        }
        
        sb.Append("AND h.Id IS NULL ");
        sb.AppendFormat("ORDER BY e.{0} ASC", config.PrimaryKey);
        return sb.ToString();
    }
}
