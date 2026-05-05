using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class InterfaceConfigService : IInterfaceConfigService
{
    private readonly IDataService _dataService;
    private const string ConfigFileName = "interfaceConfigs.json";
    private const string ProcessorsFileName = "processors.json";

    public InterfaceConfigService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public async Task<List<InterfaceConfig>> GetAllConfigsAsync()
    {
        return await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
    }

    public async Task<InterfaceConfig?> GetConfigByIdAsync(string id)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        return configs.FirstOrDefault(c => c.Id == id);
    }

    public async Task<InterfaceConfig> CreateConfigAsync(InterfaceConfig config)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);

        // 生成新ID
        config.Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        // 验证并设置处理器名称
        await SetProcessorNamesAsync(config);

        configs.Add(config);
        await _dataService.WriteAsync(ConfigFileName, configs);

        return config;
    }

    public async Task<InterfaceConfig?> UpdateConfigAsync(string id, InterfaceConfig config)
    {
        if (id != config.Id)
        {
            return null;
        }

        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        var existingConfig = configs.FirstOrDefault(c => c.Id == id);

        if (existingConfig == null)
        {
            return null;
        }

        // 验证并设置处理器名称
        if (!await SetProcessorNamesAsync(config))
        {
            return null;
        }

        var index = configs.FindIndex(c => c.Id == id);
        configs[index] = config;

        await _dataService.WriteAsync(ConfigFileName, configs);

        return config;
    }

    public async Task<bool> DeleteConfigAsync(string id)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        var config = configs.FirstOrDefault(c => c.Id == id);

        if (config == null)
        {
            return false;
        }

        configs.Remove(config);
        await _dataService.WriteAsync(ConfigFileName, configs);

        return true;
    }

    public async Task<InterfaceConfig?> ToggleConfigStatusAsync(string id)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        var config = configs.FirstOrDefault(c => c.Id == id);

        if (config == null)
        {
            return null;
        }

        config.Enabled = !config.Enabled;
        await _dataService.WriteAsync(ConfigFileName, configs);

        return config;
    }

    public async Task<InterfaceConfig?> DuplicateConfigAsync(string id)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        var config = configs.FirstOrDefault(c => c.Id == id);

        if (config == null)
        {
            return null;
        }

        var newConfig = new InterfaceConfig
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(),
            Name = $"{config.Name} (复制)",
            ProcessorIds = new List<string>(config.ProcessorIds),
            ProcessorNames = new List<string>(config.ProcessorNames),
            Url = config.Url,
            Method = config.Method,
            Headers = config.Headers.Select(h => new HeaderItem { Key = h.Key, Value = h.Value }).ToList(),
            Timeout = config.Timeout,
            RetryCount = config.RetryCount,
            RetryInterval = config.RetryInterval,
            Enabled = false,
            RequestTemplate = config.RequestTemplate,
            Description = config.Description
        };

        configs.Add(newConfig);
        await _dataService.WriteAsync(ConfigFileName, configs);

        return newConfig;
    }

    public async Task<List<AvailableProcessor>> GetAvailableProcessorsAsync()
    {
        var list = await _dataService.ReadAsync<JsProcessor>(ProcessorsFileName);
        return list.Select(x => new AvailableProcessor()
        {
            Name = x.Name,
            Id = x.Id
        }).ToList();
    }

    public async Task<bool> ValidateProcessorIdsAsync(List<string> processorIds)
    {
        var processors = await GetAvailableProcessorsAsync();
        var validProcessorIds = processorIds
            .Where(id => processors.Any(p => p.Id == id))
            .ToList();

        return validProcessorIds.Count == processorIds.Count;
    }

    private async Task<bool> SetProcessorNamesAsync(InterfaceConfig config)
    {
        var processors = await GetAvailableProcessorsAsync();
        var validProcessorIds = config.ProcessorIds
            .Where(id => processors.Any(p => p.Id == id))
            .ToList();

        if (validProcessorIds.Count != config.ProcessorIds.Count)
        {
            return false;
        }

        config.ProcessorNames = validProcessorIds
            .Select(id => processors.First(p => p.Id == id).Name)
            .ToList();

        return true;
    }

    public async Task<InterfaceConfig?> GetConfigByProcessorIdAsync(string processorId)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        return configs.FirstOrDefault(c => c.ProcessorIds.Contains(processorId));
    }

    public async Task<List<InterfaceConfig>> GetConfigsByProcessorIdAsync(string processorId)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        return configs.Where(c => c.ProcessorIds.Contains(processorId)).ToList();
    }

    public async Task<bool> IsProcessorReferencedAsync(string processorId)
    {
        var configs = await _dataService.ReadAsync<InterfaceConfig>(ConfigFileName);
        return configs.Any(c => c.ProcessorIds.Contains(processorId));
    }

    public async Task<List<ProcessorReferenceStatus>> GetProcessorReferenceStatusesAsync()
    {
        var processors = await GetAvailableProcessorsAsync();
        var configs = await GetAllConfigsAsync();

        // 构建处理器ID -> 引用配置的映射（取第一个）
        var referenceMap = new Dictionary<string, InterfaceConfig>(StringComparer.Ordinal);
        foreach (var config in configs)
        {
            foreach (var processorId in config.ProcessorIds)
            {
                // 只记录第一个引用该处理器的配置
                if (!referenceMap.ContainsKey(processorId))
                {
                    referenceMap[processorId] = config;
                }
            }
        }

        return processors.Select(p =>
        {
            var isReferenced = referenceMap.TryGetValue(p.Id, out var config);
            return new ProcessorReferenceStatus
            {
                Id = p.Id,
                Name = p.Name,
                IsReferenced = isReferenced,
                ReferencedByConfigId = config?.Id,
                ReferencedByConfigName = config?.Name
            };
        }).ToList();
    }

    public async Task<ReferenceValidationResult> ValidateReferenceConflictsAsync(
        List<string> processorIds,
        string? excludeConfigId = null)
    {
        var configs = await GetAllConfigsAsync();
        var processors = await GetAvailableProcessorsAsync();

        var conflicts = new List<string>();
        var processorNameMap = processors.ToDictionary(p => p.Id, p => p.Name);

        foreach (var processorId in processorIds)
        {
            // 找到所有引用该处理器的配置（排除自身）
            var referencingConfigs = configs
                .Where(c => c.ProcessorIds.Contains(processorId))
                .Where(c => c.Id != excludeConfigId)
                .ToList();

            if (referencingConfigs.Count > 0)
            {
                var firstConfig = referencingConfigs.First();
                var processorName = processorNameMap.GetValueOrDefault(processorId, processorId);
                conflicts.Add($"处理器 \"{processorName}\" 已被接口配置 \"{firstConfig.Name}\" 引用");
            }
        }

        if (conflicts.Count == 0)
        {
            return new ReferenceValidationResult { IsValid = true };
        }

        return new ReferenceValidationResult
        {
            IsValid = false,
            ErrorMessage = $"以下处理器已被其他接口配置引用：\n{string.Join("\n", conflicts)}"
        };
    }
}
