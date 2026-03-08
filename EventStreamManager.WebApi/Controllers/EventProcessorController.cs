using EventStreamManager.EventProcessor;
using EventStreamManager.EventProcessor.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers
{
    /// <summary>
    /// 事件处理器服务控制与状态监控
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class EventProcessorController : ControllerBase
    {
        private readonly EventProcessorService _service;

        public EventProcessorController(EventProcessorService service)
        {
            _service = service;
        }

        #region 服务总开关

        /// <summary>
        /// 获取服务状态概览
        /// </summary>
        [HttpGet("service/status")]
        public ActionResult<ServiceStatus> GetServiceStatus()
            => Ok(_service.GetServiceStatus());

        /// <summary>
        /// 启用服务
        /// </summary>
        [HttpPost("service/enable")]
        public async Task<ActionResult> EnableService()
        {
            await _service.EnableAsync();
            return Ok(new
            {
                message = "服务已启用",
                isEnabled = _service.IsEnabled,
                runningDuration = _service.GetRunningDuration()
            });
        }

        /// <summary>
        /// 禁用服务（暂停所有处理器）
        /// </summary>
        [HttpPost("service/disable")]
        public async Task<ActionResult> DisableService()
        {
            await _service.DisableAsync();
            return Ok(new
            {
                message = "服务已禁用",
                isEnabled = _service.IsEnabled,
                runningDuration = _service.GetRunningDuration()
            });
        }

        /// <summary>
        /// 切换服务状态
        /// </summary>
        [HttpPost("service/toggle")]
        public async Task<ActionResult> ToggleService()
        {
            var newState = await _service.ToggleAsync();
            return Ok(new
            {
                message = newState ? "服务已启用" : "服务已禁用",
                isEnabled = newState,
                runningDuration = _service.GetRunningDuration()
            });
        }

        /// <summary>
        /// 获取服务运行时长
        /// </summary>
        [HttpGet("service/duration")]
        public ActionResult GetRunningDuration()
        {
            var status = _service.GetServiceStatus();
            return Ok(new
            {
                startTime = status.StartTime,
                runningDuration = status.RunningDuration,
                runningDurationStr = FormatDuration(status.RunningDuration),
                isEnabled = status.IsEnabled,
                isRunning = status.IsRunning
            });
        }

        /// <summary>
        /// 获取系统运行时间（服务启动时间）
        /// </summary>
        [HttpGet("service/uptime")]
        public ActionResult GetUptime()
        {
            var status = _service.GetServiceStatus();
            var now = DateTime.Now;
            var totalUptime = now - status.StartTime;

            return Ok(new
            {
                startTime = status.StartTime,
                currentTime = now,
                totalUptime = totalUptime,
                totalUptimeStr = FormatDuration(totalUptime),
                effectiveRunningDuration = status.RunningDuration,
                effectiveRunningDurationStr = FormatDuration(status.RunningDuration),
                isEnabled = status.IsEnabled,
                processorCount = status.ProcessorCount,
                activeProcessorCount = status.ActiveProcessorCount
            });
        }

        #endregion

        #region 处理器状态

        /// <summary>
        /// 获取所有处理器状态
        /// </summary>
        [HttpGet("status")]
        public ActionResult<List<ProcessorStatus>> GetAllStatus()
            => Ok(_service.GetAllStatus());

        /// <summary>
        /// 获取单个处理器状态
        /// </summary>
        [HttpGet("status/{databaseType}")]
        public ActionResult<ProcessorStatus> GetStatus(string databaseType)
        {
            var status = _service.GetStatus(databaseType);
            return status == null ? NotFound(new { message = $"未找到处理器: {databaseType}" }) : Ok(status);
        }

        /// <summary>
        /// 手动触发扫描
        /// </summary>
        [HttpPost("trigger/{databaseType}")]
        public async Task<ActionResult> TriggerScan(string databaseType)
        {
            if (!_service.IsEnabled)
            {
                return BadRequest(new { message = "服务处于禁用状态，请先启用服务" });
            }

            try
            {
                await _service.TriggerScanAsync(databaseType);
                return Ok(new { message = $"已触发 {databaseType} 的扫描" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
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
