using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Processors;

/// <summary>
/// 数据库类型处理器
/// </summary>
public class DatabaseTypeProcessor : IDisposable  
{
    private readonly string _databaseType;
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventListenerConfigService _configService;
    private readonly ILogger<DatabaseTypeProcessor> _logger;

    
    private readonly ProcessorStatus _status;
    private readonly object _lifecycleLock = new();
    private CancellationTokenSource? _cts;
    private Task? _processingTask;
    private int _consecutiveErrors;

    public string DatabaseType => _databaseType;
    public bool IsRunning => _status.IsRunning;

    public DatabaseTypeProcessor(
        string databaseType,
        IServiceProvider serviceProvider, 
        IEventListenerConfigService configService,
        ILogger<DatabaseTypeProcessor> logger)
    {
        _databaseType = databaseType;
        _serviceProvider = serviceProvider;
        _configService = configService;
        _logger = logger;
        _status = new ProcessorStatus { DatabaseType = databaseType };
    }


    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return Task.CompletedTask;
        }

        lock (_lifecycleLock)
        {
            if (_status.IsRunning) return Task.CompletedTask;

            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _status.IsRunning = true;
            _consecutiveErrors = 0;
            _logger.LogInformation("[{DatabaseType}] 处理器启动", _databaseType);

            _processingTask = ProcessLoopAsync(_cts.Token);
            return Task.CompletedTask;
        }
    }

    public async Task StopAsync()
    {
        Task? taskToWait;
        lock (_lifecycleLock)
        {
            if (!_status.IsRunning) return;

            _status.IsRunning = false;
            _cts?.Cancel();
            taskToWait = _processingTask;
            _processingTask = null;
        }

        if (taskToWait != null)
        {
            try
            {
                await taskToWait.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("[{DatabaseType}] 处理器停止超时", _databaseType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{DatabaseType}] 处理器停止时，后台任务发生异常", _databaseType);
            }
        }

        lock (_lifecycleLock)
        {
            _cts?.Dispose();
            _cts = null;
        }
        _logger.LogInformation("[{DatabaseType}] 处理器停止", _databaseType);
    }

    public ProcessorStatus GetStatus() => new()
    {
        DatabaseType = _status.DatabaseType,
        IsRunning = _status.IsRunning,
        IsEnabled = _status.IsEnabled,
        LastScanTime = _status.LastScanTime,
        LastProcessedEventId = _status.LastProcessedEventId,
        TotalProcessedCount = _status.TotalProcessedCount,
        SuccessCount = _status.SuccessCount,
        FailedCount = _status.FailedCount,
        CurrentBatchCount = _status.CurrentBatchCount,
        LastError = _status.LastError,
        LastErrorTime = _status.LastErrorTime
    };

    /// <summary>
    /// 处理循环
    /// </summary>
     private async Task ProcessLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _status.IsRunning)
        {
            try
            {
                var config = await _configService.GetConfigByTypeAsync(_databaseType);
                if (config is not { Enabled: true })
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
                    continue;
                }

                await using var scope = _serviceProvider.CreateAsyncScope();
                
                var scanner = scope.ServiceProvider.GetRequiredService<IEventScanner>();
                var executor = scope.ServiceProvider.GetRequiredService<IScriptExecutor>();
                var recorder = scope.ServiceProvider.GetRequiredService<IHandleRecorder>();
                var sender = scope.ServiceProvider.GetRequiredService<IInterfaceSender>();
                var processorService = scope.ServiceProvider.GetRequiredService<IProcessorService>();
                var interfaceService = scope.ServiceProvider.GetRequiredService<IInterfaceConfigService>();

                var eventCodes = await GetEventCodesAsync(processorService);
                var processorIds = await GetProcessorIdsAsync(processorService);
                
                if (processorIds.Count == 0)
                {
                    _logger.LogInformation("[{DatabaseType}] 没有启用的处理器，跳过本次扫描", _databaseType);
                    await Task.Delay(TimeSpan.FromSeconds(config.ScanFrequency), cancellationToken);
                    continue;
                }
                
                _status.LastScanTime = DateTime.Now;
                var events = await scanner.ScanAsync(_databaseType, config, eventCodes, processorIds);
                _status.CurrentBatchCount = events.Count;

                foreach (var eventData in events)
                {
                    if (cancellationToken.IsCancellationRequested) break;
                    await ProcessEventAsync(eventData, processorService, interfaceService, 
                        executor, recorder, sender, config.MaxRetryCount);
                    _status.LastProcessedEventId = eventData.Id;
                }

                _consecutiveErrors = 0;
                await Task.Delay(TimeSpan.FromSeconds(config.ScanFrequency), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{DatabaseType}] 处理循环异常", _databaseType);
                _status.LastError = ex.Message;
                _status.LastErrorTime = DateTime.Now;
                _consecutiveErrors++;

                var delay = TimeSpan.FromSeconds(Math.Min(10 * _consecutiveErrors, 300));
                _logger.LogWarning("[{DatabaseType}] 连续异常次数: {Count}，退避延迟: {Delay}s",
                    _databaseType, _consecutiveErrors, delay.TotalSeconds);
                
                try
                {
                    await Task.Delay(delay, cancellationToken);
                }
                catch (OperationCanceledException) { break; }
            }
        }
    }
    
    /// <summary>
    /// 获取当前数据库类型所有处理器的事件码集合
    /// </summary>
    private async Task<List<string>?> GetEventCodesAsync(IProcessorService processorService)
    {
        var all = await processorService.GetAllAsync();

        return all
            .Where(p => p.Enabled && (p.DatabaseTypes.Contains(_databaseType) || p.DatabaseTypes.Count == 0))
            .SelectMany(p => p.EventCodes)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 获取当前数据库类型所有启用的处理器ID集合
    /// </summary>
    private async Task<List<string>> GetProcessorIdsAsync(IProcessorService processorService)
    {
        var all = await processorService.GetAllAsync();

        return all
            .Where(p => p.Enabled && (p.DatabaseTypes.Contains(_databaseType) || p.DatabaseTypes.Count == 0))
            .Select(p => p.Id)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 处理单个事件 - 每个处理器单独创建处理记录
    /// </summary>
    private async Task ProcessEventAsync(
        Event eventData,
        IProcessorService processorService,
        IInterfaceConfigService interfaceService,
        IScriptExecutor executor,
        IHandleRecorder recorder,
        IInterfaceSender sender,
        int maxRetryCount)
    {
        try
        {
            var processors = await GetMatchingProcessorsAsync(eventData, processorService);
            if (processors.Count == 0) return;

            // 遍历每个处理器，单独处理并记录
            foreach (var processor in processors)
            {
                await ProcessSingleProcessorAsync(eventData, processor, interfaceService, executor, recorder, sender, maxRetryCount);
            }

            _status.TotalProcessedCount += processors.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 处理事件异常: EventId={EventId}",
                _databaseType, eventData.Id);
            _status.LastError = ex.Message;
            _status.LastErrorTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 处理单个处理器 - 创建独立的处理记录
    /// </summary>
    private async Task ProcessSingleProcessorAsync(
        Event eventData,
        JsProcessor processor,
        IInterfaceConfigService interfaceService,
        IScriptExecutor executor,
        IHandleRecorder recorder,
        IInterfaceSender sender,
        int maxRetryCount)
    {
        EventHandle? handle = null;
        
        try
        {
            // 获取或创建该处理器的处理记录
            handle = await recorder.GetOrCreateAsync(
                _databaseType,
                eventData.Id,
                processor.Id,
                processor.Name);

            // 如果记录创建失败或已完成，跳过
            if (handle.IsFinished)
            {
                _logger.LogDebug("[{DatabaseType}] 处理器 {ProcessorName} 已完成事件 {EventId}，跳过",
                    _databaseType, processor.Name, eventData.Id);
                return;
            }

            // 执行脚本
            var result = await ExecuteProcessorAsync(eventData, processor, executor);

            // 如果需要发送，调用接口
            if (result.NeedToSend)
            {
                await SendResultAsync(processor.Id, result, interfaceService, sender);
            }

            // 记录日志
            var log = await recorder.LogAsync(_databaseType, handle, result);

            // 判断整体处理是否成功
            bool isSuccess = DetermineOverallSuccess(result);
            
            // 根据整体成功状态更新处理记录
            if (isSuccess)
            {
                await recorder.MarkFinishedAsync(_databaseType, handle.Id, HandleStatus.Success, log.Id);
                _status.SuccessCount++;
                _logger.LogInformation(
                    "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId}: 成功, 耗时 {Time}ms",
                    _databaseType, processor.Name, eventData.Id, result.ExecutionTimeMs);
            }
            else
            {
                // 判断是否超过最大重试次数（MaxRetryCount = 0 表示无限重试）
                bool retryExhausted = maxRetryCount > 0 && handle.HandleTimes + 1 >= maxRetryCount;
                
                if (retryExhausted)
                {
                    await recorder.MarkRetryExhaustedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
                    _status.FailedCount++;
                    _logger.LogError(
                        "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId}: 失败且重试次数耗尽({Times}/{Max}), 标记为死信, 错误: {Error}",
                        _databaseType, processor.Name, eventData.Id, handle.HandleTimes, maxRetryCount, result.ErrorMessage ?? "未知错误");
                }
                else
                {
                    await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
                    _status.FailedCount++;
                    _logger.LogWarning(
                        "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId}: 失败, 错误: {Error}, 耗时 {Time}ms",
                        _databaseType, processor.Name, eventData.Id, result.ErrorMessage ?? "未知错误", result.ExecutionTimeMs);
                }
            }
        }
        catch (Exception ex)
        {
            // 处理过程中的未捕获异常
            _logger.LogError(ex, "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId} 时发生未处理异常",
                _databaseType, processor.Name, eventData.Id);

            // 如果有处理记录，标记为失败
            if (handle != null)
            {
                try
                {
                    var errorResult = new ExecutionResult
                    {
                        Success = false,
                        ErrorMessage = $"未处理异常: {ex.Message}",
                        ExecutionTimeMs = 0
                    };
                    
                    var log = await recorder.LogAsync(_databaseType, handle, errorResult);
                    
                    // 判断是否超过最大重试次数
                    bool retryExhausted = maxRetryCount > 0 && handle.HandleTimes + 1 >= maxRetryCount;
                    if (retryExhausted)
                    {
                        await recorder.MarkRetryExhaustedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
                    }
                    else
                    {
                        await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
                    }
                }
                catch (Exception recordEx)
                {
                    _logger.LogError(recordEx, "[{DatabaseType}] 记录处理失败状态时发生异常", _databaseType);
                }
            }
            
            _status.FailedCount++;
            _status.LastError = ex.Message;
            _status.LastErrorTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 判断整体处理是否成功
    /// </summary>
    private bool DetermineOverallSuccess(ExecutionResult result)
    {
        // 脚本执行失败
        if (!result.Success)
        {
            return false;
        }

        // 需要发送但发送失败
        if (result.NeedToSend && (result.SendResult == null || !result.SendResult.Success))
        {
            return false;
        }

        // 脚本执行成功，且如果发送则发送成功
        return true;
    }

    private async Task<List<JsProcessor>> GetMatchingProcessorsAsync(Event eventData, IProcessorService processorService)
    {
        var all = await processorService.GetAllAsync();
        var ids = all
            .Where(p => p.Enabled)
            .Where(p => p.DatabaseTypes.Contains(_databaseType) || p.DatabaseTypes.Count == 0)
            .Where(p => p.EventCodes.Count == 0 || p.EventCodes.Contains(eventData.EventCode))
            .Select(p => p.Id)
            .ToList();

        var processors = new List<JsProcessor>();
        foreach (var id in ids)
        {
            var processor = await processorService.GetByIdAsync(id);
            if (processor != null)
            {
                processors.Add(processor);
            }
        }

        return processors;
    }

    private async Task<ExecutionResult> ExecuteProcessorAsync(
        Event eventData, JsProcessor processor, IScriptExecutor executor)
    {
        try
        {
            var context = new ScriptContext
            {
                ProcessorId = processor.Id,
                ProcessorName = processor.Name,
                DatabaseType = _databaseType,
                Event = eventData,
                ProcessorConfig = processor
            };
            return await executor.ExecuteAsync(context);
        }
        catch (Exception ex)
        {
            // 执行脚本时的异常
            _logger.LogError(ex, "[{DatabaseType}] 执行处理器 {ProcessorName} 脚本时发生异常",
                _databaseType, processor.Name);
            
            return new ExecutionResult
            {
                Success = false,
                ErrorMessage = $"脚本执行异常: {ex.Message}",
                ExecutionTimeMs = 0
            };
        }
    }

    private async Task SendResultAsync(
        string processorId, 
        ExecutionResult result,
        IInterfaceConfigService interfaceService,
        IInterfaceSender sender)
    {
        try
        {
            var config = await interfaceService.GetConfigByProcessorIdAsync(processorId);

            if (config == null)
            {
                result.ErrorMessage = "未找到匹配的接口配置";
                result.SendResult = new SendResult { Success = false, ErrorMessage = result.ErrorMessage };
                return;
            }
            
            if (!config.Enabled)
            {
                result.ErrorMessage = "接口配置未启用";
                result.SendResult = new SendResult { Success = false, ErrorMessage = result.ErrorMessage };
                return;
            }

            var data = string.IsNullOrWhiteSpace(config.RequestTemplate)
                ? result.RequestInfo ?? "{}"
                : config.RequestTemplate.Replace("${data}", result.RequestInfo ?? "{}");

            result.SendResult = await sender.SendWithRetryAsync(
                _databaseType, config, data, config.RetryCount, config.RetryInterval * 1000);
            
            // 如果发送失败，更新整体结果状态
            if (result.SendResult is { Success: false })
            {
                result.ErrorMessage = result.SendResult.ErrorMessage ?? "接口发送失败";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 发送接口请求时发生异常, ProcessorId={ProcessorId}", 
                _databaseType, processorId);
            
            result.ErrorMessage = $"接口发送异常: {ex.Message}";
            result.SendResult = new SendResult 
            { 
                Success = false, 
                ErrorMessage = result.ErrorMessage 
            };
        }
    }

    public void Dispose()
    {
        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
        }
        
        if (_processingTask is { IsCompleted: false })
        {
            try 
            { 
                _processingTask.Wait(TimeSpan.FromSeconds(5)); 
            }
            catch { /* ignore */ }
        }
    }
}
