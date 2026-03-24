using AutoMapper;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SqlTemplatesController : BaseController
{
    private readonly ISqlTemplateService _sqlTemplateService;
    private readonly ILogger<SqlTemplatesController> _logger;
    private readonly IMapper _mapper;
    public SqlTemplatesController(
        ISqlTemplateService sqlTemplateService,
        ILogger<SqlTemplatesController> logger, IMapper mapper)
    {
        _sqlTemplateService = sqlTemplateService;
        _logger = logger;
        _mapper = mapper;
    }
    
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemTemplates()
    {
        try
        {
            var list = await _sqlTemplateService.GetSystemTemplatesAsync();
            return Ok(list, "获取系统模板列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取系统模板列表失败");
            return Error("获取系统模板列表失败", data: new { error = ex.Message });
        }
    }
    
    [HttpGet("custom")]
    public async Task<IActionResult> GetCustomTemplates()
    {
        try
        {
            var list = await _sqlTemplateService.GetCustomTemplatesAsync();
            return Ok(list, "获取自定义模板列表成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取自定义模板列表失败");
            return Error("获取自定义模板列表失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustom([FromBody] CustomSqlTemplateRequest  request)
    {
        try
        {
            var template = _mapper.Map<CustomSqlTemplate>(request);
            var created = await _sqlTemplateService.CreateCustomAsync(template);
            return Ok(created, "创建自定义模板成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建自定义模板失败");
            return Error("创建自定义模板失败", data: new { error = ex.Message });
        }
    }
    
    [HttpPut("custom/{id}")]
    public async Task<IActionResult> UpdateCustom(string id, [FromBody] CustomSqlTemplateRequest request)
    {
        try
        {
            var template = _mapper.Map<CustomSqlTemplate>(request);
            var updated = await _sqlTemplateService.UpdateCustomAsync(id, template);
            if (!updated)
            {
                return Fail($"未找到ID为 {id} 的自定义模板", 404);
            }
            return OkMessage("更新自定义模板成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新自定义模板失败 - Id: {Id}", id);
            return Error("更新自定义模板失败", data: new { error = ex.Message });
        }
    }
    
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustom(string id)
    {
        try
        {
            var deleted = await _sqlTemplateService.DeleteCustomAsync(id);
            if (!deleted)
            {
                return Fail($"未找到ID为 {id} 的自定义模板", 404);
            }
            return OkMessage("删除自定义模板成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除自定义模板失败 - Id: {Id}", id);
            return Error("删除自定义模板失败", data: new { error = ex.Message });
        }
    }
}