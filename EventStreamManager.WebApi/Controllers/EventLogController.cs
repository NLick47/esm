using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Models.Requests;
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

    public EventLogController(
        IEventLogService eventLogService)
    {
        _eventLogService = eventLogService;
    }

    [HttpGet("handles")]
    public async Task<IActionResult> GetHandles([FromQuery] GetEventHandlesRequest request)
    {
        var (items, total) = await _eventLogService.GetEventHandlesAsync(
            request.DatabaseType,
            request.EventId,
            request.StrEventReferenceId,
            request.ProcessorId,
            request.Status,
            request.EventCode,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize);

        return PageData(items, total, request.Page, request.PageSize, "获取处理记录列表成功");
    }

    /// <summary>
    /// 导出事件处理记录到Excel
    /// </summary>
    [HttpGet("export")]
    public async Task<IActionResult> ExportHandles([FromQuery] ExportEventHandlesRequest request)
    {
        var excelBytes = await _eventLogService.ExportEventHandlesToExcelAsync(
            request.DatabaseType,
            request.EventId,
            request.StrEventReferenceId,
            request.ProcessorId,
            request.Status,
            request.EventCode,
            request.StartDate,
            request.EndDate,
            request.MaxRows);

        var fileName = $"{request.DatabaseType}事件处理记录_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    /// <summary>
    /// 重置死信状态，允许重新处理
    /// </summary>
    [HttpPost("{handleId}/retry")]
    public async Task<IActionResult> RetryDeadLetter(string databaseType, int handleId)
    {
        var result = await _eventLogService.ResetDeadLetterAsync(databaseType, handleId);
        if (result)
        {
            return OkMessage("死信已重置，将在下次扫描时重新处理");
        }
        return Fail("重置失败，该记录不存在或不是死信状态");
    }
}
