using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Scanners;

public class EventScanner : IEventScanner
{
    private readonly IEventRepository _repository;
    private readonly ILogger<EventScanner> _logger;

    public EventScanner(IEventRepository repository, ILogger<EventScanner> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<List<Event>> ScanAsync(string databaseType, EventConfig config, List<string>? eventCodes, List<string> processorIds)
    {
        try
        {
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

            var events = await _repository.ScanUnprocessedAsync(
                databaseType,
                config.TableName,
                config.BatchSize,
                startId,
                startTime,
                eventCodes,
                processorIds);

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
