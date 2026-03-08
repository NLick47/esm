using EventStreamManager.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptController : ControllerBase
{
    private readonly IJavaScriptExecutionService _scriptExecutionService;
    public ScriptController(IJavaScriptExecutionService javaScriptExecutionService)
    {
        _scriptExecutionService = javaScriptExecutionService;
    }

    [HttpPost("validate")]
    public IActionResult ValidateScript([FromBody] string script)
    {
        return Ok(_scriptExecutionService.ValidateScript(script));
    }
}