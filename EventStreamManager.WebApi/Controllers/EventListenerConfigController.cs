using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventListenerConfigController : BaseController
{
    private readonly IEventListenerConfigService _configService;
    private readonly ILogger<EventListenerConfigController> _logger;
    
    public EventListenerConfigController(
        IEventListenerConfigService configService,
        ILogger<EventListenerConfigController> logger)
    {
        _configService = configService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllConfigs()
    {
        try
        {
            var configs = await _configService.GetAllConfigsAsync();
            return Ok(configs, "获取所有配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有配置失败");
            return Error("获取配置失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("{databaseType}")]
    public async Task<IActionResult> GetConfigByType(string databaseType)
    {
        try
        {
            var config = await _configService.GetConfigByTypeAsync(databaseType);
            if (config == null)
            {
                return Fail($"未找到 {databaseType} 的配置", 404);
            }
            return Ok(config, "获取配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置失败 - Type: {DatabaseType}", databaseType);
            return Error("获取配置失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPut("{databaseType}")]
    public async Task<IActionResult> UpdateConfig(string databaseType, [FromBody] EventConfig config)
    {
        try
        {
            var updatedConfig = await _configService.UpdateConfigAsync(databaseType, config);
            return Ok(updatedConfig, "更新配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配置失败 - Type: {DatabaseType}", databaseType);
            return Error("更新配置失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("{databaseType}/start-condition")]
    public async Task<IActionResult> GetStartCondition(string databaseType)
    {
        try
        {
            var condition = await _configService.GetStartConditionAsync(databaseType);
            if (condition == null)
            {
                return Fail($"未找到 {databaseType} 的起始条件", 404);
            }
            return Ok(condition, "获取起始条件成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取起始条件失败 - Type: {DatabaseType}", databaseType);
            return Error("获取起始条件失败", data: new { error = ex.Message });
        }
    }

    [HttpPut("{databaseType}/start-condition")]
    public async Task<IActionResult> UpdateStartCondition(string databaseType, [FromBody] StartCondition condition)
    {
        try
        {
            var success = await _configService.UpdateStartConditionAsync(databaseType, condition);
            if (!success)
            {
                return Fail($"未找到 {databaseType} 的配置", 404);
            }
            return Ok(new { message = "起始条件更新成功", condition }, "更新起始条件成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新起始条件失败 - Type: {DatabaseType}", databaseType);
            return Error("更新起始条件失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPatch("{databaseType}/toggle")]
    public async Task<IActionResult> ToggleEnabled(string databaseType, [FromQuery] bool enabled)
    {
        try
        {
            var success = await _configService.ToggleEnabledAsync(databaseType, enabled);
            if (!success)
            {
                return Fail($"未找到 {databaseType} 的配置", 404);
            }
            return Ok(new { success = true, enabled }, "切换启用状态成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换启用状态失败 - Type: {DatabaseType}", databaseType);
            return Error("切换状态失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPost("{databaseType}/reset")]
    public async Task<IActionResult> ResetToDefault(string databaseType)
    {
        try
        {
            var defaultConfig = await _configService.ResetToDefaultAsync(databaseType);
            return Ok(defaultConfig, "重置配置成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置配置失败 - Type: {DatabaseType}", databaseType);
            return Error("重置配置失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("types")]
    public async Task<IActionResult> GetDatabaseTypes()
    {
        try
        {
            var types = await _configService.GetDatabaseTypesAsync();
            return Ok(types, "获取数据库类型成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取数据库类型失败");
            return Error("获取数据库类型失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPut("batch")]
    public async Task<IActionResult> BatchUpdate([FromBody] Dictionary<string, EventConfig> updates)
    {
        try
        {
            foreach (var update in updates)
            {
                await _configService.UpdateConfigAsync(update.Key, update.Value);
            }
            return Ok(new { message = "批量更新成功", count = updates.Count }, "批量更新成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新失败");
            return Error("批量更新失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        try
        {
            var configs = await _configService.GetAllConfigsAsync();
                
            var statistics = new
            {
                totalDatabases = configs.Databases.Count,
                enabledCount = configs.Databases.Count(d => d.Value.Enabled),
                disabledCount = configs.Databases.Count(d => !d.Value.Enabled),
                totalEventsProcessed = configs.Databases.Sum(d => d.Value.TotalEventsProcessed),
                averageScanFrequency = configs.Databases.Average(d => d.Value.ScanFrequency),
                lastUpdated = configs.LastUpdated
            };

            return Ok(statistics, "获取统计数据成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取统计数据失败");
            return Error("获取统计数据失败", data: new { error = ex.Message });
        }
    }
}