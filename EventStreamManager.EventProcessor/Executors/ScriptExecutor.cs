using System.Diagnostics;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.JSFunction.Runtime;
using Microsoft.Extensions.Logging;
using ExecutionResult = EventStreamManager.Infrastructure.Entities.ExecutionResult;

namespace EventStreamManager.EventProcessor.Executors;

/// <summary>
/// 脚本执行器 - 执行JS处理器
/// </summary>
public class ScriptExecutor : IScriptExecutor
{
    private readonly IJavaScriptExecutionService _jsService;
    private readonly ILogger<ScriptExecutor> _logger;
    private readonly IEventDataBuilderService _eventDataBuilderService;
    public ScriptExecutor(
        IJavaScriptExecutionService jsService,
        ILogger<ScriptExecutor> logger, 
        IEventDataBuilderService eventDataBuilderService)
    {
        _jsService = jsService;
        _logger = logger;
        _eventDataBuilderService = eventDataBuilderService;
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
                result.Success = false;
                result.NeedToSend = false;
                result.Reason = "处理器未配置或已禁用";
                return result;
            }

            if (string.IsNullOrEmpty(context.ProcessorConfig.SqlTemplate))
            {
                result.Success = false;
                result.NeedToSend = false;
                result.Reason = "未设置查询语句";
                return result;
            }


            var jsData = await _eventDataBuilderService.BuildEnhancedDataAsync(
                context.DatabaseType,
                context.Event,
                new JsProcessor()
                {
                    Id = context.ProcessorId,
                    Name = context.ProcessorName,
                    SqlTemplate = context.ProcessorConfig.SqlTemplate,
                });
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
}
