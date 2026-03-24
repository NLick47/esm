// Controllers/EventProcessorController.cs
using EventStreamManager.EventProcessor;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers
{
    /// <summary>
    /// 事件处理器服务控制与状态监控
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventProcessorController : BaseController
    {
        private readonly EventProcessorService _service;
        private readonly ILogger<EventProcessorController> _logger;

        public EventProcessorController(
            EventProcessorService service,
            ILogger<EventProcessorController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetServiceStatus()
        {
            var status = _service.GetServiceStatus();
            return Ok(status, "获取服务状态成功");
        }

        /// <summary>
        /// 启用服务
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> EnableService()
        {
            await _service.EnableAsync();
            var status = _service.GetServiceStatus();
            return Ok(status, "服务启用成功");
        }

        /// <summary>
        /// 禁用服务
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> DisableService()
        {
            await _service.DisableAsync();
            var status = _service.GetServiceStatus();
            return Ok(status, "服务禁用成功");
        }

        /// <summary>
        /// 获取所有处理器状态
        /// </summary>
        [HttpGet("processors")]
        public IActionResult GetAllProcessorStatus()
        {
            var statuses = _service.GetAllStatus();
            return Ok(statuses, "获取处理器状态成功");
        }
    }
}