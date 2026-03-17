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
    private readonly IEventLogService _eventLogService;
    private readonly ILogger<EventLogController> _logger;

    public EventLogController(
        IEventLogService eventLogService,
        ILogger<EventLogController> logger)
    {
        _eventLogService = eventLogService;
        _logger = logger;
    }


    [HttpGet("handles")]
    public async Task<IActionResult> GetHandles(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] string? strEventReferenceId = null,
        [FromQuery] string? processorId = null,
        [FromQuery] string? processorName = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] string? eventCode = null,
        [FromQuery] string? requestDataKeyword = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _eventLogService.GetEventHandlesAsync(
                databaseType,
                eventId,
                strEventReferenceId,
                processorId,
                processorName,
                status,
                isFinished,
                eventCode,
                requestDataKeyword,
                startDate,
                endDate,
                page,
                pageSize);
            return Ok(result, "获取处理记录列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理记录列表失败");
            return Error("获取处理记录列表失败", data: new { error = ex.Message });
        }
    }

    /// <summary>
    /// 导出事件处理记录到Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportHandles(
        [FromQuery] string databaseType,
        [FromQuery] int? eventId = null,
        [FromQuery] string? strEventReferenceId = null,
        [FromQuery] string? processorId = null,
        [FromQuery] string? processorName = null,
        [FromQuery] string? status = null,
        [FromQuery] bool? isFinished = null,
        [FromQuery] string? eventCode = null,
        [FromQuery] string? requestDataKeyword = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var excelBytes = await _eventLogService.ExportEventHandlesToExcelAsync(
                databaseType,
                eventId,
                strEventReferenceId,
                processorId,
                processorName,
                status,
                isFinished,
                eventCode,
                requestDataKeyword,
                startDate,
                endDate);

            var fileName = $"{databaseType}事件处理记录_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出处理记录失败");
            return Error("导出处理记录失败", data: new { error = ex.Message });
        }
    }
}