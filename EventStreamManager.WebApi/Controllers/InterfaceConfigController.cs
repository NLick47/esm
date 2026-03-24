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
            // 验证处理器是否存在
            var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
            if (!isValid)
            {
                return Fail("部分选择的处理器不存在");
            }

            // 检查每个处理器是否已经被其他接口配置引用
            var allConfigs = await _configService.GetAllConfigsAsync();
            var allProcessors = await _configService.GetAvailableProcessorsAsync();
                
            var conflictingProcessors = new List<string>();
            foreach (var processorId in config.ProcessorIds)
            {
                var existingConfig = await _configService.GetConfigByProcessorIdAsync(processorId);
                if (existingConfig != null)
                {
                    var processor = allProcessors.FirstOrDefault(p => p.Id == processorId);
                    conflictingProcessors.Add($"处理器 \"{processor?.Name ?? processorId}\" 已被接口配置 \"{existingConfig.Name}\" 引用");
                }
            }

            if (conflictingProcessors.Any())
            {
                return Fail($"以下处理器已被其他接口配置引用：\n{string.Join("\n", conflictingProcessors)}");
            }

            var createdConfig = await _configService.CreateConfigAsync(config);
            return Ok(createdConfig, "创建接口配置成功");
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(string id, InterfaceConfig config)
        {
             // 验证处理器是否存在
                var isValid = await _configService.ValidateProcessorIdsAsync(config.ProcessorIds);
                if (!isValid)
                {
                    return Fail("部分选择的处理器不存在");
                }

                // 获取原配置信息
                var originalConfig = await _configService.GetConfigByIdAsync(id);
                if (originalConfig == null)
                {
                    return Fail($"未找到ID为 {id} 的接口配置", 404);
                }

                // 检查每个处理器是否已经被其他接口配置引用（排除自己）
                var allConfigs = await _configService.GetAllConfigsAsync();
                var allProcessors = await _configService.GetAvailableProcessorsAsync();
                
                var conflictingProcessors = new List<string>();
                foreach (var processorId in config.ProcessorIds)
                {
                    // 如果是原配置已经关联的处理器，跳过检查
                    if (originalConfig.ProcessorIds.Contains(processorId))
                    {
                        continue;
                    }

                    var existingConfig = await _configService.GetConfigByProcessorIdAsync(processorId);
                    if (existingConfig != null && existingConfig.Id != id)
                    {
                        var processor = allProcessors.FirstOrDefault(p => p.Id == processorId);
                        conflictingProcessors.Add($"处理器 \"{processor?.Name ?? processorId}\" 已被接口配置 \"{existingConfig.Name}\" 引用");
                    }
                }

                if (conflictingProcessors.Any())
                {
                    return Fail($"以下处理器已被其他接口配置引用：\n{string.Join("\n", conflictingProcessors)}");
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

            // 检查被复制的处理器是否已经被其他配置引用
            var allConfigs = await _configService.GetAllConfigsAsync();
            var allProcessors = await _configService.GetAvailableProcessorsAsync();
                
            var conflictingProcessors = new List<string>();
            foreach (var processorId in originalConfig.ProcessorIds)
            {
                var existingConfig = await _configService.GetConfigByProcessorIdAsync(processorId);
                // 如果处理器被其他配置引用，且不是当前配置，则冲突
                if (existingConfig != null && existingConfig.Id != id)
                {
                    var processor = allProcessors.FirstOrDefault(p => p.Id == processorId);
                    conflictingProcessors.Add($"处理器 \"{processor?.Name ?? processorId}\" 已被接口配置 \"{existingConfig.Name}\" 引用");
                }
            }

            if (conflictingProcessors.Any())
            {
                return Fail($"无法复制：以下处理器已被其他接口配置引用：\n{string.Join("\n", conflictingProcessors)}");
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
            var allProcessors = await _configService.GetAvailableProcessorsAsync();
            var allConfigs = await _configService.GetAllConfigsAsync();
                
            // 获取每个处理器的引用状态
            var processorsWithStatus = new List<object>();
                
            foreach (var processor in allProcessors)
            {
                var referencingConfig = allConfigs.FirstOrDefault(c => c.ProcessorIds.Contains(processor.Id));
                    
                processorsWithStatus.Add(new
                {
                    processor.Id,
                    processor.Name,
                    IsReferenced = referencingConfig != null,
                    ReferencedBy = referencingConfig?.Name,
                    ReferencedById = referencingConfig?.Id
                });
            }
                
            return Ok(processorsWithStatus, "获取处理器列表成功");
        }
        
        
        /// <summary>
        /// 获取未被引用的处理器列表
        /// </summary>
        [HttpGet("processors/unreferenced")]
        public async Task<IActionResult> GetUnreferencedProcessors()
        {
            var allProcessors = await _configService.GetAvailableProcessorsAsync();
            var allConfigs = await _configService.GetAllConfigsAsync();
                
            // 获取已被引用的处理器ID
            var referencedProcessorIds = allConfigs
                .SelectMany(c => c.ProcessorIds)
                .ToHashSet();
                
            // 返回未被引用的处理器
            var unreferencedProcessors = allProcessors
                .Where(p => !referencedProcessorIds.Contains(p.Id))
                .ToList();
                
            return Ok(unreferencedProcessors, "获取未引用处理器列表成功");
        }

        /// <summary>
        /// 获取指定配置的处理器列表
        /// </summary>
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