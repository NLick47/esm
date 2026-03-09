using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.EventProcessor.Processors;
using EventStreamManager.Infrastructure.Models.EventProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor;

/// <summary>
/// 事件处理器后台服务 - 管理多个数据库类型处理器
/// </summary>
public class EventProcessorService : BackgroundService
{
    private readonly ProcessorFactory _factory;
    private readonly IEventListenerConfigService _configService;
    private readonly IDataService _dataService; 
    private readonly ILogger<EventProcessorService> _logger;

    private readonly Dictionary<string, DatabaseTypeProcessor> _processors = new();
    private Timer? _refreshTimer;
    private Timer? _stateSaveTimer; 

    // 总开关状态
    private bool _isEnabled;
    private readonly object _stateLock = new();

    // 服务运行时间记录
    private DateTime _startTime;
    private DateTime? _pauseTime;
    private TimeSpan _totalPausedDuration = TimeSpan.Zero;

    private const string StateFile = "service-state.json";
    private const int StateSaveIntervalMinutes = 1; // 每分钟保存一次状态

    public EventProcessorService(
        ProcessorFactory factory,
        IEventListenerConfigService configService,
        IDataService dataService, // 注入数据服务
        ILogger<EventProcessorService> logger)
    {
        _factory = factory;
        _configService = configService;
        _dataService = dataService;
        _logger = logger;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await LoadStateAsync();
        _logger.LogInformation("EventProcessorService 正在启动...");
        
       
        if (_isEnabled)
        {
            _logger.LogInformation("根据持久化状态，服务将自动启用");
        }
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _startTime = DateTime.Now;
        _logger.LogInformation("========== 事件处理器服务启动 ==========");

        try
        {
            // 只有在启用状态下才初始化处理器
            if (_isEnabled)
            {
                await InitializeProcessorsAsync(stoppingToken);
            }
            else
            {
                _logger.LogInformation("服务处于禁用状态，跳过处理器初始化");
            }

            // 启动定时刷新
            _refreshTimer = new Timer(
                _ => _ = RefreshProcessorsAsync(stoppingToken),
                null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));

            // 启动状态保存定时器
            _stateSaveTimer = new Timer(
                async _ => await SaveStateAsync(),
                null,
                TimeSpan.FromMinutes(StateSaveIntervalMinutes),
                TimeSpan.FromMinutes(StateSaveIntervalMinutes));

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("事件处理器服务收到停止信号");
        }
        finally
        {
            await StopAllProcessorsAsync();
            _refreshTimer?.Dispose();
            _stateSaveTimer?.Dispose();
            
            // 服务停止前保存最终状态
            await SaveStateAsync();
            
            _logger.LogInformation("========== 事件处理器服务停止 ==========");
        }
    }

    /// <summary>
    /// 获取服务是否启用
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            lock (_stateLock)
            {
                return _isEnabled;
            }
        }
    }

    /// <summary>
    /// 获取服务是否正在运行
    /// </summary>
    public bool IsRunning => _processors.Values.Any(p => p.IsRunning);

    /// <summary>
    /// 获取服务启动时间
    /// </summary>
    public DateTime StartTime => _startTime;

    /// <summary>
    /// 获取服务运行时长（扣除暂停时间）
    /// </summary>
    public TimeSpan GetRunningDuration()
    {
        var now = DateTime.Now;
        var totalDuration = now - _startTime;

        // 如果当前处于暂停状态，扣除当前暂停时长
        var currentPauseDuration = TimeSpan.Zero;
        if (_pauseTime.HasValue)
        {
            currentPauseDuration = now - _pauseTime.Value;
        }

        return totalDuration - _totalPausedDuration - currentPauseDuration;
    }

    /// <summary>
    /// 启用服务
    /// </summary>
    public async Task EnableAsync()
    {
        List<Task> startTasks = new();
        
        lock (_stateLock)
        {
            if (_isEnabled)
            {
                _logger.LogWarning("服务已经处于启用状态");
                return;
            }

            _isEnabled = true;

            // 计算暂停时长
            if (_pauseTime.HasValue)
            {
                _totalPausedDuration += DateTime.Now - _pauseTime.Value;
                _pauseTime = null;
            }

            // 收集需要启动的处理器
            foreach (var processor in _processors.Values.Where(p => !p.IsRunning))
            {
                startTasks.Add(processor.StartAsync(CancellationToken.None));
            }
        }

        _logger.LogInformation("事件处理服务已启用");

        // 启动所有处理器
        if (startTasks.Any())
        {
            await Task.WhenAll(startTasks);
        }

        // 立即保存状态
        await SaveStateAsync();
    }

    /// <summary>
    /// 禁用服务（暂停所有处理器）
    /// </summary>
    public async Task DisableAsync()
    {
        lock (_stateLock)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("服务已经处于禁用状态");
                return;
            }

            _isEnabled = false;
            _pauseTime = DateTime.Now;
        }

        _logger.LogInformation("事件处理服务已禁用");

        // 停止所有处理器
        await StopAllProcessorsAsync();

        // 立即保存状态
        await SaveStateAsync();
    }

    /// <summary>
    /// 切换服务状态
    /// </summary>
    public async Task<bool> ToggleAsync()
    {
        if (_isEnabled)
        {
            await DisableAsync();
            return false;
        }
        else
        {
            await EnableAsync();
            return true;
        }
    }

    /// <summary>
    /// 获取服务状态概览
    /// </summary>
    public ServiceStatus GetServiceStatus()
    {
        return new ServiceStatus
        {
            IsEnabled = IsEnabled,
            IsRunning = IsRunning,
            StartTime = _startTime,
            RunningDuration = GetRunningDuration(),
            ProcessorCount = _processors.Count,
            ActiveProcessorCount = _processors.Values.Count(p => p.IsRunning),
            Processors = _processors.Values.Select(p => p.GetStatus()).ToList()
        };
    }

    private async Task InitializeProcessorsAsync(CancellationToken token)
    {
        // 检查总开关状态
        if (!IsEnabled)
        {
            _logger.LogInformation("服务处于禁用状态，跳过初始化处理器");
            return;
        }

        var types = await _factory.GetConfiguredTypesAsync();
        foreach (var dbType in types)
        {
            try
            {
                var processor = _factory.Create(dbType);
                _processors[dbType] = processor;
                await processor.StartAsync(token);
                _logger.LogInformation("处理器已启动: {DatabaseType}", dbType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启动处理器失败: {DatabaseType}", dbType);
            }
        }
    }

    private async Task RefreshProcessorsAsync(CancellationToken token)
    {
        // 如果服务被禁用，跳过刷新
        if (!IsEnabled)
        {
            return;
        }

        try
        {
            var types = await _factory.GetConfiguredTypesAsync();
            var configs = await _configService.GetAllConfigsAsync();

            // 新增处理器
            foreach (var dbType in types.Where(t => !_processors.ContainsKey(t)))
            {
                _logger.LogInformation("新增处理器: {DatabaseType}", dbType);
                var processor = _factory.Create(dbType);
                _processors[dbType] = processor;
                await processor.StartAsync(token);
            }

            // 检查启用/禁用状态
            foreach (var (dbType, processor) in _processors)
            {
                if (configs.Databases.TryGetValue(dbType, out var config))
                {
                    if (config.Enabled && !processor.IsRunning)
                    {
                        _logger.LogInformation("启用处理器: {DatabaseType}", dbType);
                        await processor.StartAsync(token);
                    }
                    else if (!config.Enabled && processor.IsRunning)
                    {
                        _logger.LogInformation("禁用处理器: {DatabaseType}", dbType);
                        await processor.StopAsync();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新处理器配置失败");
        }
    }

    private async Task StopAllProcessorsAsync()
    {
        var tasks = _processors.Values.Select(p => p.StopAsync());
        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 获取所有处理器状态
    /// </summary>
    public List<ProcessorStatus> GetAllStatus()
        => _processors.Values.Select(p => p.GetStatus()).ToList();

    /// <summary>
    /// 获取单个处理器状态
    /// </summary>
    public ProcessorStatus? GetStatus(string databaseType)
        => _processors.TryGetValue(databaseType, out var p) ? p.GetStatus() : null;

    /// <summary>
    /// 手动触发扫描
    /// </summary>
    public async Task TriggerScanAsync(string databaseType)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("服务处于禁用状态，无法触发扫描");
            return;
        }

        if (_processors.TryGetValue(databaseType, out var processor))
        {
            await processor.StopAsync();
            await processor.StartAsync(CancellationToken.None);
            
            _logger.LogInformation("手动触发扫描完成: {DatabaseType}", databaseType);
        }
    }

    /// <summary>
    /// 立即保存服务状态
    /// </summary>
    public async Task SaveStateImmediatelyAsync()
    {
        await SaveStateAsync();
    }

    #region 状态持久化

    /// <summary>
    /// 加载服务状态
    /// </summary>
    private async Task LoadStateAsync()
    {
        try
        {
            var states = await _dataService.ReadAsync<ServiceState>(StateFile);
            var state = states.FirstOrDefault();

            lock (_stateLock)
            {
                if (state != null)
                {
                    _isEnabled = state.IsEnabled;
                    if (!state.IsEnabled)
                    {
                        // 如果之前是禁用状态，设置暂停时间
                        _pauseTime = DateTime.Now;
                        _totalPausedDuration = TimeSpan.Zero;
                    }
                    
                    _logger.LogInformation("已加载服务状态: IsEnabled={IsEnabled}, LastUpdated={LastUpdated}", 
                        _isEnabled, state.LastUpdated);
                }
                else
                {
                    // 默认状态：禁用
                    _isEnabled = false;
                    _pauseTime = DateTime.Now;
                    _totalPausedDuration = TimeSpan.Zero;
                    
                    _logger.LogInformation("未找到历史状态，使用默认禁用状态");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载服务状态失败，使用默认状态");
            
            lock (_stateLock)
            {
                _isEnabled = false;
                _pauseTime = DateTime.Now;
                _totalPausedDuration = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// 保存服务状态
    /// </summary>
    private async Task SaveStateAsync()
    {
        try
        {
            ServiceState state;
            
            lock (_stateLock)
            {
                state = new ServiceState
                {
                    IsEnabled = _isEnabled,
                    StartTime = _startTime,
                    LastUpdated = DateTime.Now,
                    Version = "1.0"
                };
            }

            await _dataService.WriteAsync(StateFile, new List<ServiceState> { state });

            _logger.LogDebug("服务状态已保存: IsEnabled={IsEnabled}, LastUpdated={LastUpdated}", 
                state.IsEnabled, state.LastUpdated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存服务状态失败");
        }
    }

    #endregion

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("EventProcessorService 正在停止...");
        
        // 停止前保存状态
        await SaveStateAsync();
        
        await base.StopAsync(cancellationToken);
    }
}

