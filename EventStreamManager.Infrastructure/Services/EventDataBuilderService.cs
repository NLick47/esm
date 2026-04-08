using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Execution.Parameter;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services;

public class EventDataBuilderService : IEventDataBuilderService
{
    private readonly ISqlSugarContext _sqlSugarContext;
    private readonly ILogger<EventDataBuilderService> _logger;

    public EventDataBuilderService(
        ISqlSugarContext sqlSugarContext,
        ILogger<EventDataBuilderService> logger)
    {
        _sqlSugarContext = sqlSugarContext;
        _logger = logger;
    }

    public async Task<EnhancedQueryData> BuildEnhancedDataAsync(
        string databaseType,
        Event eventData,
        JsProcessor processor,
        CancellationToken ct = default)
    {
       
        var rows = await ExecuteProcessorQueryAsync(
            databaseType, 
            processor.SqlTemplate, 
            eventData, 
            ct);
        
        return new EnhancedQueryData
        {
            Rows = rows,
            Database = new DatabaseInfo { Type = databaseType },
            Context = new ContextInfo
            {
                EventId = eventData.Id.ToString(),
                StrEventReferenceId = eventData.StrEventReferenceId,
                EventType = eventData.EventType,
                EventName = eventData.EventName,
                EventCode = eventData.EventCode,
                OperatorName = eventData.OperatorName,
                OperatorCode = eventData.OperatorCode,
                CreateDatetime = eventData.CreateDatetime,
                ExtenData = eventData.ExtenData ?? ""
            },
            Processor = new ProcessorInfo
            {
                Id = processor.Id,
                Name = processor.Name,
                Enabled = processor.Enabled
            }
        };
    }

    public async Task<List<Dictionary<string, object>>> ExecuteProcessorQueryAsync(
        string databaseType,
        string sqlTemplate,
        Event eventData,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(sqlTemplate))
            return new List<Dictionary<string, object>>();

        var sql = ReplaceVariables(sqlTemplate, eventData);
        
        _logger.LogDebug("执行SQL: {Sql}", sql);
        
        return await _sqlSugarContext.ExecuteQueryAsync(databaseType, sql);
    }

    
    
    
    public async Task<EnhancedQueryData> BuildEnhancedDataForExamineAsync(
        string databaseType,
        string sqlTemplate,
        string examineId,
        JsProcessor? processor,
        CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(sqlTemplate))
            return new EnhancedQueryData
            {
                Rows = new List<Dictionary<string, object>>(),
                Database = new DatabaseInfo { Type = databaseType },
                Context = new ContextInfo(),
                Processor = processor == null ? new ProcessorInfo() : new ProcessorInfo
                {
                    Id = processor.Id,
                    Name = processor.Name,
                    Enabled = processor.Enabled
                }
            };

        var sql = sqlTemplate.Replace("${strEventReferenceId}", "@strexamineId");
        var parameters = new { strexamineId = examineId };
    
        _logger.LogDebug("执行Examine SQL: {Sql}", sql);
    
        var rows = await _sqlSugarContext.ExecuteQueryAsync(databaseType, sql, parameters);

        return new EnhancedQueryData
        {
            Rows = rows,
            Database = new DatabaseInfo { Type = databaseType },
            Context = new ContextInfo(), 
            Processor = processor == null ? new ProcessorInfo() : new ProcessorInfo
            {
                Id = processor.Id,
                Name = processor.Name,
                Enabled = processor.Enabled
            }
        };
    }
    private static string ReplaceVariables(string sql, Event eventData)
    {
        return sql
            .Replace("${strEventReferenceId}", $"'{eventData.StrEventReferenceId}'")
            .Replace("${eventId}", eventData.Id.ToString())
            .Replace("${eventType}", $"'{eventData.EventType}'")
            .Replace("${eventCode}", $"'{eventData.EventCode}'")
            .Replace("${operatorCode}", $"'{eventData.OperatorCode}'");
    }
}