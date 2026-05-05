using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PipelineController : BaseController
{
    private readonly IPipelineService _pipelineService;

    public PipelineController(
        IPipelineService pipelineService)
    {
        _pipelineService = pipelineService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _pipelineService.GetAllAsync();
        return Ok(list, "获取管道列表成功");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _pipelineService.GetByIdAsync(id);
        if (item == null)
        {
            return Fail($"未找到ID为 {id} 的管道", 404);
        }
        return Ok(item, "获取管道成功");
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProcessorPipeline pipeline)
    {
        var created = await _pipelineService.CreateAsync(pipeline);
        return Ok(created, "创建管道成功");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] ProcessorPipeline pipeline)
    {
        var updated = await _pipelineService.UpdateAsync(id, pipeline);
        if (!updated)
        {
            return Fail($"未找到ID为 {id} 的管道", 404);
        }
        return OkMessage("更新管道成功");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var deleted = await _pipelineService.DeleteAsync(id);
        if (!deleted)
        {
            return Fail($"未找到ID为 {id} 的管道", 404);
        }
        return OkMessage("删除管道成功");
    }

    [HttpPatch("{id}/toggle")]
    public async Task<IActionResult> Toggle(string id)
    {
        var item = await _pipelineService.ToggleAsync(id);
        if (!item)
        {
            return Fail($"未找到ID为 {id} 的管道", 404);
        }
        return OkMessage("切换管道状态成功");
    }
}
