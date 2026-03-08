using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Recorders;

/// <summary>
/// 处理记录器
/// </summary>
public class HandleRecorder
{
    private readonly ISqlSugarContext _db;
    private readonly ILogger<HandleRecorder> _logger;

    public HandleRecorder(ISqlSugarContext db, ILogger<HandleRecorder> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 获取或创建单个处理器的处理记录
    /// </summary>
    public async Task<EventHandle> GetOrCreateAsync(string databaseType, int eventId, string processorId, string processorName)
    {
        var client = await _db.GetClientAsync(databaseType);
        var existing = await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId && h.ProcessorId == processorId)
            .FirstAsync();

        if (existing != null) return existing;

        var newHandle = new EventHandle
        {
            EventId = eventId,
            ProcessorId = processorId,
            ProcessorName = processorName,
            HandleTimes = 0,
            IsFinished = false,
            LastHandleStatus = HandleStatus.Unhandled,
            CreateDatetime = DateTime.Now
        };

        newHandle.Id = await client.Insertable(newHandle).ExecuteReturnIdentityAsync();
        _logger.LogDebug("[{DatabaseType}] 创建处理记录: EventId={EventId}, Processor={ProcessorName}",
            databaseType, eventId, processorName);
        return newHandle;
    }

    /// <summary>
    /// 获取事件的所有处理记录
    /// </summary>
    public async Task<List<EventHandle>> GetByEventIdAsync(string databaseType, int eventId)
    {
        var client = await _db.GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId)
            .ToListAsync();
    }

    /// <summary>
    /// 记录单次处理日志
    /// </summary>
    public async Task<EventHandleLog> LogAsync(string databaseType, EventHandle handle, ExecutionResult result)
    {
        var status = result.Success && (result.SendResult?.Success ?? true)
            ? HandleStatus.Success
            : HandleStatus.Fail;

        var log = new EventHandleLog
        {
            EventHandleId = handle.Id,
            EventId = handle.EventId,
            ProcessorId = handle.ProcessorId,
            ProcessorName = handle.ProcessorName,
            HandleTimes = handle.HandleTimes + 1,
            NeedToSend = result.NeedToSend,
            RequestData = result.RequestInfo,
            ResponseData = result.SendResult?.ResponseContent,
            SendSuccess = result.SendResult?.Success,
            ExceptionMessage = result.ErrorMessage,
            Status = status,
            ExecutionTimeMs = result.ExecutionTimeMs,
            HandleDatetime = DateTime.Now
        };

        var client = await _db.GetClientAsync(databaseType);
        log.Id = await client.Insertable(log).ExecuteReturnIdentityAsync();

        _logger.LogDebug("[{DatabaseType}] 记录日志: HandleId={HandleId}, Processor={ProcessorName}, Status={Status}",
            databaseType, handle.Id, handle.ProcessorName, status);

        return log;
    }

    /// <summary>
    /// 标记完成
    /// </summary>
    public async Task MarkFinishedAsync(string databaseType, int handleId, string status, int logId)
    {
        var client = await _db.GetClientAsync(databaseType);
        await client.Updateable<EventHandle>()
            .SetColumns(h => new EventHandle
            {
                IsFinished = true,
                LastHandleStatus = status,
                LastHandleDatetime = DateTime.Now,
                LastHandleLogId = logId
            })
            .Where(h => h.Id == handleId)
            .ExecuteCommandAsync();

        _logger.LogInformation("[{DatabaseType}] 处理完成: HandleId={HandleId}, Status={Status}",
            databaseType, handleId, status);
    }

    /// <summary>
    /// 标记失败（增加处理次数）
    /// </summary>
    public async Task MarkFailedAsync(string databaseType, EventHandle handle, string status, int logId)
    {
        var client = await _db.GetClientAsync(databaseType);
        handle.HandleTimes++;
        handle.LastHandleStatus = status;
        handle.LastHandleDatetime = DateTime.Now;
        handle.LastHandleLogId = logId;

        await client.Updateable(handle).ExecuteCommandAsync();
        _logger.LogWarning("[{DatabaseType}] 处理失败: HandleId={HandleId}, Processor={ProcessorName}, Times={Times}",
            databaseType, handle.Id, handle.ProcessorName, handle.HandleTimes);
    }

    /// <summary>
    /// 获取待重试的处理记录
    /// </summary>
    public async Task<List<EventHandle>> GetPendingRetryAsync(string databaseType, int maxRetryTimes = 3)
    {
        var client = await _db.GetClientAsync(databaseType);
        return await client.Queryable<EventHandle>()
            .Where(h => !h.IsFinished && h.HandleTimes < maxRetryTimes && h.LastHandleStatus != HandleStatus.Success)
            .ToListAsync();
    }

    /// <summary>
    /// 获取处理统计
    /// </summary>
    public async Task<ProcessorStatistics> GetStatisticsAsync(string databaseType, string? processorId = null)
    {
        var client = await _db.GetClientAsync(databaseType);

        var query = client.Queryable<EventHandle>();
        if (!string.IsNullOrEmpty(processorId))
        {
            query = query.Where(h => h.ProcessorId == processorId);
        }

        var handles = await query.ToListAsync();

        return new ProcessorStatistics
        {
            TotalCount = handles.Count,
            SuccessCount = handles.Count(h => h.IsFinished && h.LastHandleStatus == HandleStatus.Success),
            FailedCount = handles.Count(h => h.IsFinished && h.LastHandleStatus != HandleStatus.Success),
            PendingCount = handles.Count(h => !h.IsFinished),
            ProcessingCount = handles.Count(h => !h.IsFinished && h.HandleTimes > 0)
        };
    }
}

