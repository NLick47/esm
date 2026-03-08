using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessorsController : ControllerBase
{
    private readonly IProcessorService _processorService;

    public ProcessorsController(IProcessorService processorService)
    {
        _processorService = processorService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _processorService.GetAllAsync();
        return Ok(list);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _processorService.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
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
            return Ok(new { code });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"读取模板失败: {ex.Message}" });
        }
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] JSProcessor processor)
    {
        var created = await _processorService.CreateAsync(processor);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }
    
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] JSProcessor processor)
    {
        var updated = await _processorService.UpdateAsync(id, processor);
        if (!updated) return NotFound();
        return NoContent();
    }
    
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _processorService.DeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
    
    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(string id)
    {
        var item = await _processorService.ToggleAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }
}