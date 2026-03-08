using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

/// <summary>
/// 事件处理日志查询
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventLogController : ControllerBase
{
    private readonly ISqlSugarContext _db;

    public EventLogController(ISqlSugarContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 查询处理记录列表
    /// </summary>
    [HttpGet("handles")]
    public async Task<ActionResult> GetHandles(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            
            var query = client.Queryable<EventHandle>()
                .WhereIF(eventId.HasValue, h => h.EventId == eventId)
                .WhereIF(!string.IsNullOrEmpty(status), h => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, h => h.IsFinished == isFinished)
                .OrderByDescending(h => h.LastHandleDatetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await query.ToListAsync();
            var total = await client.Queryable<EventHandle>()
                .WhereIF(eventId.HasValue, h => h.EventId == eventId)
                .WhereIF(!string.IsNullOrEmpty(status), h => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, h => h.IsFinished == isFinished)
                .CountAsync();

            return Ok(new { list, total, page, pageSize });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 查询单条处理记录
    /// </summary>
    [HttpGet("handles/{databaseType}/{id}")]
    public async Task<ActionResult> GetHandle(string databaseType, int id)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var handle = await client.Queryable<EventHandle>()
                .Where(h => h.Id == id)
                .FirstAsync();

            if (handle == null)
                return NotFound(new { message = "记录不存在" });

            return Ok(handle);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 查询处理日志列表
    /// </summary>
    [HttpGet("logs")]
    public async Task<ActionResult> GetLogs(
        [FromQuery] string databaseType,
        [FromQuery] int? eventHandleId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var query = client.Queryable<EventHandleLog>()
                .WhereIF(eventHandleId.HasValue, l => l.EventHandleId == eventHandleId)
                .WhereIF(!string.IsNullOrEmpty(status), l => l.Status == status)
                .OrderByDescending(l => l.HandleDatetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await query.ToListAsync();
            var total = await client.Queryable<EventHandleLog>()
                .WhereIF(eventHandleId.HasValue, l => l.EventHandleId == eventHandleId)
                .WhereIF(!string.IsNullOrEmpty(status), l => l.Status == status)
                .CountAsync();

            return Ok(new { list, total, page, pageSize });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 查询单条日志详情
    /// </summary>
    [HttpGet("logs/{databaseType}/{id}")]
    public async Task<ActionResult> GetLog(string databaseType, int id)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var log = await client.Queryable<EventHandleLog>()
                .Where(l => l.Id == id)
                .FirstAsync();

            if (log == null)
                return NotFound(new { message = "日志不存在" });

            return Ok(log);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 查询事件及处理记录（联合查询）
    /// </summary>
    [HttpGet("event-with-handles/{databaseType}/{eventId}")]
    public async Task<ActionResult> GetEventWithHandles(string databaseType, int eventId)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            // 查询事件
            var evt = await client.Queryable<Event>()
                .Where(e => e.Id == eventId)
                .FirstAsync();

            if (evt == null)
                return NotFound(new { message = "事件不存在" });

            // 查询处理记录
            var handles = await client.Queryable<EventHandle>()
                .Where(h => h.EventId == eventId)
                .ToListAsync();

            // 查询日志
            var handleIds = handles.Select(h => h.Id).ToList();
            var logs = handleIds.Count > 0
                ? await client.Queryable<EventHandleLog>()
                    .Where(l => handleIds.Contains(l.EventHandleId))
                    .ToListAsync()
                : new List<EventHandleLog>();

            return Ok(new
            {
                @event = evt,
                handles,
                logs
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// 统计处理状态
    /// </summary>
    [HttpGet("stats/{databaseType}")]
    public async Task<ActionResult> GetStats(string databaseType)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var total = await client.Queryable<EventHandle>().CountAsync();
            var finished = await client.Queryable<EventHandle>()
                .Where(h => h.IsFinished).CountAsync();
            var pending = await client.Queryable<EventHandle>()
                .Where(h => !h.IsFinished).CountAsync();
            var success = await client.Queryable<EventHandle>()
                .Where(h => h.LastHandleStatus == "Success").CountAsync();
            var failed = await client.Queryable<EventHandle>()
                .Where(h => h.LastHandleStatus == "Fail" || h.LastHandleStatus == "Exception")
                .CountAsync();

            return Ok(new
            {
                total,
                finished,
                pending,
                success,
                failed
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
