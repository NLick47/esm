using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace EventStreamManager.EventProcessor.Scanners;

/// <summary>
/// 事件扫描器
/// </summary>
public class EventScanner
{
    private readonly ISqlSugarContext _db;
    private readonly ILogger<EventScanner> _logger;

    public EventScanner(
        ISqlSugarContext db,
        ILogger<EventScanner> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 扫描未被处理的新事件
    /// </summary>
    public async Task<List<Event>> ScanAsync(string databaseType, EventConfig config, List<string>? eventCodes)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            
            // 解析起始 ID
            int? startId = null;
            DateTime? startTime = null;
            
            if (config.StartCondition != null)
            {
                if (config.StartCondition.Type == "id" && 
                    int.TryParse(config.StartCondition.IdValue, out var id))
                {
                    startId = id;
                }
                else if (config.StartCondition.Type == "time" && 
                         DateTime.TryParse(config.StartCondition.TimeValue, out var time))
                {
                    startTime = time;
                }
            }

            // 使用 SqlSugar 查询，自动适配多数据库
            // NOT EXISTS: 筛选没有处理记录的新事件
            var query = client.Queryable<Event>()
                .AS(config.TableName)
                .Where(e => !SqlFunc.Subqueryable<EventHandle>()
                    .Where(h => h.EventId == e.Id)
                    .Any())
                // 起始条件 - ID
                .WhereIF(startId.HasValue, e => e.Id >= startId!.Value)
                // 起始条件 - 时间
                .WhereIF(startTime.HasValue, e => e.CreateDatetime >= startTime!.Value)
                // 事件码筛选
                .WhereIF(eventCodes != null && eventCodes.Count > 0, 
                    e => eventCodes!.Contains(e.EventCode))
                // 排序
                .OrderBy(e => e.Id, OrderByType.Asc)
                // 分页
                .Take(config.BatchSize);

            // 执行查询
            var events = await query.ToListAsync();

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
}
