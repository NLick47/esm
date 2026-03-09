using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessorsController : BaseController
{
    private readonly IProcessorService _processorService;
    private readonly IInterfaceConfigService _interfaceConfigService;
    private readonly ILogger<ProcessorsController> _logger;

    public ProcessorsController(
        IProcessorService processorService,
        IInterfaceConfigService interfaceConfigService,
        ILogger<ProcessorsController> logger)
    {
        _processorService = processorService;
        _interfaceConfigService = interfaceConfigService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _processorService.GetAllAsync();
            return Ok(list, "获取处理器列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理器列表失败");
            return Error("获取处理器列表失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        try
        {
            var item = await _processorService.GetByIdAsync(id);
            if (item == null)
            {
                return Fail($"未找到ID为 {id} 的处理器", 404);
            }
            return Ok(item, "获取处理器成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取处理器失败 - Id: {Id}", id);
            return Error("获取处理器失败", data: new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// 获取默认的处理器模板代码
    /// </summary>
    [HttpGet("default-template")]
    public async Task<IActionResult> GetDefaultTemplate()
    {
        try
        {
            var code = await _processorService.GetDefaultTemplateAsync();
            return Ok(new { code }, "获取默认模板成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取默认模板失败");
            return Error("获取默认模板失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JSProcessor processor)
    {
        try
        {
            var created = await _processorService.CreateAsync(processor);
            return Ok(created, "创建处理器成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建处理器失败");
            return Error("创建处理器失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] JSProcessor processor)
    {
        try
        {
            var updated = await _processorService.UpdateAsync(id, processor);
            if (!updated)
            {
                return Fail($"未找到ID为 {id} 的处理器", 404);
            }
            return OkMessage("更新处理器成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新处理器失败 - Id: {Id}", id);
            return Error("更新处理器失败", data: new { error = ex.Message });
        }
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            
            var processor = await _processorService.GetByIdAsync(id);
            if (processor == null)
            {
                return Fail($"未找到ID为 {id} 的处理器", 404);
            }
            
            var referencingConfig = await _interfaceConfigService.GetConfigByProcessorIdAsync(id);
            if (referencingConfig != null)
            {
                _logger.LogWarning("尝试删除被引用的处理器 - Id: {Id}, Name: {Name}, 引用配置: {ConfigName}", 
                    id, processor.Name, referencingConfig.Name);
                
                return Fail($"处理器 \"{processor.Name}\" 正在被接口配置 \"{referencingConfig.Name}\" 引用，无法删除");
            }
            var deleted = await _processorService.DeleteAsync(id);
            if (!deleted)
            {
                return Fail($"删除处理器失败", 500);
            }
            
            _logger.LogInformation("处理器删除成功 - Id: {Id}, Name: {Name}", id, processor.Name);
            return OkMessage("删除处理器成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除处理器失败 - Id: {Id}", id);
            return Error("删除处理器失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(string id)
    {
        try
        {
            var item = await _processorService.ToggleAsync(id);
            if (item == null)
            {
                return Fail($"未找到ID为 {id} 的处理器", 404);
            }
            return Ok(item, "切换处理器状态成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换处理器状态失败 - Id: {Id}", id);
            return Error("切换处理器状态失败", data: new { error = ex.Message });
        }
    }
}