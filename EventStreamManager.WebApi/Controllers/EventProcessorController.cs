using EventStreamManager.EventProcessor.Services;
using EventStreamManager.WebApi.Models.Requests;
using EventStreamManager.WebApi.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

/// <summary>
/// 事件处理器服务控制与状态监控
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EventProcessorController : BaseController
{
    private readonly IStateManagerService _stateManager;
    private readonly IProcessorManagerService _processorManager;
    private readonly ILogger<EventProcessorController> _logger;

    public EventProcessorController(
        IStateManagerService stateManager,
        IProcessorManagerService processorManager,
        ILogger<EventProcessorController> logger)
    {
        _stateManager = stateManager;
        _processorManager = processorManager;
        _logger = logger;
    }

    #region 服务状态

    /// <summary>
    /// 获取服务状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var snapshot = _stateManager.GetSnapshot();
        
        var response = new ServiceStatusResponse
        {
            IsEnabled = snapshot.IsEnabled,
            IsRunning = _processorManager.IsRunning,
            StartTime = snapshot.StartTime,
            RunningDuration = snapshot.GetRunningDuration(),
            ProcessorCount = _processorManager.ProcessorCount,
            ActiveProcessorCount = _processorManager.ActiveProcessorCount,
            Processors = _processorManager.GetAllStatus()
                .Select(p => new ProcessorStatusResponse
                {
                    DatabaseType = p.DatabaseType,
                    IsRunning = p.IsRunning,
                    LastScanTime = p.LastScanTime,
                    ProcessedEventCount = p.TotalProcessedCount,
                    LastError = p.LastError
                })
                .ToList()
        };

        return Ok(response, "获取服务状态成功");
    }

    /// <summary>
    /// 启用服务
    /// </summary>
    [HttpPost("enable")]
    public async Task<IActionResult> Enable()
    {
        await _stateManager.EnableAsync();
        _logger.LogInformation("服务已通过 API 启用");

        var snapshot = _stateManager.GetSnapshot();
        
        var response = new ServiceStatusResponse
        {
            IsEnabled = snapshot.IsEnabled,
            IsRunning = _processorManager.IsRunning,
            StartTime = snapshot.StartTime,
            RunningDuration = snapshot.GetRunningDuration(),
            ProcessorCount = _processorManager.ProcessorCount,
            ActiveProcessorCount = _processorManager.ActiveProcessorCount
        };

        return Ok(response, "服务启用成功");
    }

    /// <summary>
    /// 禁用服务
    /// </summary>
    [HttpPost("disable")]
    public async Task<IActionResult> Disable()
    {
        await _stateManager.DisableAsync();
        _logger.LogInformation("服务已通过 API 禁用");

        var snapshot = _stateManager.GetSnapshot();
        
        var response = new ServiceStatusResponse
        {
            IsEnabled = snapshot.IsEnabled,
            IsRunning = _processorManager.IsRunning,
            StartTime = snapshot.StartTime,
            RunningDuration = snapshot.GetRunningDuration(),
            ProcessorCount = _processorManager.ProcessorCount,
            ActiveProcessorCount = _processorManager.ActiveProcessorCount
        };

        return Ok(response, "服务禁用成功");
    }

    /// <summary>
    /// 切换服务状态
    /// </summary>
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle([FromBody] ToggleStateRequest? request)
    {
        bool result;
        
        if (request?.ForceState.HasValue == true)
        {
            if (request.ForceState.Value)
            {
                await _stateManager.EnableAsync();
                result = true;
            }
            else
            {
                await _stateManager.DisableAsync();
                result = false;
            }
        }
        else
        {
            result = await _stateManager.ToggleAsync();
        }

        _logger.LogInformation("服务状态已切换为: {IsEnabled}", result);

        var snapshot = _stateManager.GetSnapshot();
        
        var response = new ServiceStatusResponse
        {
            IsEnabled = snapshot.IsEnabled,
            IsRunning = _processorManager.IsRunning,
            StartTime = snapshot.StartTime,
            RunningDuration = snapshot.GetRunningDuration(),
            ProcessorCount = _processorManager.ProcessorCount,
            ActiveProcessorCount = _processorManager.ActiveProcessorCount
        };

        return Ok(response, result ? "服务已启用" : "服务已禁用");
    }

    #endregion

    #region 处理器管理

    /// <summary>
    /// 获取所有处理器状态
    /// </summary>
    [HttpGet("processors")]
    public IActionResult GetProcessors()
    {
        var response = _processorManager.GetAllStatus()
            .Select(p => new ProcessorStatusResponse
            {
                DatabaseType = p.DatabaseType,
                IsRunning = p.IsRunning,
                LastScanTime = p.LastScanTime,
                ProcessedEventCount = p.TotalProcessedCount,
                LastError = p.LastError
            });

        return Ok(response, "获取处理器状态成功");
    }

    /// <summary>
    /// 获取单个处理器状态
    /// </summary>
    [HttpGet("processors/{databaseType}")]
    public IActionResult GetProcessor(string databaseType)
    {
        var status = _processorManager.GetStatus(databaseType);
        
        if (status == null)
        {
            return NotFound(NotFound($"处理器 '{databaseType}' 不存在"));
        }

        var response = new ProcessorStatusResponse
        {
            DatabaseType = status.DatabaseType,
            IsRunning = status.IsRunning,
            LastScanTime = status.LastScanTime,
            ProcessedEventCount = status.TotalProcessedCount,
            LastError = status.LastError
        };

        return Ok(response, "获取处理器状态成功");
    }

    /// <summary>
    /// 手动触发处理器扫描
    /// </summary>
    [HttpPost("processors/{databaseType}/scan")]
    public async Task<IActionResult> ScanProcessor(string databaseType)
    {
        await _processorManager.TriggerScanAsync(databaseType);
        
        return Ok(new { DatabaseType = databaseType }, $"已触发处理器 '{databaseType}' 扫描");
    }

    /// <summary>
    /// 立即刷新处理器配置
    /// </summary>
    [HttpPost("processors/refresh")]
    public async Task<IActionResult> RefreshProcessors()
    {
        await _processorManager.RefreshConfigurationAsync(HttpContext.RequestAborted);
        
        var response = new 
        { 
            Total = _processorManager.ProcessorCount,
            Active = _processorManager.ActiveProcessorCount
        };

        return Ok(response, "处理器配置刷新成功");
    }

    #endregion
}