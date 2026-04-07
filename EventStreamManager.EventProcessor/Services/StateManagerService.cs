using System.Threading.Channels;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Services;

public sealed class StateManagerService : IStateManagerService, IDisposable
{
    private readonly IDataService _dataService;
    private readonly ILogger<StateManagerService> _logger;
    private readonly string _stateFile;
    
    private readonly object _lock = new();
    private ServiceStateSnapshot _state;
    private readonly Channel<Func<ServiceStateSnapshot, ServiceStateSnapshot>> _stateUpdates;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task? _processingTask;

    public StateManagerService(
        IDataService dataService,
        ILogger<StateManagerService> logger,
        string stateFile = "service-state.json")
    {
        _dataService = dataService;
        _logger = logger;
        _stateFile = stateFile;
        _state = CreateDefaultState();
        
        _stateUpdates = Channel.CreateUnbounded<Func<ServiceStateSnapshot, ServiceStateSnapshot>>();
        _processingTask = ProcessStateUpdatesAsync(_cts.Token);
    }

    public bool IsEnabled 
    { 
        get { lock (_lock) return _state.IsEnabled; } 
    }

    public DateTime StartTime 
    { 
        get { lock (_lock) return _state.StartTime; } 
    }
    
    DateTime IStateManagerService.StartTime 
    { 
        get => StartTime;
        init => UpdateStartTime(value);  
    }

    public TimeSpan GetRunningDuration()
    {
        lock (_lock) return _state.GetRunningDuration();
    }

    public ServiceStateSnapshot GetSnapshot()
    {
        lock (_lock) return _state;
    }

    public async Task EnableAsync()
    {
        await UpdateStateAsync(current =>
        {
            if (current.IsEnabled)
            {
                _logger.LogWarning("服务已经处于启用状态");
                return null; // 无变化
            }

            var now = DateTime.Now;
            var pausedDuration = current.PauseTime.HasValue 
                ? now - current.PauseTime.Value 
                : TimeSpan.Zero;

            return current with
            {
                IsEnabled = true,
                PauseTime = null,
                TotalPausedDuration = current.TotalPausedDuration + pausedDuration,
                LastUpdated = now
            };
        });

        _logger.LogInformation("事件处理服务已启用");
        await SaveStateAsync();
    }

    public async Task DisableAsync()
    {
        await UpdateStateAsync(current =>
        {
            if (!current.IsEnabled)
            {
                _logger.LogWarning("服务已经处于禁用状态");
                return null;
            }

            return current with
            {
                IsEnabled = false,
                PauseTime = DateTime.Now,
                LastUpdated = DateTime.Now
            };
        });

        _logger.LogInformation("事件处理服务已禁用");
        await SaveStateAsync();
    }

    public async Task<bool> ToggleAsync()
    {
        if (IsEnabled)
        {
            await DisableAsync();
            return false;
        }

        await EnableAsync();
        return true;
    }

    public void UpdateStartTime(DateTime startTime)
    {
        UpdateState(current => current with { StartTime = startTime });
    }

    public async Task LoadStateAsync()
    {
        try
        {
            var states = await _dataService.ReadAsync<ServiceState>(_stateFile);
            var saved = states.FirstOrDefault();

            lock (_lock)
            {
                if (saved != null)
                {
                    _state = new ServiceStateSnapshot
                    {
                        IsEnabled = saved.IsEnabled,
                        StartTime = saved.StartTime,
                        PauseTime = saved.IsEnabled ? null : DateTime.Now,
                        TotalPausedDuration = TimeSpan.Zero,
                        LastUpdated = saved.LastUpdated
                    };

                    _logger.LogInformation("已加载服务状态: IsEnabled={IsEnabled}", _state.IsEnabled);
                }
                else
                {
                    _state = CreateDefaultState();
                    _logger.LogInformation("未找到历史状态，使用默认禁用状态");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载服务状态失败，使用默认状态");
            lock (_lock) _state = CreateDefaultState();
        }
    }

    public async Task SaveStateAsync()
    {
        try
        {
            ServiceStateSnapshot snapshot;
            lock (_lock) snapshot = _state with { LastUpdated = DateTime.Now };

            var toSave = new ServiceState
            {
                IsEnabled = snapshot.IsEnabled,
                StartTime = snapshot.StartTime,
                LastUpdated = snapshot.LastUpdated,
                Version = snapshot.Version
            };

            await _dataService.WriteAsync(_stateFile, new List<ServiceState> { toSave });
            _logger.LogDebug("服务状态已保存");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存服务状态失败");
        }
    }

    private ServiceStateSnapshot CreateDefaultState() => new()
    {
        IsEnabled = false,
        StartTime = DateTime.Now,
        PauseTime = DateTime.Now,
        TotalPausedDuration = TimeSpan.Zero,
        LastUpdated = DateTime.Now
    };

    private void UpdateState(Func<ServiceStateSnapshot, ServiceStateSnapshot?> updater)
    {
        lock (_lock)
        {
            var result = updater(_state);
            if (result != null) _state = result;
        }
    }

    private async Task UpdateStateAsync(Func<ServiceStateSnapshot, ServiceStateSnapshot?> updater)
    {
        var tcs = new TaskCompletionSource();
        
        await _stateUpdates.Writer.WriteAsync(current =>
        {
            var result = updater(current);
            tcs.SetResult();
            return result ?? current;
        });

        await tcs.Task;
    }

    private async Task ProcessStateUpdatesAsync(CancellationToken ct)
    {
        await foreach (var updater in _stateUpdates.Reader.ReadAllAsync(ct))
        {
            lock (_lock)
            {
                var result = updater(_state);
                _state = result;
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _stateUpdates.Writer.Complete();
        _processingTask?.Wait(TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }
}