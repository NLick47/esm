using EventStreamManager.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptController : BaseController
{
    private readonly IJavaScriptExecutionService _scriptExecutionService;
    private readonly ILogger<ScriptController> _logger;

    public ScriptController(
        IJavaScriptExecutionService javaScriptExecutionService,
        ILogger<ScriptController> logger)
    {
        _scriptExecutionService = javaScriptExecutionService;
        _logger = logger;
    }

    [HttpPost("validate")]
    public IActionResult ValidateScript([FromBody] string script)
    {
        var result = _scriptExecutionService.ValidateScript(script);
        return Ok(result, "脚本验证完成");
    }
}