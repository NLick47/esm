using EventStreamManager.EventProcessor.Entities;
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

    /// <summary>
    /// 查询处理记录列表
    /// </summary>
    [HttpGet("handles")]
    public async Task<IActionResult> GetHandles(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] string? processorId = null,
        [FromQuery] string? processorName = null,
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
                .WhereIF(!string.IsNullOrEmpty(processorId), h => h.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(processorName), h => h.ProcessorName.Contains(processorName!))
                .WhereIF(!string.IsNullOrEmpty(status), h => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, h => h.IsFinished == isFinished)
                .OrderByDescending(h => h.LastHandleDatetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await query.ToListAsync();
            var total = await client.Queryable<EventHandle>()
                .WhereIF(eventId.HasValue, h => h.EventId == eventId)
                .WhereIF(!string.IsNullOrEmpty(processorId), h => h.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(processorName), h => h.ProcessorName.Contains(processorName!))
                .WhereIF(!string.IsNullOrEmpty(status), h => h.LastHandleStatus == status)
                .WhereIF(isFinished.HasValue, h => h.IsFinished == isFinished)
                .CountAsync();

            return Ok(new { list, total, page, pageSize }, "获取处理记录列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理记录列表失败");
            return Error("获取处理记录列表失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询单条处理记录
    /// </summary>
    [HttpGet("handles/{databaseType}/{id}")]
    public async Task<IActionResult> GetHandle(string databaseType, int id)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var handle = await client.Queryable<EventHandle>()
                .Where(h => h.Id == id)
                .FirstAsync();

            if (handle == null)
                return Fail("记录不存在", 404);

            return Ok(handle, "获取处理记录成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理记录失败");
            return Error("获取处理记录失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询处理日志列表
    /// </summary>
    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] int? eventHandleId = null,
        [FromQuery] string? processorId = null,
        [FromQuery] string? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var query = client.Queryable<EventHandleLog>()
                .WhereIF(eventId.HasValue, l => l.EventId == eventId)
                .WhereIF(eventHandleId.HasValue, l => l.EventHandleId == eventHandleId)
                .WhereIF(!string.IsNullOrEmpty(processorId), l => l.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(status), l => l.Status == status)
                .OrderByDescending(l => l.HandleDatetime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await query.ToListAsync();
            var total = await client.Queryable<EventHandleLog>()
                .WhereIF(eventId.HasValue, l => l.EventId == eventId)
                .WhereIF(eventHandleId.HasValue, l => l.EventHandleId == eventHandleId)
                .WhereIF(!string.IsNullOrEmpty(processorId), l => l.ProcessorId == processorId)
                .WhereIF(!string.IsNullOrEmpty(status), l => l.Status == status)
                .CountAsync();

            return Ok(new { list, total, page, pageSize }, "获取处理日志列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理日志列表失败");
            return Error("获取处理日志列表失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询单条日志详情
    /// </summary>
    [HttpGet("logs/{databaseType}/{id}")]
    public async Task<IActionResult> GetLog(string databaseType, int id)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);
            var log = await client.Queryable<EventHandleLog>()
                .Where(l => l.Id == id)
                .FirstAsync();

            if (log == null)
                return Fail("日志不存在", 404);

            return Ok(log, "获取日志详情成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日志详情失败");
            return Error("获取日志详情失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询事件及处理记录（联合查询）
    /// </summary>
    [HttpGet("event-with-handles/{databaseType}/{eventId}")]
    public async Task<IActionResult> GetEventWithHandles(string databaseType, int eventId)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            // 查询事件
            var evt = await client.Queryable<Event>()
                .Where(e => e.Id == eventId)
                .FirstAsync();

            if (evt == null)
                return Fail("事件不存在", 404);

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
            }, "获取事件及处理记录成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取事件及处理记录失败");
            return Error("获取事件及处理记录失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 统计处理状态
    /// </summary>
    [HttpGet("stats/{databaseType}")]
    public async Task<IActionResult> GetStats(string databaseType)
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
            }, "获取统计状态成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取统计状态失败");
            return Error("获取统计状态失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 按处理器统计处理状态
    /// </summary>
    [HttpGet("stats/{databaseType}/by-processor")]
    public async Task<IActionResult> GetStatsByProcessor(string databaseType)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            // 按处理器分组统计
            var handles = await client.Queryable<EventHandle>().ToListAsync();
            
            var stats = handles
                .GroupBy(h => new { h.ProcessorId, h.ProcessorName })
                .Select(g => new
                {
                    processorId = g.Key.ProcessorId,
                    processorName = g.Key.ProcessorName,
                    totalCount = g.Count(),
                    successCount = g.Count(h => h.IsFinished && h.LastHandleStatus == "Success"),
                    failedCount = g.Count(h => h.IsFinished && h.LastHandleStatus != "Success"),
                    pendingCount = g.Count(h => !h.IsFinished),
                    avgHandleTimes = g.Average(h => h.HandleTimes)
                })
                .OrderByDescending(s => s.totalCount)
                .ToList();

            return Ok(stats, "按处理器统计成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按处理器统计失败");
            return Error("按处理器统计失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 查询失败的处理记录
    /// </summary>
    [HttpGet("failed-handles/{databaseType}")]
    public async Task<IActionResult> GetFailedHandles(
        string databaseType,
        [FromQuery] string? processorId = null,
        [FromQuery] int maxRetryTimes = 3,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var client = await _db.GetClientAsync(databaseType);

            var query = client.Queryable<EventHandle>()
                .Where(h => !h.IsFinished && h.HandleTimes < maxRetryTimes)
                .WhereIF(!string.IsNullOrEmpty(processorId), h => h.ProcessorId == processorId)
                .OrderBy(h => h.HandleTimes)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var list = await query.ToListAsync();
            var total = await client.Queryable<EventHandle>()
                .Where(h => !h.IsFinished && h.HandleTimes < maxRetryTimes)
                .WhereIF(!string.IsNullOrEmpty(processorId), h => h.ProcessorId == processorId)
                .CountAsync();

            return Ok(new { list, total, page, pageSize }, "获取失败处理记录成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取失败处理记录失败");
            return Error("获取失败处理记录失败", data: new { error = ex.Message });
        }
    }
}