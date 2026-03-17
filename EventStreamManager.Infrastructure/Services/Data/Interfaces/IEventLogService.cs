using EventStreamManager.Infrastructure.Models;
using EventStreamManager.Infrastructure.Models.EventLog;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IEventLogService
{
    /// <summary>
    /// 获取事件处理记录列表
    /// </summary>
    /// <param name="databaseType">数据库类型</param>
    /// <param name="eventId">事件ID</param>
    /// <param name="strEventReferenceId">事件引用ID</param>
    /// <param name="processorId">处理器ID</param>
    /// <param name="processorName">处理器名称</param>
    /// <param name="status">处理状态</param>
    /// <param name="isFinished">是否完成</param>
    /// <param name="eventCode">事件代码</param>
    /// <param name="requestDataKeyword">请求数据关键字（模糊查询）</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="page">页码</param>
    /// <param name="pageSize">每页大小</param>
    /// <returns>分页的事件处理记录</returns>
    Task<PagedResult<EventHandleResult>> GetEventHandlesAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? processorName = null,
        string? status = null,
        bool? isFinished = null,
        string? eventCode = null,
        string? requestDataKeyword = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20);
    
    
    /// <summary>
    /// 导出事件处理记录到Excel
    /// </summary>
    /// <param name="databaseType">数据库类型</param>
    /// <param name="eventId">事件ID</param>
    /// <param name="strEventReferenceId">事件引用ID</param>
    /// <param name="processorId">处理器ID</param>
    /// <param name="processorName">处理器名称</param>
    /// <param name="status">处理状态</param>
    /// <param name="isFinished">是否完成</param>
    /// <param name="eventCode">事件代码</param>
    /// <param name="requestDataKeyword">请求数据关键字（模糊查询）</param>
    /// <param name="startDate">开始日期</param>
    /// <param name="endDate">结束日期</param>
    /// <param name="maxRows">最大行数</param>
    /// <returns>分页的事件处理记录</returns>
    Task<byte[]> ExportEventHandlesToExcelAsync(
        string databaseType,
        int? eventId = null,
        string? strEventReferenceId = null,
        string? processorId = null,
        string? processorName = null,
        string? status = null,
        bool? isFinished = null,
        string? eventCode = null,
        string? requestDataKeyword = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int maxRows = 10000);
}