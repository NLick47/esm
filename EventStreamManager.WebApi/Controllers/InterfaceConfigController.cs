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
            try
            {
                var configs = await _configService.GetAllConfigsAsync();
                return Ok(configs, "获取接口配置列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取接口配置列表失败");
                return Error("获取接口配置列表失败", data: new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfig(string id)
        {
            try
            {
                var config = await _configService.GetConfigByIdAsync(id);
                
                if (config == null)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }
                
                return Ok(config, "获取接口配置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取接口配置失败 - Id: {Id}", id);
                return Error("获取接口配置失败", data: new { error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateConfig(InterfaceConfig config)
        {
            try
            {
                // 验证必填字段
                if (string.IsNullOrWhiteSpace(config.Name))
                {
                    return Fail("配置名称不能为空");
                }
                
                if (string.IsNullOrWhiteSpace(config.Url))
                {
                    return Fail("接口URL不能为空");
                }
                
                if (config.ProcessorIds.Count == 0)
                {
                    return Fail("请至少选择一个关联的处理器");
                }

                // 验证处理器是否存在
                var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
                if (!isValid)
                {
                    return Fail("部分选择的处理器不存在");
                }

                var createdConfig = await _configService.CreateConfigAsync(config);
                return Ok(createdConfig, "创建接口配置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建接口配置失败");
                return Error("创建接口配置失败", data: new { error = ex.Message });
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(string id, InterfaceConfig config)
        {
            try
            {
                // 验证必填字段
                if (string.IsNullOrWhiteSpace(config.Name))
                {
                    return Fail("配置名称不能为空");
                }
                
                if (string.IsNullOrWhiteSpace(config.Url))
                {
                    return Fail("接口URL不能为空");
                }
                
                if (config.ProcessorIds.Count == 0)
                {
                    return Fail("请至少选择一个关联的处理器");
                }

                // 验证处理器是否存在
                var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
                if (!isValid)
                {
                    return Fail("部分选择的处理器不存在");
                }

                var updatedConfig = await _configService.UpdateConfigAsync(id, config);
                
                if (updatedConfig == null)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }
                
                return Ok(updatedConfig, "更新接口配置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新接口配置失败 - Id: {Id}", id);
                return Error("更新接口配置失败", data: new { error = ex.Message });
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(string id)
        {
            try
            {
                var deleted = await _configService.DeleteConfigAsync(id);
                
                if (!deleted)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }
                
                return OkMessage("删除接口配置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除接口配置失败 - Id: {Id}", id);
                return Error("删除接口配置失败", data: new { error = ex.Message });
            }
        }
        
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleConfigStatus(string id)
        {
            try
            {
                var config = await _configService.ToggleConfigStatusAsync(id);
                
                if (config == null)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }
                
                return Ok(config, "切换接口配置状态成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "切换接口配置状态失败 - Id: {Id}", id);
                return Error("切换接口配置状态失败", data: new { error = ex.Message });
            }
        }
        
        [HttpPost("{id}/duplicate")]
        public async Task<IActionResult> DuplicateConfig(string id)
        {
            try
            {
                var newConfig = await _configService.DuplicateConfigAsync(id);
                
                if (newConfig == null)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }
                
                return Ok(newConfig, "复制接口配置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "复制接口配置失败 - Id: {Id}", id);
                return Error("复制接口配置失败", data: new { error = ex.Message });
            }
        }
        
        [HttpGet("processors/available")]
        public async Task<IActionResult> GetAvailableProcessors()
        {
            try
            {
                var processors = await _configService.GetAvailableProcessorsAsync();
                return Ok(processors, "获取可用处理器列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取可用处理器列表失败");
                return Error("获取可用处理器列表失败", data: new { error = ex.Message });
            }
        }
    }
}