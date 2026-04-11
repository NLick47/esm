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

    private Task? _stateSaveTask;

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
        await _stateManager.UpdateStartTimeAsync(DateTime.Now);
        _logger.LogInformation("========== 事件处理器服务启动 ==========");

        // 启动后台刷新循环
        _processorManager.StartBackgroundRefresh(_refreshInterval, stoppingToken);

        // 启动状态保存循环
        _stateSaveTask = StateSaveLoopAsync(stoppingToken);

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
            try
            {
                await _stateManager.SaveStateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "自动保存服务状态失败");
            }
        }
    }

    private async Task ShutdownAsync()
    {
        _logger.LogInformation("========== 事件处理器服务停止 ==========");
        await _processorManager.StopAllAsync();

        // 等待状态保存循环完成，避免与最后的 SaveStateAsync 并发写文件
        if (_stateSaveTask != null)
        {
            try
            {
                await _stateSaveTask.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("等待状态保存循环完成超时");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "等待状态保存循环完成时发生异常");
            }
        }

        await _stateManager.SaveStateAsync();
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventProcessorService 正在停止...");
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
