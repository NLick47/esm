using System.Diagnostics;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.JSFunction.Runtime;
using Microsoft.Extensions.Logging;
using ExecutionResult = EventStreamManager.Infrastructure.Entities.ExecutionResult;

namespace EventStreamManager.EventProcessor;

public class PipelineExecutor
{
    private readonly IProcessorService _processorService;
    private readonly IJavaScriptExecutionService _jsService;
    private readonly IEventDataBuilderService _eventDataBuilderService;
    private readonly ILogger<PipelineExecutor> _logger;

    public PipelineExecutor(
        IProcessorService processorService,
        IJavaScriptExecutionService jsService,
        IEventDataBuilderService eventDataBuilderService,
        ILogger<PipelineExecutor> logger)
    {
        _processorService = processorService;
        _jsService = jsService;
        _eventDataBuilderService = eventDataBuilderService;
        _logger = logger;
    }

    public async Task<ExecutionResult> ExecuteAsync(
        string databaseType,
        Event eventData,
        ProcessorPipeline pipeline,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var shared = new Dictionary<string, object?>();
        var stages = pipeline.Stages.OrderBy(s => s.SortOrder).ToList();
        ExecutionResult? finalResult = null;

        for (int i = 0; i < stages.Count; i++)
        {
            var stage = stages[i];
            var processor = await _processorService.GetByIdAsync(stage.ProcessorId);

            if (processor == null || !processor.Enabled)
            {
                if (stage.IsSender)
                {
                    stopwatch.Stop();
                    return new ExecutionResult
                    {
                        Success = false,
                        ErrorMessage = $"Sender Stage 引用的处理器不存在或已禁用: {stage.ProcessorName}",
                        ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                    };
                }
                continue;
            }

            var pipelineInfo = new PipelineStageInfo
            {
                StageIndex = i,
                StageCount = stages.Count,
                StageName = processor.Name,
                IsSender = stage.IsSender
            };

            EnhancedQueryData jsData;
            try
            {
                jsData = await _eventDataBuilderService.BuildEnhancedDataAsync(
                    databaseType, eventData, processor, shared, pipelineInfo, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{DatabaseType}] Pipeline Stage {Index}/{Count} SQL查询失败: {Name}",
                    databaseType, i + 1, stages.Count, processor.Name);

                var errorResult = new ExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Stage '{processor.Name}' SQL查询失败: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                if (!HandleStageFailure(stage, ref i, stages))
                    return errorResult;

                continue;
            }

            JSFunction.Runtime.ExecutionResult stageResult;
            try
            {
                stageResult = await _jsService.ExecuteProcessAsync(processor.Code, jsData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{DatabaseType}] Pipeline Stage {Index}/{Count} JS执行失败: {Name}",
                    databaseType, i + 1, stages.Count, processor.Name);

                var errorResult = new ExecutionResult
                {
                    Success = false,
                    ErrorMessage = $"Stage '{processor.Name}' JS执行失败: {ex.Message}",
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                if (!HandleStageFailure(stage, ref i, stages))
                    return errorResult;

                continue;
            }

            // 合并 sharedOutput
            if (stageResult.SharedOutput != null)
            {
                foreach (var kv in stageResult.SharedOutput)
                {
                    shared[kv.Key] = kv.Value;
                }
            }

            // 如果是 Sender Stage，保存最终结果
            if (stage.IsSender)
            {
                finalResult = new ExecutionResult
                {
                    Success = stageResult.Success,
                    NeedToSend = stageResult.NeedToSend,
                    RequestInfo = stageResult.RequestInfo,
                    Reason = stageResult.Reason,
                    ErrorMessage = stageResult.ErrorMessage,
                    ConsoleOutput = stageResult.ConsoleOutput,
                    ExecutionTimeMs = stageResult.ExecutionTimeMs
                };
            }

            // 处理 Stage 失败
            if (!stageResult.Success)
            {
                var errorResult = new ExecutionResult
                {
                    Success = false,
                    ErrorMessage = stageResult.ErrorMessage ?? $"Stage '{processor.Name}' 执行失败",
                    ConsoleOutput = stageResult.ConsoleOutput,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                if (!HandleStageFailure(stage, ref i, stages))
                    return errorResult;
            }
        }

        stopwatch.Stop();

        if (finalResult != null)
        {
            finalResult.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            return finalResult;
        }

        return new ExecutionResult
        {
            Success = false,
            ErrorMessage = "Pipeline 没有配置 Sender Stage",
            ExecutionTimeMs = stopwatch.ElapsedMilliseconds
        };
    }

    private static bool HandleStageFailure(PipelineStage stage,
        ref int currentIndex,
        List<PipelineStage> stages)
    {
        switch (stage.OnFailure)
        {
            case StageFailureAction.Stop:
                return false;

            case StageFailureAction.Continue:
                return true;

            case StageFailureAction.SkipToSender:
                var senderIndex = stages.FindIndex(currentIndex + 1, s => s.IsSender);
                if (senderIndex >= 0)
                {
                    currentIndex = senderIndex - 1;
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}
