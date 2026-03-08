using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class EventListenerConfigController : ControllerBase
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
    public async Task<ActionResult<EventListenerConfigs>> GetAllConfigs()
    {
        try
        {
            var configs = await _configService.GetAllConfigsAsync();
            return Ok(configs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有配置失败");
            return StatusCode(500, new { message = "获取配置失败", error = ex.Message });
        }
    }
    
    [HttpGet("{databaseType}")]
    public async Task<ActionResult<EventConfig>> GetConfigByType(string databaseType)
    {
        try
        {
            var config = await _configService.GetConfigByTypeAsync(databaseType);
            if (config == null)
            {
                return NotFound(new { message = $"未找到 {databaseType} 的配置" });
            }
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "获取配置失败", error = ex.Message });
        }
    }
    
    
    
    [HttpPut("{databaseType}")]
    public async Task<ActionResult<EventConfig>> UpdateConfig(string databaseType, [FromBody] EventConfig config)
    {
        try
        {
           
            var updatedConfig = await _configService.UpdateConfigAsync(databaseType, config);
            return Ok(updatedConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配置失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "更新配置失败", error = ex.Message });
        }
    }
    
    
    [HttpGet("{databaseType}/start-condition")]
    public async Task<ActionResult<StartCondition>> GetStartCondition(string databaseType)
    {
        try
        {
            var condition = await _configService.GetStartConditionAsync(databaseType);
            if (condition == null)
            {
                return NotFound(new { message = $"未找到 {databaseType} 的起始条件" });
            }
            return Ok(condition);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取起始条件失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "获取起始条件失败", error = ex.Message });
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
                return NotFound(new { message = $"未找到 {databaseType} 的配置" });
            }
            return Ok(new { message = "起始条件更新成功", condition });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新起始条件失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "更新起始条件失败", error = ex.Message });
        }
    }
    
    
    [HttpPatch("{databaseType}/toggle")]
    public async Task<IActionResult> ToggleEnabled(string databaseType, [FromBody] bool enabled)
    {
        try
        {
            var success = await _configService.ToggleEnabledAsync(databaseType, enabled);
            if (!success)
            {
                return NotFound(new { message = $"未找到 {databaseType} 的配置" });
            }
            return Ok(new { success = true, enabled });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换启用状态失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "切换状态失败", error = ex.Message });
        }
    }
    
    
    [HttpPost("{databaseType}/reset")]
    public async Task<ActionResult<EventConfig>> ResetToDefault(string databaseType)
    {
        try
        {
            var defaultConfig = await _configService.ResetToDefaultAsync(databaseType);
            return Ok(defaultConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重置配置失败 - Type: {DatabaseType}", databaseType);
            return StatusCode(500, new { message = "重置配置失败", error = ex.Message });
        }
    }
    
    
    [HttpGet("types")]
    public async Task<ActionResult<List<DatabaseTypeInfo>>> GetDatabaseTypes()
    {
        try
        {
            var types = await _configService.GetDatabaseTypesAsync();
            return Ok(types);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取数据库类型失败");
            return StatusCode(500, new { message = "获取数据库类型失败", error = ex.Message });
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
            return Ok(new { message = "批量更新成功", count = updates.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量更新失败");
            return StatusCode(500, new { message = "批量更新失败", error = ex.Message });
        }
    }
    
    
    [HttpGet("statistics")]
    public async Task<ActionResult<object>> GetStatistics()
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

            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取统计数据失败");
            return StatusCode(500, new { message = "获取统计数据失败", error = ex.Message });
        }
    }
}