using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProcessorVersionsController : BaseController
{
    private readonly IProcessorVersionService _versionService;
    private readonly ILogger<ProcessorVersionsController> _logger;

    public ProcessorVersionsController(
        IProcessorVersionService versionService,
        ILogger<ProcessorVersionsController> logger)
    {
        _versionService = versionService;
        _logger = logger;
    }

    [HttpGet("{processorId}")]
    public async Task<IActionResult> GetVersions(string processorId)
    {
        var versions = await _versionService.GetVersionsAsync(processorId);
        return Ok(versions, "获取版本历史成功");
    }

    [HttpGet("detail/{versionId}")]
    public async Task<IActionResult> GetVersion(string versionId)
    {
        var version = await _versionService.GetVersionAsync(versionId);
        if (version == null)
        {
            return Fail("未找到指定版本", 404);
        }
        return Ok(version, "获取版本详情成功");
    }

    [HttpPost("{processorId}/commit")]
    public async Task<IActionResult> Commit(string processorId, [FromBody] CommitVersionRequest request)
    {
        var version = await _versionService.CommitAsync(processorId, request.CommitMessage);
        if (version == null)
        {
            return Fail("提交失败，处理器不存在", 404);
        }

        _logger.LogInformation("处理器版本已提交 - ProcessorId: {ProcessorId}, Version: {Version}, Message: {Message}",
            processorId, version.Version, request.CommitMessage);

        return Ok(version, "版本提交成功");
    }

    [HttpPost("{processorId}/rollback/{versionId}")]
    public async Task<IActionResult> Rollback(string processorId, string versionId, [FromBody] RollbackOptions? options = null)
    {
        var result = await _versionService.RollbackAsync(processorId, versionId, options);
        if (result == null)
        {
            return Fail("回退失败，版本或处理器不存在", 404);
        }

        _logger.LogInformation("处理器已回退 - ProcessorId: {ProcessorId}, Version: {Version}, Recovered: {Recovered}, MissingCodes: {Missing}",
            processorId, result.Version.Version, result.RecoveredTemplates.Count, result.MissingEventCodes.Count);

        var message = $"已回退到版本 v{result.Version.Version}: {result.Version.CommitMessage}";
        if (result.HasWarnings)
        {
            message += "（回滚成功，但存在需要关注的事项）";
        }

        return Ok(result, message);
    }
}
