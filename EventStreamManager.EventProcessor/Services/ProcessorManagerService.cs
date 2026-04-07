using System.Collections.Concurrent;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Processors;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Services;

public sealed class ProcessorManagerService : IProcessorManagerService, IDisposable
{
    private readonly ProcessorFactory _factory;
    private readonly IEventListenerConfigService _configService;
    private readonly IStateManagerService _stateManager;
    private readonly ILogger<ProcessorManagerService> _logger;

    private readonly ConcurrentDictionary<string, DatabaseTypeProcessor> _processors = new();
    private readonly CancellationTokenSource _refreshCts = new();
    private Task? _refreshTask;

    public ProcessorManagerService(
        ProcessorFactory factory,
        IEventListenerConfigService configService,
        IStateManagerService stateManager,
        ILogger<ProcessorManagerService> logger)
    {
        _factory = factory;
        _configService = configService;
        _stateManager = stateManager;
        _logger = logger;
    }

    public bool IsRunning => _processors.Values.Any(p => p.IsRunning);
    public int ProcessorCount => _processors.Count;
    public int ActiveProcessorCount => _processors.Values.Count(p => p.IsRunning);

    public async Task InitializeAsync(CancellationToken ct)
    {
        if (!_stateManager.IsEnabled)
        {
            _logger.LogInformation("服务处于禁用状态，跳过初始化处理器");
            return;
        }

        var types = await _factory.GetConfiguredTypesAsync();
        await Parallel.ForEachAsync(types, ct, async (type, token) =>
        {
            await StartProcessorAsync(type, token);
        });
    }

    public async Task StartProcessorAsync(string dbType, CancellationToken ct)
    {
        try
        {
            var processor = _factory.Create(dbType);
            
            if (_processors.TryAdd(dbType, processor))
            {
                await processor.StartAsync(ct);
                _logger.LogInformation("处理器已启动: {DatabaseType}", dbType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动处理器失败: {DatabaseType}", dbType);
            _processors.TryRemove(dbType, out _);
        }
    }

    public async Task StopProcessorAsync(string dbType)
    {
        if (_processors.TryRemove(dbType, out var processor))
        {
            await processor.StopAsync();
            _logger.LogInformation("处理器已停止: {DatabaseType}", dbType);
        }
    }

    public async Task StopAllAsync()
    {
        var tasks = _processors.Values.Select(p => p.StopAsync()).ToList();
        await Task.WhenAll(tasks);
        _processors.Clear();
    }

    public async Task RefreshConfigurationAsync(CancellationToken ct)
    {
        if (!_stateManager.IsEnabled) return;

        try
        {
            var types = await _factory.GetConfiguredTypesAsync();
            var configs = await _configService.GetAllConfigsAsync();

            // 新增处理器
            var newTypes = types.Except(_processors.Keys);
            foreach (var dbType in newTypes)
            {
                _logger.LogInformation("新增处理器: {DatabaseType}", dbType);
                await StartProcessorAsync(dbType, ct);
            }

            // 移除已删除的
            var removedTypes = _processors.Keys.Except(types).ToList();
            foreach (var dbType in removedTypes)
            {
                _logger.LogInformation("移除处理器: {DatabaseType}", dbType);
                await StopProcessorAsync(dbType);
            }

            // 状态变更
            foreach (var (dbType, processor) in _processors)
            {
                if (!configs.Databases.TryGetValue(dbType, out var config))
                    continue;

                var shouldRun = config.Enabled && _stateManager.IsEnabled;
                
                if (shouldRun && !processor.IsRunning)
                {
                    _logger.LogInformation("启用处理器: {DatabaseType}", dbType);
                    await processor.StartAsync(ct);
                }
                else if (!shouldRun && processor.IsRunning)
                {
                    _logger.LogInformation("禁用处理器: {DatabaseType}", dbType);
                    await processor.StopAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新处理器配置失败");
        }
    }

    public void StartBackgroundRefresh(TimeSpan interval)
    {
        _refreshTask = BackgroundRefreshLoopAsync(interval, _refreshCts.Token);
    }

    private async Task BackgroundRefreshLoopAsync(TimeSpan interval, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(interval);
        
        while (await timer.WaitForNextTickAsync(ct))
        {
            await RefreshConfigurationAsync(ct);
        }
    }

    public async Task TriggerScanAsync(string dbType)
    {
        if (!_stateManager.IsEnabled)
        {
            _logger.LogWarning("服务处于禁用状态，无法触发扫描");
            return;
        }

        if (_processors.TryGetValue(dbType, out var processor))
        {
            await processor.StopAsync();
            await processor.StartAsync(CancellationToken.None);
            _logger.LogInformation("手动触发扫描完成: {DatabaseType}", dbType);
        }
    }

    public IReadOnlyList<ProcessorStatus> GetAllStatus() 
        => _processors.Values.Select(p => p.GetStatus()).ToList();

    public ProcessorStatus? GetStatus(string databaseType) 
        => _processors.TryGetValue(databaseType, out var p) ? p.GetStatus() : null;

    public void Dispose()
    {
        _refreshCts.Cancel();
        _refreshTask?.Wait(TimeSpan.FromSeconds(5));
        _refreshCts.Dispose();
    }
}