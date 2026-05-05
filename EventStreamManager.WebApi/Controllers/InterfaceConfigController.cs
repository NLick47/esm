using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterfaceConfigController : BaseController
    {
        private readonly IInterfaceConfigService _configService;
        private readonly ILogger<InterfaceConfigController> _logger;

        public InterfaceConfigController(
            IInterfaceConfigService configService,
            ILogger<InterfaceConfigController> logger)
        {
            _configService = configService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            var configs = await _configService.GetAllConfigsAsync();
            return Ok(configs, "获取接口配置列表成功");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfig(string id)
        {
            var config = await _configService.GetConfigByIdAsync(id);

            if (config == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            return Ok(config, "获取接口配置成功");
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfig(InterfaceConfig config)
        {
            var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
            if (!isValid)
            {
                return Fail("部分选择的处理器不存在");
            }

            var validation = await _configService.ValidateReferenceConflictsAsync(config.ProcessorIds);
            if (!validation.IsValid)
            {
                return Fail(validation.ErrorMessage);
            }

            var createdConfig = await _configService.CreateConfigAsync(config);
            return Ok(createdConfig, "创建接口配置成功");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(string id, InterfaceConfig config)
        {
            var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
            if (!isValid)
            {
                return Fail("部分选择的处理器不存在");
            }

            var originalConfig = await _configService.GetConfigByIdAsync(id);
            if (originalConfig == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            var validation = await _configService.ValidateReferenceConflictsAsync(config.ProcessorIds, id);
            if (!validation.IsValid)
            {
                return Fail(validation.ErrorMessage);
            }

            var updatedConfig = await _configService.UpdateConfigAsync(id, config);

            if (updatedConfig == null)
            {
                return Fail($"更新接口配置失败", 500);
            }

            return Ok(updatedConfig, "更新接口配置成功");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(string id)
        {
            var config = await _configService.GetConfigByIdAsync(id);
            if (config == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            var deleted = await _configService.DeleteConfigAsync(id);

            if (!deleted)
            {
                return Fail($"删除接口配置失败", 500);
            }

            _logger.LogInformation("接口配置删除成功 - Id: {Id}, Name: {Name}", id, config.Name);
            return OkMessage("删除接口配置成功");
        }

        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleConfigStatus(string id)
        {
            var config = await _configService.ToggleConfigStatusAsync(id);

            if (config == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            return Ok(config, "切换接口配置状态成功");
        }

        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> DuplicateConfig(string id)
        {
            var originalConfig = await _configService.GetConfigByIdAsync(id);
            if (originalConfig == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            var validation = await _configService.ValidateReferenceConflictsAsync(originalConfig.ProcessorIds);
            if (!validation.IsValid)
            {
                return Fail($"无法复制：{validation.ErrorMessage}");
            }

            var newConfig = await _configService.DuplicateConfigAsync(id);

            if (newConfig == null)
            {
                return Fail($"复制接口配置失败", 500);
            }

            return Ok(newConfig, "复制接口配置成功");
        }

        [HttpGet("processors/available")]
        public async Task<IActionResult> GetAvailableProcessors()
        {
            var statuses = await _configService.GetProcessorReferenceStatusesAsync();
            return Ok(statuses, "获取处理器列表成功");
        }

        [HttpGet("processors/unreferenced")]
        public async Task<IActionResult> GetUnreferencedProcessors()
        {
            var statuses = await _configService.GetProcessorReferenceStatusesAsync();
            var unreferenced = statuses.Where(s => !s.IsReferenced).ToList();
            return Ok(unreferenced, "获取未引用处理器列表成功");
        }

        [HttpGet("{id}/processors")]
        public async Task<IActionResult> GetConfigProcessors(string id)
        {
            var config = await _configService.GetConfigByIdAsync(id);
            if (config == null)
            {
                return Fail($"未找到ID为 {id} 的接口配置", 404);
            }

            var allProcessors = await _configService.GetAvailableProcessorsAsync();
            var configProcessors = allProcessors
                .Where(p => config.ProcessorIds.Contains(p.Id))
                .ToList();

            return Ok(configProcessors, "获取配置关联处理器成功");
        }
    }
}
