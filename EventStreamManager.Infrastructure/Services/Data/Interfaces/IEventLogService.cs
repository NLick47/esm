using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventLog;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IEventLogService
{
    /// <summary>
    /// 获取事件处理记录列表
    /// </summary>
    Task<(List<EventHandleResult> Items, int Total)> GetEventHandlesAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? status = null,
        string? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20);

    /// <summary>
    /// 获取事件处理记录统计
    /// </summary>
    Task<EventHandleStats> GetEventHandleStatsAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? status = null,
        string? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// 导出事件处理记录到Excel
    /// </summary>
    Task<byte[]> ExportEventHandlesToExcelAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? status = null,
        string? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxRows = 10000);

    /// <summary>
    /// 重置死信状态，允许重新处理
    /// </summary>
    Task<bool> ResetDeadLetterAsync(string databaseType, int handleId);

    /// <summary>
    /// 获取单条处理记录详情（含最新日志及诊断信息）
    /// </summary>
    Task<(EventHandleResult? Handle, EventHandleLogDetail? Detail)> GetHandleDetailAsync(string databaseType, int handleId);
}
