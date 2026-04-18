using ClosedXML.Excel;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models;
using EventStreamManager.Infrastructure.Models.EventLog;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;
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
    /// 生成Excel文件
    /// </summary>
    private byte[] GenerateExcel(List<EventHandleResult> data)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("事件处理记录");

        // 设置标题行
        var titles = new[]
        {
            "事件ID", "事件代码", "事件名称", "引用ID", "处理器名称",
            "请求体", "响应体",
            "处理次数", "最后状态", "最后消息", "最后处理时间",
            "处理耗时(ms)", "是否完成", "事件创建时间"
        };

        // 设置标题样式
        for (int i = 0; i < titles.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = titles[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // 填充数据
        for (int i = 0; i < data.Count; i++)
        {
            var item = data[i];
            var row = i + 2;

            worksheet.Cell(row, 1).Value = item.EventId;
            worksheet.Cell(row, 2).Value = item.EventCode;
            worksheet.Cell(row, 3).Value = item.EventName;
            worksheet.Cell(row, 4).Value = item.StrEventReferenceId;
            worksheet.Cell(row, 5).Value = item.ProcessorName;
            worksheet.Cell(row, 6).Value = item.RequestData;
            worksheet.Cell(row, 7).Value = item.ResponseData;
            
            worksheet.Cell(row, 8).Value = item.HandleTimes;
            worksheet.Cell(row, 9).Value = item.LastHandleStatus;
            worksheet.Cell(row, 10).Value = item.LastHandleMessage ?? "";
            worksheet.Cell(row, 11).Value = item.LastHandleDatetime?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";
            worksheet.Cell(row, 12).Value = item.LastHandleElapsedMs;
            worksheet.Cell(row, 13).Value = item.IsFinished;
            worksheet.Cell(row, 14).Value = item.CreateDatetime.ToString("yyyy-MM-dd HH:mm:ss");

            // 根据状态设置颜色
            var statusCell = worksheet.Cell(row, 7);
            if (!string.IsNullOrEmpty(item.LastHandleStatus))
            {
                switch (item.LastHandleStatus.ToLower())
                {
                    case "success":
                        statusCell.Style.Font.FontColor = XLColor.Green;
                        break;
                    case "fail":
                    case "exception":
                        statusCell.Style.Font.FontColor = XLColor.Red;
                        statusCell.Style.Font.Bold = true;
                        break;
                }
            }

            // 设置边框和数据对齐
            for (int j = 1; j <= titles.Length; j++)
            {
                var cell = worksheet.Cell(row, j);
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                // 数字列右对齐
                if (j == 1 || j == 6 || j == 10)
                {
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }
                // 日期列居中对齐
                else if (j == 9 || j == 12)
                {
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }
            }
        }

        // 自动调整列宽
        worksheet.Columns().AdjustToContents();

        // 设置列宽最小值和最大值
        for (int i = 1; i <= titles.Length; i++)
        {
            var col = worksheet.Column(i);
            if (col.Width < 8) col.Width = 8;
            if (col.Width > 50) col.Width = 50;
        }

        // 冻结标题行
        worksheet.SheetView.FreezeRows(1);

        // 转换为字节数组
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
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