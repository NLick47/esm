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
    private readonly object _refreshLock = new();
    private readonly SemaphoreSlim _processorOpsLock = new(1, 1);
    private Task? _refreshTask;
    private CancellationTokenSource? _linkedRefreshCts;
    private bool _disposed;

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
        await _processorOpsLock.WaitAsync(ct);
        try
        {
            await StartProcessorCoreAsync(dbType, ct);
        }
        finally
        {
            _processorOpsLock.Release();
        }
    }

    public async Task StopProcessorAsync(string dbType)
    {
        await _processorOpsLock.WaitAsync();
        try
        {
            await StopProcessorCoreAsync(dbType);
        }
        finally
        {
            _processorOpsLock.Release();
        }
    }

    public async Task StopAllAsync()
    {
        await _processorOpsLock.WaitAsync();
        try
        {
            var keys = _processors.Keys.ToList();
            foreach (var dbType in keys)
            {
                await StopProcessorCoreAsync(dbType);
            }
        }
        finally
        {
            _processorOpsLock.Release();
        }
    }

    public async Task RefreshConfigurationAsync(CancellationToken ct)
    {
        await _processorOpsLock.WaitAsync(ct);
        try
        {
            if (_disposed || !_stateManager.IsEnabled) return;

            var types = await _factory.GetConfiguredTypesAsync();
            var configs = await _configService.GetAllConfigsAsync();

            // 新增处理器
            var newTypes = types.Except(_processors.Keys);
            foreach (var dbType in newTypes)
            {
                _logger.LogInformation("新增处理器: {DatabaseType}", dbType);
                try
                {
                    await StartProcessorCoreAsync(dbType, ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "刷新配置时启动处理器失败: {DatabaseType}", dbType);
                }
            }

            // 移除已删除的
            var removedTypes = _processors.Keys.Except(types).ToList();
            foreach (var dbType in removedTypes)
            {
                _logger.LogInformation("移除处理器: {DatabaseType}", dbType);
                await StopProcessorCoreAsync(dbType);
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
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新处理器配置失败");
        }
        finally
        {
            _processorOpsLock.Release();
        }
    }

    public void StartBackgroundRefresh(TimeSpan interval, CancellationToken externalCt = default)
    {
        lock (_refreshLock)
        {
            if (_refreshTask is { IsCompleted: false })
            {
                _logger.LogWarning("后台配置刷新循环已经在运行，忽略重复启动请求");
                return;
            }

            _linkedRefreshCts?.Dispose();
            _linkedRefreshCts = CancellationTokenSource.CreateLinkedTokenSource(_refreshCts.Token, externalCt);
            _refreshTask = BackgroundRefreshLoopAsync(interval, _linkedRefreshCts.Token);
        }
    }

    private async Task BackgroundRefreshLoopAsync(TimeSpan interval, CancellationToken ct)
    {
        using var timer = new PeriodicTimer(interval);
        
        while (await timer.WaitForNextTickAsync(ct))
        {
            try
            {
                await RefreshConfigurationAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "后台刷新配置时发生异常");
            }
        }
    }

    public async Task TriggerScanAsync(string dbType, CancellationToken ct = default)
    {
        if (!_stateManager.IsEnabled)
        {
            _logger.LogWarning("服务处于禁用状态，无法触发扫描");
            return;
        }

        await _processorOpsLock.WaitAsync(ct);
        try
        {
            if (_processors.TryGetValue(dbType, out var processor))
            {
                try
                {
                    await processor.StopAsync();
                    await processor.StartAsync(ct);
                    _logger.LogInformation("手动触发扫描完成: {DatabaseType}", dbType);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "手动触发扫描失败: {DatabaseType}", dbType);
                }
            }
        }
        finally
        {
            _processorOpsLock.Release();
        }
    }

    public IReadOnlyList<ProcessorStatus> GetAllStatus() 
        => _processors.Values.Select(p => p.GetStatus()).ToList();

    public ProcessorStatus? GetStatus(string databaseType) 
        => _processors.TryGetValue(databaseType, out var p) ? p.GetStatus() : null;

    private async Task StartProcessorCoreAsync(string dbType, CancellationToken ct)
    {
        if (_disposed) return;
        if (_processors.ContainsKey(dbType)) return;

        var processor = _factory.Create(dbType);
        
        if (_processors.TryAdd(dbType, processor))
        {
            try
            {
                await processor.StartAsync(ct);
                _logger.LogInformation("处理器已启动: {DatabaseType}", dbType);
            }
            catch
            {
                _processors.TryRemove(dbType, out _);
                throw;
            }
        }
        else
        {
            // 如果 TryAdd 失败，释放创建的 processor 实例（如果实现了 IDisposable）
            if (processor is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    private async Task StopProcessorCoreAsync(string dbType)
    {
        if (_processors.TryRemove(dbType, out var processor))
        {
            try
            {
                await processor.StopAsync();
                _logger.LogInformation("处理器已停止: {DatabaseType}", dbType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止处理器时发生异常: {DatabaseType}", dbType);
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            _refreshCts.Cancel();
        }
        catch (ObjectDisposedException) { }

        lock (_refreshLock)
        {
            if (_refreshTask != null && !_refreshTask.IsCompleted)
            {
                try
                {
                    if (!_refreshTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        _logger.LogWarning("ProcessorManagerService 后台刷新任务停止超时");
                    }
                }
                catch (AggregateException aex)
                {
                    _logger.LogError(aex, "ProcessorManagerService 后台刷新任务异常");
                }
                catch (ObjectDisposedException) { }
            }
        }

        try
        {
            _refreshCts.Dispose();
        }
        catch (ObjectDisposedException) { }

        try
        {
            _linkedRefreshCts?.Dispose();
        }
        catch (ObjectDisposedException) { }

        _processorOpsLock.Dispose();
    }
}
