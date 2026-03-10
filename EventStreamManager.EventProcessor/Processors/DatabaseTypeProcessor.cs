using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Executors;
using EventStreamManager.EventProcessor.Recorders;
using EventStreamManager.EventProcessor.Scanners;
using EventStreamManager.EventProcessor.Senders;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Processors;

/// <summary>
/// 数据库类型处理器
/// </summary>
public class DatabaseTypeProcessor
{
    private readonly string _databaseType;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IEventListenerConfigService _configService;
    private readonly ILogger<DatabaseTypeProcessor> _logger;

    private readonly ProcessorStatus _status;
    private CancellationToken _cancellationToken;

    public string DatabaseType => _databaseType;
    public bool IsRunning => _status.IsRunning;

    public DatabaseTypeProcessor(
        string databaseType,
        IServiceScopeFactory scopeFactory,
        ILoggerFactory loggerFactory,
        IEventListenerConfigService configService,
        ILogger<DatabaseTypeProcessor> logger)
    {
        _databaseType = databaseType;
        _scopeFactory = scopeFactory;
        _loggerFactory = loggerFactory;
        _configService = configService;
        _logger = logger;
        _status = new ProcessorStatus { DatabaseType = databaseType };
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_status.IsRunning) return;

        _cancellationToken = cancellationToken;
        _status.IsRunning = true;
        _logger.LogInformation("[{DatabaseType}] 处理器启动", _databaseType);

        _ = ProcessLoopAsync();
    }

    public async Task StopAsync()
    {
        _status.IsRunning = false;
        _logger.LogInformation("[{DatabaseType}] 处理器停止", _databaseType);
        await Task.CompletedTask;
    }

    public ProcessorStatus GetStatus() => _status;

    /// <summary>
    /// 处理循环
    /// </summary>
    private async Task ProcessLoopAsync()
    {
        while (!_cancellationToken.IsCancellationRequested && _status.IsRunning)
        {
            try
            {
                var config = await _configService.GetConfigByTypeAsync(_databaseType);
                if (config == null || !config.Enabled)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), _cancellationToken);
                    continue;
                }

                // 在每次循环中创建 scope 来获取 Scoped 服务
                using var scope = _scopeFactory.CreateScope();
                var services = scope.ServiceProvider;

                var db = services.GetRequiredService<ISqlSugarContext>();
                var processorService = services.GetRequiredService<IProcessorService>();
                var interfaceService = services.GetRequiredService<IInterfaceConfigService>();
                var jsService = services.GetRequiredService<Infrastructure.Services.IJavaScriptExecutionService>();
                var httpFactory = services.GetRequiredService<IHttpClientFactory>();

                // 创建本次循环使用的组件实例
                var scanner = CreateScanner(db);
                var executor = CreateExecutor(jsService, db);
                var recorder = CreateRecorder(db);
                var sender = CreateSender(httpFactory);

                // 获取当前数据库类型所有处理器的事件码
                var eventCodes = await GetEventCodesAsync(processorService);

                _status.LastScanTime = DateTime.Now;
                var events = await scanner.ScanAsync(_databaseType, config, eventCodes);
                _status.CurrentBatchCount = events.Count;

                foreach (var eventData in events)
                {
                    if (_cancellationToken.IsCancellationRequested) break;
                    await ProcessEventAsync(eventData, processorService, interfaceService, executor, recorder, sender);
                    _status.LastProcessedEventId = eventData.Id;
                }
                

                await Task.Delay(TimeSpan.FromSeconds(config.ScanFrequency), _cancellationToken);
            }
            catch (TaskCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{DatabaseType}] 处理循环异常", _databaseType);
                _status.LastError = ex.Message;
                _status.LastErrorTime = DateTime.Now;
                await Task.Delay(TimeSpan.FromSeconds(10), _cancellationToken);
            }
        }
    }

    private EventScanner CreateScanner(ISqlSugarContext db)
    {
        return new EventScanner(db, _loggerFactory.CreateLogger<EventScanner>());
    }

    private ScriptExecutor CreateExecutor(Infrastructure.Services.IJavaScriptExecutionService jsService, ISqlSugarContext db)
    {
        return new ScriptExecutor(jsService, db, _loggerFactory.CreateLogger<ScriptExecutor>());
    }

    private HandleRecorder CreateRecorder(ISqlSugarContext db)
    {
        return new HandleRecorder(db, _loggerFactory.CreateLogger<HandleRecorder>());
    }

    private InterfaceSender CreateSender(IHttpClientFactory httpFactory)
    {
        return new InterfaceSender(httpFactory, _loggerFactory.CreateLogger<InterfaceSender>());
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
    /// 处理单个事件 - 每个处理器单独创建处理记录
    /// </summary>
    private async Task ProcessEventAsync(
        Event eventData,
        IProcessorService processorService,
        IInterfaceConfigService interfaceService,
        ScriptExecutor executor,
        HandleRecorder recorder,
        InterfaceSender sender)
    {
        try
        {
            var processors = await GetMatchingProcessorsAsync(eventData, processorService);
            if (processors.Count == 0) return;

            // 遍历每个处理器，单独处理并记录
            foreach (var processor in processors)
            {
                await ProcessSingleProcessorAsync(eventData, processor, interfaceService, executor, recorder, sender);
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
        JSProcessor processor,
        IInterfaceConfigService interfaceService,
        ScriptExecutor executor,
        HandleRecorder recorder,
        InterfaceSender sender)
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

            // 如果已完成，跳过
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
                await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
                _status.FailedCount++;
                _logger.LogWarning(
                    "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId}: 失败, 错误: {Error}, 耗时 {Time}ms",
                    _databaseType, processor.Name, eventData.Id, result.ErrorMessage ?? "未知错误", result.ExecutionTimeMs);
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
                    await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
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

    private async Task<List<JSProcessor>> GetMatchingProcessorsAsync(Event eventData, IProcessorService processorService)
    {
        var all = await processorService.GetAllAsync();
        return all.Where(p => p.Enabled)
                  .Where(p => p.DatabaseTypes.Contains(_databaseType) || p.DatabaseTypes.Count == 0)
                  .Where(p => p.EventCodes.Count == 0 || p.EventCodes.Contains(eventData.EventCode))
                  .ToList();
    }

    private async Task<ExecutionResult> ExecuteProcessorAsync(
        Event eventData, JSProcessor processor, ScriptExecutor executor)
    {
        try
        {
            Dictionary<string, object>? extendedData = null;
            if (!string.IsNullOrWhiteSpace(processor.SqlTemplate))
            {
                extendedData = await executor.QueryDataAsync(_databaseType, processor.SqlTemplate, eventData);
            }

            var context = new ScriptContext
            {
                ProcessorId = processor.Id,
                ProcessorName = processor.Name,
                DatabaseType = _databaseType,
                Event = eventData,
                QueryResult = extendedData,
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
        InterfaceSender sender)
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
}