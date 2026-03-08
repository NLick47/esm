using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SqlTemplatesController : ControllerBase
{
    private readonly ISqlTemplateService _sqlTemplateService;

    public SqlTemplatesController(ISqlTemplateService sqlTemplateService)
    {
        _sqlTemplateService = sqlTemplateService;
    }
    
    [HttpGet("system")]
    public async Task<IActionResult> GetSystemTemplates()
    {
        var list = await _sqlTemplateService.GetSystemTemplatesAsync();
        return Ok(list);
    }
    
    [HttpGet("custom")]
    public async Task<IActionResult> GetCustomTemplates()
    {
        var list = await _sqlTemplateService.GetCustomTemplatesAsync();
        return Ok(list);
    }
    
    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustom([FromBody] CustomSqlTemplate template)
    {
        var created = await _sqlTemplateService.CreateCustomAsync(template);
        return CreatedAtAction(nameof(GetCustomTemplates), new { id = created.Id }, created);
    }
    
    [HttpPut("custom/{id}")]
    public async Task<IActionResult> UpdateCustom(string id, [FromBody] CustomSqlTemplate template)
    {
        var updated = await _sqlTemplateService.UpdateCustomAsync(id, template);
        if (!updated) return NotFound();
        return NoContent();
    }
    
    [HttpDelete("custom/{id}")]
    public async Task<IActionResult> DeleteCustom(string id)
    {
        var deleted = await _sqlTemplateService.DeleteCustomAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}