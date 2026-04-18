using EventStreamManager.JSFunction.Runtime;
using EventStreamManager.WebApi.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScriptController : BaseController
{
    private readonly IJavaScriptExecutionService _scriptExecutionService;

    public ScriptController(
        IJavaScriptExecutionService javaScriptExecutionService)
    {
        _scriptExecutionService = javaScriptExecutionService;
    }

    [HttpPost("validate")]
    public IActionResult ValidateScript([FromBody] ValidateScriptRequest request)
    {
        var result = _scriptExecutionService.ValidateScript(request.Script);
        return Ok(result, "脚本验证完成");
    }
}