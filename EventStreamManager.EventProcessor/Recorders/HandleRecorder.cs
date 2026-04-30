using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Recorders;

public class HandleRecorder : IHandleRecorder
{
    private readonly IEventHandleRepository _repository;
    private readonly ILogger<HandleRecorder> _logger;

    public HandleRecorder(IEventHandleRepository repository, ILogger<HandleRecorder> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<EventHandle> GetOrCreateAsync(string databaseType, int eventId, string processorId, string processorName)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        ArgumentNullException.ThrowIfNull(processorId);
        ArgumentNullException.ThrowIfNull(processorName);

        var existing = await _repository.GetAsync(databaseType, eventId, processorId);
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

        try
        {
            var created = await _repository.CreateAsync(databaseType, newHandle);
            _logger.LogDebug("[{DatabaseType}] 创建处理记录: EventId={EventId}, Processor={ProcessorName}",
                databaseType, eventId, processorName);
            return created;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[{DatabaseType}] 插入处理记录冲突，尝试重新查询: EventId={EventId}, Processor={ProcessorName}",
                databaseType, eventId, processorName);

            var conflicted = await _repository.GetAsync(databaseType, eventId, processorId);
            if (conflicted != null) return conflicted;

            _logger.LogError(ex, "[{DatabaseType}] 重新查询处理记录仍然不存在: EventId={EventId}, Processor={ProcessorName}",
                databaseType, eventId, processorName);
            throw;
        }
    }

    public async Task<List<EventHandle>> GetByEventIdAsync(string databaseType, int eventId)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        return await _repository.GetByEventIdAsync(databaseType, eventId);
    }

    public async Task<EventHandleLog> LogAsync(string databaseType, EventHandle handle, ExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentNullException.ThrowIfNull(result);

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
            SendSuccess = result.SendResult?.Success ?? false,
            ScriptSuccess = result.Success,
            ExceptionMessage = result.ErrorMessage,
            Status = status,
            ExecutionTimeMs = result.ExecutionTimeMs,
            Reason = result.Reason,
            HandleDatetime = DateTime.Now
        };

        var created = await _repository.CreateLogAsync(databaseType, log);

        _logger.LogDebug("[{DatabaseType}] 记录日志: HandleId={HandleId}, Processor={ProcessorName}, Status={Status}",
            databaseType, handle.Id, handle.ProcessorName, status);

        return created;
    }

    public async Task MarkFinishedAsync(string databaseType, int handleId, string status, int logId)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        ArgumentNullException.ThrowIfNull(status);

        var handle = await _repository.GetByIdAsync(databaseType, handleId);
        if (handle == null)
        {
            _logger.LogError("[{DatabaseType}] 标记完成时找不到记录: HandleId={HandleId}", databaseType, handleId);
            throw new InvalidOperationException($"处理记录不存在: HandleId={handleId}");
        }

        handle.IsFinished = true;
        handle.LastHandleStatus = status;
        handle.LastHandleDatetime = DateTime.Now;
        handle.LastHandleLogId = logId;

        await _repository.UpdateAsync(databaseType, handle);

        _logger.LogInformation("[{DatabaseType}] 处理完成: HandleId={HandleId}, Status={Status}",
            databaseType, handleId, status);
    }

    public async Task MarkFailedAsync(string databaseType, EventHandle handle, string status, int logId)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentNullException.ThrowIfNull(status);

        handle.HandleTimes++;
        handle.LastHandleStatus = status;
        handle.LastHandleDatetime = DateTime.Now;
        handle.LastHandleLogId = logId;

        await _repository.UpdateAsync(databaseType, handle);

        _logger.LogWarning("[{DatabaseType}] 处理失败: HandleId={HandleId}, Processor={ProcessorName}, Times={Times}",
            databaseType, handle.Id, handle.ProcessorName, handle.HandleTimes);
    }

    public async Task MarkRetryExhaustedAsync(string databaseType, EventHandle handle, string status, int logId)
    {
        ArgumentNullException.ThrowIfNull(databaseType);
        ArgumentNullException.ThrowIfNull(handle);
        ArgumentNullException.ThrowIfNull(status);

        handle.HandleTimes++;
        handle.IsFinished = true;
        handle.IsDeadLetter = true;
        handle.LastHandleStatus = status;
        handle.LastHandleDatetime = DateTime.Now;
        handle.LastHandleLogId = logId;

        await _repository.UpdateAsync(databaseType, handle);

        _logger.LogError("[{DatabaseType}] 重试次数耗尽，标记为死信: HandleId={HandleId}, Processor={ProcessorName}, Times={Times}",
            databaseType, handle.Id, handle.ProcessorName, handle.HandleTimes);
    }
}
