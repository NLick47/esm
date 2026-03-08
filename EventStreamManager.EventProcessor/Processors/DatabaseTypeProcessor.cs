using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Executors;
using EventStreamManager.EventProcessor.Recorders;
using EventStreamManager.EventProcessor.Scanners;
using EventStreamManager.EventProcessor.Senders;
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
        //获取或创建该处理器的处理记录
        var handle = await recorder.GetOrCreateAsync(
            _databaseType, 
            eventData.Id, 
            processor.Id, 
            processor.Name);

        //如果已完成，跳过
        if (handle.IsFinished)
        {
            _logger.LogDebug("[{DatabaseType}] 处理器 {ProcessorName} 已完成事件 {EventId}，跳过",
                _databaseType, processor.Name, eventData.Id);
            return;
        }

        //执行脚本
        var result = await ExecuteProcessorAsync(eventData, processor, executor);

        //如果需要发送，调用接口
        if (result.NeedToSend)
        {
            await SendResultAsync(processor.Id, result, interfaceService, sender);
        }

        //记录日志
        var log = await recorder.LogAsync(_databaseType, handle, result);

        //更新处理记录状态
        var isSuccess = result.Success && (result.SendResult?.Success ?? true);
        if (isSuccess)
        {
            await recorder.MarkFinishedAsync(_databaseType, handle.Id, HandleStatus.Success, log.Id);
            _status.SuccessCount++;
        }
        else
        {
            await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
            _status.FailedCount++;
        }

        _logger.LogInformation(
            "[{DatabaseType}] 处理器 {ProcessorName} 处理事件 {EventId}: {Status}, 耗时 {Time}ms",
            _databaseType, processor.Name, eventData.Id,
            isSuccess ? "成功" : "失败",
            result.ExecutionTimeMs);
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

    private async Task SendResultAsync(
        string processorId, 
        ExecutionResult result,
        IInterfaceConfigService interfaceService,
        InterfaceSender sender)
    {
        var interfaces = await interfaceService.GetAllConfigsAsync();
        var config = interfaces.FirstOrDefault(i => i.Enabled && i.ProcessorIds.Contains(processorId));

        if (config == null)
        {
            result.ErrorMessage = "未找到匹配的接口配置";
            return;
        }

        var data = string.IsNullOrWhiteSpace(config.RequestTemplate)
            ? result.RequestInfo ?? "{}"
            : config.RequestTemplate.Replace("${data}", result.RequestInfo ?? "{}");

        result.SendResult = await sender.SendWithRetryAsync(
            _databaseType, config, data, config.RetryCount, config.RetryInterval * 1000);
    }
}
