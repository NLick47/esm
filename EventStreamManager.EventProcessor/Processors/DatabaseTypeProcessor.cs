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
/// 使用 IServiceScopeFactory 在每次处理循环中创建 scope，解决 Singleton/Scoped 生命周期问题
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
                    await ProcessEventAsync(eventData, processorService, interfaceService, scanner, executor, recorder, sender);
                    _status.LastProcessedEventId = eventData.Id;
                }

                if (events.Count > 0)
                {
                    await scanner.UpdatePositionAsync(_databaseType, events.Max(e => e.Id));
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
        return new EventScanner(db, _configService, _loggerFactory.CreateLogger<EventScanner>());
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
    private async Task<List<string>> GetEventCodesAsync(IProcessorService processorService)
    {
        var all = await processorService.GetAllAsync();
        return all
            .Where(p => p.Enabled && (p.DatabaseTypes.Contains(_databaseType) || p.DatabaseTypes.Count == 0))
            .SelectMany(p => p.EventCodes)
            .Distinct()
            .ToList();
    }

    /// <summary>
    /// 处理单个事件
    /// </summary>
    private async Task ProcessEventAsync(
        Event eventData,
        IProcessorService processorService,
        IInterfaceConfigService interfaceService,
        EventScanner scanner,
        ScriptExecutor executor,
        HandleRecorder recorder,
        InterfaceSender sender)
    {
        try
        {
            var processors = await GetMatchingProcessorsAsync(eventData, processorService);
            if (processors.Count == 0) return;

            var handleType = string.Join(",", processors.Select(p => p.Name));
            var handle = await recorder.GetOrCreateAsync(_databaseType, eventData.Id, handleType);

            if (handle.IsFinished) return;

            var results = new List<ExecutionResult>();
            foreach (var processor in processors)
            {
                var result = await ExecuteProcessorAsync(eventData, processor, executor);
                results.Add(result);

                if (result.NeedToSend)
                {
                    await SendResultAsync(processor.Id, result, interfaceService, sender);
                }
            }

            var log = await recorder.LogAsync(_databaseType, handle, results);
            var allSuccess = results.All(r => r.Success && (r.SendResult?.Success ?? true));

            if (allSuccess)
            {
                await recorder.MarkFinishedAsync(_databaseType, handle.Id, HandleStatus.Success, log.Id);
            }
            else
            {
                await recorder.MarkFailedAsync(_databaseType, handle, HandleStatus.Fail, log.Id);
            }

            _status.TotalProcessedCount++;
            _status.SuccessCount += results.Count(r => r.Success);
            _status.FailedCount += results.Count(r => !r.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{DatabaseType}] 处理事件异常: EventId={EventId}",
                _databaseType, eventData.Id);
            _status.LastError = ex.Message;
            _status.LastErrorTime = DateTime.Now;
        }
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
