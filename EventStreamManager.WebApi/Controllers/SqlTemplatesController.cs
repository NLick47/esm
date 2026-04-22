using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Mappings;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SqlTemplatesController : BaseController
{
    private readonly ISqlTemplateService _sqlTemplateService;
    public SqlTemplatesController(
        ISqlTemplateService sqlTemplateService)
    {
        _sqlTemplateService = sqlTemplateService;
    }
    
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemTemplates()
    {
        var list = await _sqlTemplateService.GetSystemTemplatesAsync();
        return Ok(list, "获取系统模板列表成功");
    }
    
    [HttpGet("custom")]
    public async Task<IActionResult> GetCustomTemplates()
    {
        var list = await _sqlTemplateService.GetCustomTemplatesAsync();
        return Ok(list, "获取自定义模板列表成功");
    }
    
    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustom([FromBody] CustomSqlTemplateRequest  request)
    {
        var template = request.ToEntity();
        var created = await _sqlTemplateService.CreateCustomAsync(template);
        return Ok(created, "创建自定义模板成功");
    }
    
    [HttpPut("custom/{id}")]
    public async Task<IActionResult> UpdateCustom(string id, [FromBody] CustomSqlTemplateRequest request)
    {
        var template = request.ToEntity();
        var updated = await _sqlTemplateService.UpdateCustomAsync(id, template);
        if (!updated)
        {
            return Fail($"未找到ID为 {id} 的自定义模板", 404);
        }
        return OkMessage("更新自定义模板成功");
    }
    
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustom(string id)
    {
        var deleted = await _sqlTemplateService.DeleteCustomAsync(id);
        if (!deleted)
        {
            return Fail($"未找到ID为 {id} 的自定义模板", 404);
        }
        return OkMessage("删除自定义模板成功");
    }
}
