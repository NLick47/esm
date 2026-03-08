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

        #region 服务总开关

        /// <summary>
        /// 获取服务状态概览
        /// </summary>
        [HttpGet("service/status")]
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
                return Error("获取服务状态失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 启用服务
        /// </summary>
        [HttpPost("service/enable")]
        public async Task<IActionResult> EnableService()
        {
            try
            {
                await _service.EnableAsync();
                return Ok(new
                {
                    message = "服务已启用",
                    isEnabled = _service.IsEnabled,
                    runningDuration = _service.GetRunningDuration()
                }, "服务启用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "启用服务失败");
                return Error("启用服务失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 禁用服务（暂停所有处理器）
        /// </summary>
        [HttpPost("service/disable")]
        public async Task<IActionResult> DisableService()
        {
            try
            {
                await _service.DisableAsync();
                return Ok(new
                {
                    message = "服务已禁用",
                    isEnabled = _service.IsEnabled,
                    runningDuration = _service.GetRunningDuration()
                }, "服务禁用成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "禁用服务失败");
                return Error("禁用服务失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 切换服务状态
        /// </summary>
        [HttpPost("service/toggle")]
        public async Task<IActionResult> ToggleService()
        {
            try
            {
                var newState = await _service.ToggleAsync();
                return Ok(new
                {
                    message = newState ? "服务已启用" : "服务已禁用",
                    isEnabled = newState,
                    runningDuration = _service.GetRunningDuration()
                }, "服务状态切换成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换服务状态失败");
                return Error("切换服务状态失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取服务运行时长
        /// </summary>
        [HttpGet("service/duration")]
        public IActionResult GetRunningDuration()
        {
            try
            {
                var status = _service.GetServiceStatus();
                return Ok(new
                {
                    startTime = status.StartTime,
                    runningDuration = status.RunningDuration,
                    runningDurationStr = FormatDuration(status.RunningDuration),
                    isEnabled = status.IsEnabled,
                    isRunning = status.IsRunning
                }, "获取服务运行时长成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取服务运行时长失败");
                return Error("获取服务运行时长失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取系统运行时间（服务启动时间）
        /// </summary>
        [HttpGet("service/uptime")]
        public IActionResult GetUptime()
        {
            try
            {
                var status = _service.GetServiceStatus();
                var now = DateTime.Now;
                var totalUptime = now - status.StartTime;

                return Ok(new
                {
                    startTime = status.StartTime,
                    currentTime = now,
                    totalUptime,
                    totalUptimeStr = FormatDuration(totalUptime),
                    effectiveRunningDuration = status.RunningDuration,
                    effectiveRunningDurationStr = FormatDuration(status.RunningDuration),
                    isEnabled = status.IsEnabled,
                    processorCount = status.ProcessorCount,
                    activeProcessorCount = status.ActiveProcessorCount
                }, "获取系统运行时间成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取系统运行时间失败");
                return Error("获取系统运行时间失败", data: new { error = ex.Message });
            }
        }

        #endregion

        #region 处理器状态

        /// <summary>
        /// 获取所有处理器状态
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetAllStatus()
        {
            try
            {
                var statuses = _service.GetAllStatus();
                return Ok(statuses, "获取所有处理器状态成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有处理器状态失败");
                return Error("获取所有处理器状态失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 获取单个处理器状态
        /// </summary>
        [HttpGet("status/{databaseType}")]
        public IActionResult GetStatus(string databaseType)
        {
            try
            {
                var status = _service.GetStatus(databaseType);
                if (status == null)
                {
                    return Fail($"未找到处理器: {databaseType}", 404);
                }
                return Ok(status, "获取处理器状态成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取处理器状态失败 - Type: {DatabaseType}", databaseType);
                return Error("获取处理器状态失败", data: new { error = ex.Message });
            }
        }

        /// <summary>
        /// 手动触发扫描
        /// </summary>
        [HttpPost("trigger/{databaseType}")]
        public async Task<IActionResult> TriggerScan(string databaseType)
        {
            try
            {
                if (!_service.IsEnabled)
                {
                    return Fail("服务处于禁用状态，请先启用服务");
                }

                await _service.TriggerScanAsync(databaseType);
                return Ok(new { message = $"已触发 {databaseType} 的扫描" }, "触发扫描成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "触发扫描失败 - Type: {DatabaseType}", databaseType);
                return Error("触发扫描失败", data: new { error = ex.Message });
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 格式化时长为可读字符串
        /// </summary>
        private static string FormatDuration(TimeSpan duration)
        {
            var parts = new List<string>();

            if (duration.Days > 0)
                parts.Add($"{duration.Days}天");
            if (duration.Hours > 0)
                parts.Add($"{duration.Hours}小时");
            if (duration.Minutes > 0)
                parts.Add($"{duration.Minutes}分钟");
            if (duration.Seconds > 0 || parts.Count == 0)
                parts.Add($"{duration.Seconds}秒");

            return string.Join(" ", parts);
        }

        #endregion
    }
}