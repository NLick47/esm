using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventLog;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

/// <summary>
/// 事件处理日志查询
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventLogController : BaseController
{
    private readonly ISqlSugarContext _db;
    private readonly ILogger<EventLogController> _logger;

    public EventLogController(
        ISqlSugarContext db,
        ILogger<EventLogController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("handles")]
    public async Task<IActionResult> GetHandles(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] string? processorId = null,
        [FromQuery] string? processorName = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] string? eventCode = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var query = client.Queryable<EventHandle>()
                .LeftJoin<EventHandleLog>((h, l) => h.Id == l.EventHandleId)
                .LeftJoin<Event>((h, l, e) => h.EventId == e.Id) // 关联Event表获取事件信息
                .WhereIF(eventId.HasValue, (h, l, e) => h.EventId == eventId)
                .WhereIF(!string.IsNullOrEmpty(processorId), (h, l, e) => h.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(processorName), (h, l, e) => h.ProcessorName.Contains(processorName!))
                .WhereIF(!string.IsNullOrEmpty(status), (h, l, e) => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, (h, l, e) => h.IsFinished == isFinished)
                .WhereIF(!string.IsNullOrEmpty(eventCode),
                    (h, l, e) => e.EventCode == eventCode) // 使用Event表的EventCode筛选
                .WhereIF(startDate.HasValue, (h, l, e) => e.CreateDatetime >= startDate) // 使用Event表的创建时间
                .WhereIF(endDate.HasValue, (h, l, e) => e.CreateDatetime <= endDate) // 使用Event表的创建时间
                .OrderByDescending((h, l, e) => h.LastHandleDatetime)
                .Select((h, l, e) => new EventHandleResult()
                {
                    Id = h.Id,
                    EventId = h.EventId,
                    ProcessorId = h.ProcessorId,
                    ProcessorName = h.ProcessorName,
                    HandleTimes = h.HandleTimes,
                    LastHandleStatus = h.LastHandleStatus!,
                    LastHandleMessage = l.ExceptionMessage,
                    LastHandleDatetime = h.LastHandleDatetime,
                    LastHandleElapsedMs = l.ExecutionTimeMs,
                    IsFinished = h.IsFinished,
                    CreateDatetime = e.CreateDatetime,
                    EventCode = e.EventCode,
                    EventName = e.EventName,
                });

            // 获取总数
            var total = await client.Queryable<EventHandle>()
                .LeftJoin<EventHandleLog>((h, l) => h.Id == l.EventHandleId)
                .LeftJoin<Event>((h, l, e) => h.EventId == e.Id)
                .WhereIF(eventId.HasValue, (h, l, e) => h.EventId == eventId)
                .WhereIF(!string.IsNullOrEmpty(processorId), (h, l, e) => h.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(processorName), (h, l, e) => h.ProcessorName.Contains(processorName!))
                .WhereIF(!string.IsNullOrEmpty(status), (h, l, e) => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, (h, l, e) => h.IsFinished == isFinished)
                .WhereIF(!string.IsNullOrEmpty(eventCode), (h, l, e) => e.EventCode == eventCode)
                .WhereIF(startDate.HasValue, (h, l, e) => e.CreateDatetime >= startDate)
                .WhereIF(endDate.HasValue, (h, l, e) => e.CreateDatetime <= endDate)
                .CountAsync();

            // 分页
            var list = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { list, total, page, pageSize }, "获取处理记录列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理记录列表失败");
            return Error("获取处理记录列表失败", data: new { error = ex.Message });
        }
    }
}