using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventLog;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
using MiniExcelLibs;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Services.Data;

public class EventLogService : IEventLogService
{
    private readonly ISqlSugarContext _db;
    private readonly ILogger<EventLogService> _logger;

    public EventLogService(
        ISqlSugarContext db,
        ILogger<EventLogService> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 获取事件处理记录列表
    /// </summary>
    public async Task<(List<EventHandleResult> Items, int Total)> GetEventHandlesAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? status = null,
        string? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var query = BuildQuery(client, eventId, strEventReferenceId, processorId,
                status, eventCode, startDate, endDate);

            var total = await BuildCountQuery(client, eventId, strEventReferenceId, processorId,
                    status, eventCode, startDate, endDate)
                .CountAsync();

            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (list, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理记录列表失败，参数: {@Params}", new
            {
                databaseType, eventId, strEventReferenceId, processorId,
                status, eventCode, startDate, endDate, page, pageSize
            });
            throw;
        }
    }


    /// <summary>
    /// 导出事件处理记录到Excel
    /// </summary>
    public async Task<byte[]> ExportEventHandlesToExcelAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? status = null,
        string? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxRows = 10000)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var query = BuildQuery(client, eventId, strEventReferenceId, processorId,
                status, eventCode, startDate, endDate);

            var exportList = await query
                .Take(maxRows)
                .ToListAsync();

            if (exportList.Count >= maxRows)
            {
                _logger.LogWarning("导出数据已达到最大行数限制：{MaxRows}，可能不是完整数据", maxRows);
            }

            return GenerateExcel(exportList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出处理记录到Excel失败，参数: {@Params}", new
            {
                databaseType, eventId, strEventReferenceId, processorId,
                status, eventCode, startDate, endDate
            });
            throw;
        }
    }

    /// <summary>
    /// 重置死信状态
    /// </summary>
    public async Task<bool> ResetDeadLetterAsync(string databaseType, int handleId)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var handle = await client.Queryable<EventHandle>()
                .Where(h => h.Id == handleId)
                .FirstAsync();

            if (handle == null)
            {
                _logger.LogWarning("[{DatabaseType}] 重置死信失败: HandleId={HandleId} 不存在",
                    databaseType, handleId);
                return false;
            }

            if (!handle.IsDeadLetter)
            {
                _logger.LogWarning("[{DatabaseType}] 重置死信失败: HandleId={HandleId} 不是死信状态",
                    databaseType, handleId);
                return false;
            }

            handle.IsDeadLetter = false;
            handle.IsFinished = false;
            handle.HandleTimes = 0;
            handle.LastHandleStatus = HandleStatus.Unhandled;
            handle.LastHandleDatetime = null;
            handle.LastHandleLogId = null;

            await client.Updateable(handle).ExecuteCommandAsync();

            _logger.LogInformation("[{DatabaseType}] 死信已重置: HandleId={HandleId}, Processor={ProcessorName}",
                databaseType, handleId, handle.ProcessorName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 重置死信异常: HandleId={HandleId}",
                databaseType, handleId);
            throw;
        }
    }

    /// <summary>
    /// 生成Excel文件（使用 MiniExcel）
    /// </summary>
    private byte[] GenerateExcel(List<EventHandleResult> data)
    {
        var exportData = data.Select(item => new
        {
            事件ID = item.EventId,
            事件代码 = item.EventCode,
            事件名称 = item.EventName,
            引用ID = item.StrEventReferenceId,
            处理器名称 = item.ProcessorName,
            请求体 = item.RequestData,
            响应体 = item.ResponseData,
            处理次数 = item.HandleTimes,
            最后状态 = item.LastHandleStatus,
            最后消息 = item.LastHandleMessage,
            最后处理时间 = item.LastHandleDatetime?.ToString("yyyy-MM-dd HH:mm:ss"),
            处理耗时 = item.LastHandleElapsedMs,
            是否完成 = item.IsFinished,
            事件创建时间 = item.CreateDatetime.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();

        using var stream = new MemoryStream();
        MiniExcel.SaveAs(stream, exportData, sheetName: "事件处理记录");
        return stream.ToArray();
    }
    
    private ISugarQueryable<EventHandleResult> BuildQuery(
        ISqlSugarClient client,
        int? eventId,
        string? strEventReferenceId,
        string? processorId,
        string? status,
        string? eventCode,
        DateTime? startDate,
        DateTime? endDate)
    {
        var query = client.Queryable<EventHandle>()
            .LeftJoin<EventHandleLog>((h, l) => h.Id == l.EventHandleId)
            .LeftJoin<Event>((h, l, e) => h.EventId == e.Id)
            .WhereIF(eventId.HasValue, (h, l, e) => h.EventId == eventId)
            .WhereIF(!string.IsNullOrEmpty(strEventReferenceId),
                (h, l, e) => e.StrEventReferenceId == strEventReferenceId)
            .WhereIF(!string.IsNullOrEmpty(processorId), (h, l, e) => h.ProcessorId == processorId)
            .WhereIF(!string.IsNullOrEmpty(status), (h, l, e) => h.LastHandleStatus == status)
            .WhereIF(!string.IsNullOrEmpty(eventCode), (h, l, e) => e.EventCode == eventCode)
            .WhereIF(startDate.HasValue, (h, l, e) => e.CreateDatetime >= startDate)
            .WhereIF(endDate.HasValue, (h, l, e) => e.CreateDatetime <= endDate)
            .OrderByDescending((h, l, e) => h.LastHandleDatetime)
            .Select((h, l, e) => new EventHandleResult
            {
                Id = h.Id,
                EventId = h.EventId,
                StrEventReferenceId = e.StrEventReferenceId,
                ProcessorId = h.ProcessorId,
                ProcessorName = h.ProcessorName,
                HandleTimes = h.HandleTimes,
                LastHandleStatus = h.LastHandleStatus ?? string.Empty,
                LastHandleMessage = l.ExceptionMessage,
                LastHandleDatetime = h.LastHandleDatetime,
                NeedToSend = l.NeedToSend,
                ScriptSuccess = l.ScriptSuccess,
                SendSuccess = l.SendSuccess,
                IsDeadLetter = h.IsDeadLetter,
                Reason = l.Reason,
                LastHandleElapsedMs = l.ExecutionTimeMs,
                IsFinished = h.IsFinished,
                CreateDatetime = e.CreateDatetime,
                EventCode = e.EventCode,
                EventName = e.EventName,
                RequestData = l.RequestData,
                ResponseData = l.ResponseData,
            });

        return query;
    }

 
    private ISugarQueryable<EventHandle> BuildCountQuery(
        ISqlSugarClient client,
        int? eventId,
        string? strEventReferenceId,
        string? processorId,
        string? status,
        string? eventCode,
        DateTime? startDate,
        DateTime? endDate)
    {
        return client.Queryable<EventHandle>()
            .LeftJoin<EventHandleLog>((h, l) => h.Id == l.EventHandleId)
            .LeftJoin<Event>((h, l, e) => h.EventId == e.Id)
            .WhereIF(eventId.HasValue, (h, l, e) => h.EventId == eventId)
            .WhereIF(!string.IsNullOrEmpty(strEventReferenceId),
                (h, l, e) => e.StrEventReferenceId == strEventReferenceId)
            .WhereIF(!string.IsNullOrEmpty(processorId), (h, l, e) => h.ProcessorId == processorId)
            .WhereIF(!string.IsNullOrEmpty(status), (h, l, e) => h.LastHandleStatus == status)
            .WhereIF(!string.IsNullOrEmpty(eventCode), (h, l, e) => e.EventCode == eventCode)
            .WhereIF(startDate.HasValue, (h, l, e) => e.CreateDatetime >= startDate)
            .WhereIF(endDate.HasValue, (h, l, e) => e.CreateDatetime <= endDate)
            .Select(h => h);
    }
}