using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventCodesController : BaseController
{
    private readonly IDataService _dataService;
    private readonly ILogger<EventCodesController> _logger;
    private const string FileName = "eventcodes.json";

    public EventCodesController(
        IDataService dataService,
        ILogger<EventCodesController> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var list = await _dataService.ReadTemplateAsync<EventCode>(FileName);
            return Ok(list, "获取事件代码成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取事件代码失败");
            return Error("获取事件代码失败", data: new { error = ex.Message });
        }
    }
}