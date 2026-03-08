using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace EventStreamManager.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterfaceConfigController : ControllerBase
    {
        private readonly IInterfaceConfigService _configService;

        public InterfaceConfigController(IInterfaceConfigService configService)
        {
            _configService = configService;
        }

       
        [HttpGet]
        public async Task<ActionResult<List<InterfaceConfig>>> GetConfigs()
        {
            var configs = await _configService.GetAllConfigsAsync();
            return Ok(configs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InterfaceConfig>> GetConfig(string id)
        {
            var config = await _configService.GetConfigByIdAsync(id);
            
            if (config == null)
            {
                return NotFound($"未找到ID为 {id} 的接口配置");
            }
            
            return Ok(config);
        }


        [HttpPost]
        public async Task<ActionResult<InterfaceConfig>> CreateConfig(InterfaceConfig config)
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(config.Name))
            {
                return BadRequest("配置名称不能为空");
            }
            
            if (string.IsNullOrWhiteSpace(config.Url))
            {
                return BadRequest("接口URL不能为空");
            }
            
            if (config.ProcessorIds == null || config.ProcessorIds.Count == 0)
            {
                return BadRequest("请至少选择一个关联的处理器");
            }

            // 验证处理器是否存在
            var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
            if (!isValid)
            {
                return BadRequest("部分选择的处理器不存在");
            }

            var createdConfig = await _configService.CreateConfigAsync(config);
            return CreatedAtAction(nameof(GetConfig), new { id = createdConfig.Id }, createdConfig);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(string id, InterfaceConfig config)
        {
            // 验证必填字段
            if (string.IsNullOrWhiteSpace(config.Name))
            {
                return BadRequest("配置名称不能为空");
            }
            
            if (string.IsNullOrWhiteSpace(config.Url))
            {
                return BadRequest("接口URL不能为空");
            }
            
            if (config.ProcessorIds == null || config.ProcessorIds.Count == 0)
            {
                return BadRequest("请至少选择一个关联的处理器");
            }

            // 验证处理器是否存在
            var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
            if (!isValid)
            {
                return BadRequest("部分选择的处理器不存在");
            }

            var updatedConfig = await _configService.UpdateConfigAsync(id, config);
            
            if (updatedConfig == null)
            {
                return NotFound($"未找到ID为 {id} 的接口配置");
            }
            
            return Ok(updatedConfig);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(string id)
        {
            var deleted = await _configService.DeleteConfigAsync(id);
            
            if (!deleted)
            {
                return NotFound($"未找到ID为 {id} 的接口配置");
            }
            
            return NoContent();
        }
        
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleConfigStatus(string id)
        {
            var config = await _configService.ToggleConfigStatusAsync(id);
            
            if (config == null)
            {
                return NotFound($"未找到ID为 {id} 的接口配置");
            }
            
            return Ok(config);
        }
        
        [HttpPost("{id}/duplicate")]
        public async Task<ActionResult<InterfaceConfig>> DuplicateConfig(string id)
        {
            var newConfig = await _configService.DuplicateConfigAsync(id);
            
            if (newConfig == null)
            {
                return NotFound($"未找到ID为 {id} 的接口配置");
            }
            
            return CreatedAtAction(nameof(GetConfig), new { id = newConfig.Id }, newConfig);
        }
        
        [HttpGet("processors/available")]
        public async Task<ActionResult<List<AvailableProcessor>>> GetAvailableProcessors()
        {
            var processors = await _configService.GetAvailableProcessorsAsync();
            return Ok(processors);
        }
    }
}