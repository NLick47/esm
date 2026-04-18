using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SystemVariablesController : BaseController
{
    private readonly ISystemVariableService _systemVariableService;
    private readonly ILogger<SystemVariablesController> _logger;

    public SystemVariablesController(
        ISystemVariableService systemVariableService,
        ILogger<SystemVariablesController> logger)
    {
        _systemVariableService = systemVariableService;
        _logger = logger;
    }

    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _systemVariableService.GetAllAsync();
        return Ok(list, "获取系统变量列表成功");
    }

   
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        var item = await _systemVariableService.GetByIdAsync(id);
        if (item == null)
        {
            return Fail($"未找到ID为 {id} 的系统变量", 404);
        }
        return Ok(item, "获取系统变量成功");
    }

 
    [HttpGet("by-key/{key}")]
    public async Task<IActionResult> GetByKey(string key)
    {
        var item = await _systemVariableService.GetByKeyAsync(key);
        if (item == null)
        {
            return Fail($"未找到键名为 {key} 的系统变量", 404);
        }
        return Ok(item, "获取系统变量成功");
    }

  
    [HttpPost]
    public async Task<IActionResult> CreateOrUpdate([FromBody] SystemVariableRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Key))
        {
            return Fail("变量键名不能为空");
        }

        var variable = await _systemVariableService.SetAsync(
            request.Key,
            request.Value,
            request.Description ?? "",
            request.Category ?? "General"
        );

        _logger.LogInformation("系统变量已保存 - Key: {Key}, Category: {Category}", variable.Key, variable.Category);
        return Ok(variable, "保存系统变量成功");
    }

   
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var item = await _systemVariableService.GetByIdAsync(id);
        if (item == null)
        {
            return Fail($"未找到ID为 {id} 的系统变量", 404);
        }

        var deleted = await _systemVariableService.DeleteAsync(id);
        if (!deleted)
        {
            return Fail("删除系统变量失败", 500);
        }

        _logger.LogInformation("系统变量已删除 - Id: {Id}, Key: {Key}", id, item.Key);
        return OkMessage("删除系统变量成功");
    }

 
    [HttpDelete("by-key/{key}")]
    public async Task<IActionResult> DeleteByKey(string key)
    {
        var item = await _systemVariableService.GetByKeyAsync(key);
        if (item == null)
        {
            return Fail($"未找到键名为 {key} 的系统变量", 404);
        }

        var deleted = await _systemVariableService.DeleteByKeyAsync(key);
        if (!deleted)
        {
            return Fail("删除系统变量失败", 500);
        }

        _logger.LogInformation("系统变量已删除 - Key: {Key}", key);
        return OkMessage("删除系统变量成功");
    }
}


