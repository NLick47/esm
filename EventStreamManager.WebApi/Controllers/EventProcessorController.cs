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
            try
            {
                var status = _service.GetServiceStatus();
                return Ok(status, "获取服务状态成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取服务状态失败");
                return Error("获取服务状态失败");
            }
        }

        /// <summary>
        /// 启用服务
        /// </summary>
        [HttpPost("enable")]
        public async Task<IActionResult> EnableService()
        {
            try
            {
                await _service.EnableAsync();
                var status = _service.GetServiceStatus();
                return Ok(status, "服务启用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启用服务失败");
                return Error("启用服务失败");
            }
        }

        /// <summary>
        /// 禁用服务
        /// </summary>
        [HttpPost("disable")]
        public async Task<IActionResult> DisableService()
        {
            try
            {
                await _service.DisableAsync();
                var status = _service.GetServiceStatus();
                return Ok(status, "服务禁用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "禁用服务失败");
                return Error("禁用服务失败");
            }
        }

        /// <summary>
        /// 获取所有处理器状态
        /// </summary>
        [HttpGet("processors")]
        public IActionResult GetAllProcessorStatus()
        {
            try
            {
                var statuses = _service.GetAllStatus();
                return Ok(statuses, "获取处理器状态成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取处理器状态失败");
                return Error("获取处理器状态失败");
            }
        }
    }
}