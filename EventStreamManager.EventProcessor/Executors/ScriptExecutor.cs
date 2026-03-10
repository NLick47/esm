using System.Diagnostics;
using System.Text.Json;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Execution.Parameter;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Executors;

/// <summary>
/// 脚本执行器 - 执行JS处理器
/// </summary>
public class ScriptExecutor
{
    private readonly IJavaScriptExecutionService _jsService;
    private readonly ISqlSugarContext _db;
    private readonly ILogger<ScriptExecutor> _logger;

    public ScriptExecutor(
        IJavaScriptExecutionService jsService,
        ISqlSugarContext db,
        ILogger<ScriptExecutor> logger)
    {
        _jsService = jsService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// 执行单个处理器
    /// </summary>
    public async Task<ExecutionResult> ExecuteAsync(ScriptContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ExecutionResult
        {
            ProcessorId = context.ProcessorId,
            ProcessorName = context.ProcessorName
        };

        try
        {
            if (context.ProcessorConfig == null || !context.ProcessorConfig.Enabled)
            {
                result.Success = true;
                result.NeedToSend = false;
                result.Reason = "处理器未配置或已禁用";
                return result;
            }

            var jsData = BuildJsData(context);
            var execResult = await _jsService.ExecuteProcessAsync(context.ProcessorConfig.Code, jsData);

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            if (!execResult.Success)
            {
                result.Success = false;
                result.ErrorMessage = execResult.ErrorMessage;
                result.ConsoleOutput = execResult.ConsoleOutput;
            }
            else
            {
                result.Success = true;
                result.NeedToSend = execResult.NeedToSend;
                result.RequestInfo = execResult.RequestInfo;
                result.Reason = execResult.Reason;
                result.ConsoleOutput = execResult.ConsoleOutput;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "[{DatabaseType}] 执行失败: {ProcessorName}",
                context.DatabaseType, context.ProcessorName);
        }

        return result;
    }

    /// <summary>
    /// 查询扩展数据
    /// </summary>
    public async Task<Dictionary<string, object>?> QueryDataAsync(
        string databaseType, string sqlTemplate, Event eventData)
    {
        try
        {
            var sql = ReplaceVariables(sqlTemplate, eventData);
            var client = await _db.GetClientAsync(databaseType);
            var result = await client.Ado.SqlQueryAsync<dynamic>(sql);

            if (result?.Any() == true && result.First() is IDictionary<string, object> dict)
            {
                return dict.ToDictionary(k => k.Key, v => v.Value ?? string.Empty);
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 查询扩展数据失败", databaseType);
            return null;
        }
    }

    private EnhancedQueryData BuildJsData(ScriptContext context)
    {
       
        
        var data = new EnhancedQueryData
        {
            Rows = context.QueryResult != null 
                ? new List<Dictionary<string, object>> { context.QueryResult }
                : new List<Dictionary<string, object>>(),
            
            Database = new DatabaseInfo
            {
                Type = context.DatabaseType,
            },
            
            Context = new ContextInfo
            {
                EventId = context.Event.Id.ToString(),
                strEventReferenceId = context.Event.StrEventReferenceId,
                EventType = context.Event.EventType,
                EventName = context.Event.EventName,
                EventCode = context.Event.EventCode,
                OperatorName = context.Event.OperatorName,
                OperatorCode = context.Event.OperatorCode,
                CreateDatetime = context.Event.CreateDatetime,
                ExtenData = context.Event.ExtenData ?? ""
            },

        
            Processor = new ProcessorInfo
            {
                Id = context.ProcessorId,
                Name = context.ProcessorName,
                Enabled = context.ProcessorConfig?.Enabled
            }
        };
        return data;
    }

    private string ReplaceVariables(string sql, Event eventData)
    {
        return sql
            .Replace("${strEventReferenceId}", $"'{eventData.StrEventReferenceId}'")
            .Replace("${eventId}", eventData.Id.ToString())
            .Replace("${eventType}", $"'{eventData.EventType}'")
            .Replace("${eventCode}", $"'{eventData.EventCode}'")
            .Replace("${operatorCode}", $"'{eventData.OperatorCode}'");
    }
}
