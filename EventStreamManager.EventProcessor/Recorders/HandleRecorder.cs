using System.Text.Json;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace EventStreamManager.EventProcessor.Recorders;

/// <summary>
/// 处理记录器 - 记录事件处理状态和日志
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
    /// 获取或创建处理记录
    /// </summary>
    public async Task<EventHandle> GetOrCreateAsync(string databaseType, int eventId, string handleType)
    {
        var client = await _db.GetClientAsync(databaseType);
        var existing = await client.Queryable<EventHandle>()
            .Where(h => h.EventId == eventId && h.HandleType == handleType)
            .FirstAsync();

        if (existing != null) return existing;

        var newHandle = new EventHandle
        {
            EventId = eventId,
            HandleType = handleType,
            HandleTimes = 0,
            IsFinished = false,
            LastHandleStatus = HandleStatus.Unhandled
        };

        newHandle.Id = await client.Insertable(newHandle).ExecuteReturnIdentityAsync();
        _logger.LogDebug("[{DatabaseType}] 创建处理记录: EventId={EventId}", databaseType, eventId);
        return newHandle;
    }

    /// <summary>
    /// 记录处理日志
    /// </summary>
    public async Task<EventHandleLog> LogAsync(string databaseType, EventHandle handle, List<ExecutionResult> results)
    {
        var allSuccess = results.All(r => r.Success && (r.SendResult?.Success ?? true));
        var status = allSuccess ? HandleStatus.Success : HandleStatus.Fail;

        var log = new EventHandleLog
        {
            EventHandleId = handle.Id,
            HandleTimes = handle.HandleTimes + 1,
            Status = status,
            RequestData = JsonSerializer.Serialize(results.Select(r => new { r.ProcessorName, r.RequestInfo })),
            ResponseData = JsonSerializer.Serialize(results.Select(r => new { r.ProcessorName, r.SendResult })),
            ExceptionMessage = string.Join("\n", results
                .Where(r => !string.IsNullOrEmpty(r.ErrorMessage))
                .Select(r => $"{r.ProcessorName}: {r.ErrorMessage}"))
        };

        var client = await _db.GetClientAsync(databaseType);
        log.Id = await client.Insertable(log).ExecuteReturnIdentityAsync();

        _logger.LogDebug("[{DatabaseType}] 记录日志: HandleId={HandleId}, Status={Status}",
            databaseType, handle.Id, status);

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

        _logger.LogInformation("[{DatabaseType}] 处理完成: HandleId={HandleId}", databaseType, handleId);
    }

    /// <summary>
    /// 标记失败
    /// </summary>
    public async Task MarkFailedAsync(string databaseType, EventHandle handle, string status, int logId)
    {
        var client = await _db.GetClientAsync(databaseType);
        handle.HandleTimes++;
        handle.LastHandleStatus = status;
        handle.LastHandleDatetime = DateTime.Now;
        handle.LastHandleLogId = logId;

        await client.Updateable(handle).ExecuteCommandAsync();
        _logger.LogWarning("[{DatabaseType}] 处理失败: HandleId={HandleId}, Times={Times}",
            databaseType, handle.Id, handle.HandleTimes);
    }
}
