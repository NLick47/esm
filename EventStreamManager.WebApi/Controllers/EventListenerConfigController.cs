using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventListenerConfigController : BaseController
{
    private readonly IEventListenerConfigService _configService;

    public EventListenerConfigController(
        IEventListenerConfigService configService)
    {
        _configService = configService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllConfigs()
    {
        var configs = await _configService.GetAllConfigsAsync();
        return Ok(configs, "获取所有配置成功");
    }
    
    [HttpGet("{databaseType}")]
    public async Task<IActionResult> GetConfigByType(string databaseType)
    {
        var config = await _configService.GetConfigByTypeAsync(databaseType);
        if (config == null)
        {
            config = await _configService.UpdateConfigAsync(databaseType, new EventConfig());
        }
        return Ok(config, "获取配置成功");
    }
    
    [HttpPut("{databaseType}")]
    public async Task<IActionResult> UpdateConfig(string databaseType, [FromBody] EventConfig config)
    {
        var updatedConfig = await _configService.UpdateConfigAsync(databaseType, config);
        return Ok(updatedConfig, "更新配置成功");
    }
    
    [HttpGet("{databaseType}/start-condition")]
    public async Task<IActionResult> GetStartCondition(string databaseType)
    {
        var condition = await _configService.GetStartConditionAsync(databaseType);
        if (condition == null)
        {
            return Fail($"未找到 {databaseType} 的起始条件", 404);
        }
        return Ok(condition, "获取起始条件成功");
    }

    [HttpPut("{databaseType}/start-condition")]
    public async Task<IActionResult> UpdateStartCondition(string databaseType, [FromBody] StartCondition condition)
    {
        var success = await _configService.UpdateStartConditionAsync(databaseType, condition);
        if (!success)
        {
            return Fail($"未找到 {databaseType} 的配置", 404);
        }
        return Ok(new { message = "起始条件更新成功", condition }, "更新起始条件成功");
    }
    
    [HttpPatch("{databaseType}/toggle")]
    public async Task<IActionResult> ToggleEnabled(string databaseType, [FromQuery] bool enabled)
    {
        var success = await _configService.ToggleEnabledAsync(databaseType, enabled);
        if (!success)
        {
            return Fail($"未找到 {databaseType} 的配置", 404);
        }
        return Ok(new { success = true, enabled }, "切换启用状态成功");
    }
    
    [HttpPost("{databaseType}/reset")]
    public async Task<IActionResult> ResetToDefault(string databaseType)
    {
        var defaultConfig = await _configService.ResetToDefaultAsync(databaseType);
        return Ok(defaultConfig, "重置配置成功");
    }
    
    [HttpGet("types")]
    public async Task<IActionResult> GetDatabaseTypes()
    {
        var types = await _configService.GetDatabaseTypesAsync();
        return Ok(types, "获取数据库类型成功");
    }
    
    [HttpPut("batch")]
    public async Task<IActionResult> BatchUpdate([FromBody] Dictionary<string, EventConfig> updates)
    {
        foreach (var update in updates)
        {
            await _configService.UpdateConfigAsync(update.Key, update.Value);
        }
        return Ok(new { message = "批量更新成功", count = updates.Count }, "批量更新成功");
    }
    
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
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
}