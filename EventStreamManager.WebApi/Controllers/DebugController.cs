using EventStreamManager.Infrastructure.Services;
using EventStreamManager.WebApi.Mappings;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : BaseController
{
    private readonly IDebugService _debugService;
   
    private readonly ILogger<DebugController> _logger;

    public DebugController(
        IDebugService debugService,
        ILogger<DebugController> logger)
    {
        _debugService = debugService;
        _logger = logger;
    }

    /// <summary>
    /// 编辑器调试执行 - 专门用于Examine事件调试
    /// </summary>
    [HttpPost("execute-examine")]
    public async Task<IActionResult> ExecuteExamineDebug([FromBody] ExecuteExamineDebugRequest request)
    {
        _logger.LogInformation("开始编辑器调试 - ExamineID: {ExamineId}, 数据库类型: {DatabaseType}",
            request.ExamineId, request.DatabaseType);

        var result = await _debugService.ExecuteExamineDebugAsync(request.ToInfrastructure());
        return result.Success
            ? Ok(result.ToResponse(), "编辑器调试执行完成")
            : Ok(result.ToResponse(), "编辑器调试执行完成（业务执行失败）");
    }

    /// <summary>
    /// 调试执行处理器 - 使用真实事件数据
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteDebug([FromBody] ExecuteDebugRequest request)
    {
        _logger.LogInformation("开始调试处理器: {ProcessorId}, 数据库: {DatabaseType}, 事件ID: {EventId}",
            request.ProcessorId, request.DatabaseType, request.EventId);

        var result = await _debugService.ExecuteDebugAsync(request.ToInfrastructure());
        return result.Success
            ? Ok(result.ToResponse(), "调试执行完成")
            : Ok(result.ToResponse(), "调试执行完成（业务执行失败）");
    }

    /// <summary>
    /// 接口配置调试 - 执行处理器并发送到接口
    /// </summary>
    [HttpPost("interface")]
    public async Task<IActionResult> DebugInterface([FromBody] DebugInterfaceRequest request)
    {
        var result = await _debugService.DebugInterfaceAsync(request.ToInfrastructure());
        
        if (result.Success)
        {
            return Ok(result.ToResponse(), "接口调试完成");
        }

        return Ok(result.ToResponse(), "接口调试完成（业务执行失败）");
    }
}