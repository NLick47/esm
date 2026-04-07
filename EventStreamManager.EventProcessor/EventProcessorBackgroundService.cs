using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor;

public sealed class EventProcessorService : BackgroundService
{
    private readonly IStateManagerService _stateManager;
    private readonly IProcessorManagerService _processorManager;
    private readonly ILogger<EventProcessorService> _logger;

    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(30);
    private readonly TimeSpan _stateSaveInterval = TimeSpan.FromMinutes(1);

    public EventProcessorService(
        IStateManagerService stateManager,
        IProcessorManagerService processorManager,
        ILogger<EventProcessorService> logger)
    {
        _stateManager = stateManager;
        _processorManager = processorManager;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventProcessorService 正在启动...");
        await _stateManager.LoadStateAsync();
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stateManager.UpdateStartTime(DateTime.Now);
        _logger.LogInformation("========== 事件处理器服务启动 ==========");

        // 启动后台刷新循环
        _processorManager.StartBackgroundRefresh(_refreshInterval);

        // 启动状态保存循环
        _ = StateSaveLoopAsync(stoppingToken);

        try
        {
            // 初始化处理器
            await _processorManager.InitializeAsync(stoppingToken);

            // 等待停止信号
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("事件处理器服务收到停止信号");
        }
        finally
        {
            await ShutdownAsync();
        }
    }

    private async Task StateSaveLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(_stateSaveInterval);
        
        while (await timer.WaitForNextTickAsync(ct))
        {
            await _stateManager.SaveStateAsync();
        }
    }

    private async Task ShutdownAsync()
    {
        _logger.LogInformation("========== 事件处理器服务停止 ==========");
        await _processorManager.StopAllAsync();
        await _stateManager.SaveStateAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventProcessorService 正在停止...");
        await _stateManager.SaveStateAsync();
        await base.StopAsync(cancellationToken);
    }

   
    public Task EnableAsync() => _stateManager.EnableAsync();
    public Task DisableAsync() => _stateManager.DisableAsync();
    public Task<bool> ToggleAsync() => _stateManager.ToggleAsync();
    public ServiceStatus GetStatus() => new()
    {
        IsEnabled = _stateManager.IsEnabled,
        IsRunning = _processorManager.IsRunning,
        StartTime = _stateManager.StartTime,
        RunningDuration = _stateManager.GetRunningDuration(),
        ProcessorCount = _processorManager.ProcessorCount,
        ActiveProcessorCount = _processorManager.ActiveProcessorCount,
        Processors = _processorManager.GetAllStatus()
    };
}