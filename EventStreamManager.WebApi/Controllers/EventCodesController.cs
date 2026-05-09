using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.WebApi.Mappings;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventCodesController : BaseController
{
    private readonly IDataService _dataService;
    private const string FileName = "eventcodes.json";

    public EventCodesController(
        IDataService dataService)
    {
        _dataService = dataService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var list = await _dataService.ReadTemplateAsync<EventCode>(FileName);
        return Ok(list.ToResponses(), "获取事件代码成功");
    }
}