using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Mappings;
using EventStreamManager.WebApi.Models.Requests;
using EventStreamManager.WebApi.Models.Responses;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessorsController : BaseController
{
    private readonly IProcessorService _processorService;
    private readonly IInterfaceConfigService _interfaceConfigService;
    private readonly ILogger<ProcessorsController> _logger;
    private readonly ISqlTemplateService _sqlTemplateService; 

    public ProcessorsController(
        IProcessorService processorService,
        IInterfaceConfigService interfaceConfigService,
        ILogger<ProcessorsController> logger, 
        ISqlTemplateService sqlTemplateService)
    {
        _processorService = processorService;
        _interfaceConfigService = interfaceConfigService;
        _logger = logger;
        _sqlTemplateService = sqlTemplateService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _processorService.GetAllAsync();
        return Ok(list, "获取处理器列表成功");
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _processorService.GetByIdAsync(id);
        if (item == null)
        {
            return Fail($"未找到ID为 {id} 的处理器", 404);
        }
    
        var response = item.ToDetailResponse();
    
        if (!string.IsNullOrEmpty(item.SqlTemplate))
        {
            response.SqlTemplate = item.SqlTemplate;
            
            if (item.SqlTemplateType == SqlTemplateType.System)
            {
                var systemTemplates = await _sqlTemplateService.GetSystemTemplatesAsync();
                var template = systemTemplates.FirstOrDefault(t => t.Id == item.SqlTemplateId);
                response.SqlTemplateName = template?.Name;
            }
            else if (item.SqlTemplateType == SqlTemplateType.Custom)
            {
                var customTemplates = await _sqlTemplateService.GetCustomTemplatesAsync();
                var template = customTemplates.FirstOrDefault(t => t.Id == item.SqlTemplateId);
                response.SqlTemplateName = template?.Name;
            }
        }
    
        return Ok(response, "获取处理器成功");
    }
    
    /// <summary>
    /// 获取默认的处理器模板代码
    /// </summary>
    [HttpGet("default-template")]
    public async Task<IActionResult> GetDefaultTemplate()
    {
        var code = await _processorService.GetDefaultTemplateAsync();
        return Ok(new { code }, "获取默认模板成功");
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProcessorRequest request)
    {
        var processor = request.ToEntity();
        var created = await _processorService.CreateAsync(processor);
        return Ok(created, "创建处理器成功");
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ProcessorRequest request)
    {
        var processor = request.ToEntity();
        var updated = await _processorService.UpdateAsync(id, processor);
        if (!updated)
        {
            return Fail($"未找到ID为 {id} 的处理器", 404);
        }
        return OkMessage("更新处理器成功");
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
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
    
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(string id)
    {
        var item = await _processorService.ToggleAsync(id);
        if (item == null)
        {
            return Fail($"未找到ID为 {id} 的处理器", 404);
        }
        return Ok(item, "切换处理器状态成功");
    }
}
