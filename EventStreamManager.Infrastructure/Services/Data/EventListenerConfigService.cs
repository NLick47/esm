using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services.Data
{
    public class EventListenerConfigService : IEventListenerConfigService
    {
        private readonly IDataService _dataService;
        private readonly ILogger<EventListenerConfigService> _logger;
        private readonly string _configFileName = "event-listener-config.json";
        private readonly IDatabaseSchemeService _databaseSchemeService;
      
        private Dictionary<string, string> _databaseTypes;
        private readonly object _databaseTypesLock = new object();

        public EventListenerConfigService(IDataService dataService,
            ILogger<EventListenerConfigService> logger, 
            IDatabaseSchemeService databaseSchemeService)
        {
            _dataService = dataService;
            _logger = logger;
            _databaseSchemeService = databaseSchemeService;
            _databaseTypes = new Dictionary<string, string>();
        }

        /// <summary>
        /// 从配置文件或数据源加载数据库类型
        /// </summary>
        private async Task<Dictionary<string, string>> LoadDatabaseTypesAsync()
        {
            if (_databaseTypes.Any())
                return _databaseTypes;

            lock (_databaseTypesLock)
            {
                if (_databaseTypes.Any())
                    return _databaseTypes;
            }

            try
            {
                var typesList = await _databaseSchemeService.GetAllDatabaseTypesAsync();
                if (typesList.Any())
                {
                    var types = new Dictionary<string, string>();
                    foreach (var type in typesList)
                    {
                        if (!string.IsNullOrEmpty(type.Value) && !string.IsNullOrEmpty(type.Label))
                        {
                            types[type.Value] = type.Label;
                        }
                    }
                    
                    lock (_databaseTypesLock)
                    {
                        _databaseTypes = types;
                    }
                    return _databaseTypes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "加载数据库类型配置失败，将使用默认配置");
            }
            
            lock (_databaseTypesLock)
            {
                _databaseTypes = new Dictionary<string, string>();
            }
            return _databaseTypes;
        }

        private async Task<EventListenerConfigs> GetConfigsAsync()
        {
            try
            {
                var configsList = await _dataService.ReadAsync<EventListenerConfigs>(_configFileName);

                if (configsList.FirstOrDefault() is { } configs && configs.Databases.Any())
                {
                    // 验证配置中的数据库类型是否都存在于数据库类型列表中
                    var validTypes = await LoadDatabaseTypesAsync();
                    if (validTypes.Any())
                    {
                        var invalidTypes = configs.Databases.Keys
                            .Where(key => !validTypes.ContainsKey(key))
                            .ToList();
                        
                        foreach (var invalidType in invalidTypes)
                        {
                            _logger.LogWarning("配置中包含无效的数据库类型: {DatabaseType}", invalidType);
                        }
                    }
                    
                    return configs;
                }
                
                // 创建默认配置
                return await InitializeDefaultConfigsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载配置文件失败");
                return await InitializeDefaultConfigsAsync();
            }
        }

        private async Task<EventListenerConfigs> InitializeDefaultConfigsAsync()
        {
            var defaultConfigs = new EventListenerConfigs
            {
                Databases = new Dictionary<string, EventConfig>(),
                LastUpdated = DateTime.Now
            };

            var databaseTypes = await LoadDatabaseTypesAsync();
            
            if (!databaseTypes.Any())
            {
                _logger.LogWarning("没有可用的数据库类型配置，将使用空配置");
                return defaultConfigs;
            }

            foreach (var dbType in databaseTypes)
            {
                defaultConfigs.Databases[dbType.Key] = GetDefaultConfig();
            }

            await SaveConfigsAsync(defaultConfigs);
            return defaultConfigs;
        }

        private async Task SaveConfigsAsync(EventListenerConfigs configs)
        {
            try
            {
                configs.LastUpdated = DateTime.Now;
                await _dataService.WriteAsync(_configFileName, new List<EventListenerConfigs> { configs });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存配置文件失败");
                throw;
            }
        }

        public async Task<EventListenerConfigs> GetAllConfigsAsync()
        {
            return await GetConfigsAsync();
        }

        public async Task<EventConfig?> GetConfigByTypeAsync(string databaseType)
        {
            var configs = await GetConfigsAsync();
            
            if (configs.Databases.TryGetValue(databaseType, out var config))
            {
                return config;
            }
            
            _logger.LogWarning("未找到数据库类型 {DatabaseType} 的配置", databaseType);
            return null;
        }

        public async Task<EventConfig> UpdateConfigAsync(string databaseType, EventConfig config)
        {
            var configs = await GetConfigsAsync();

            if (!configs.Databases.ContainsKey(databaseType))
            {
                // 验证数据库类型是否有效
                var validTypes = await LoadDatabaseTypesAsync();
                if (!validTypes.ContainsKey(databaseType))
                {
                    throw new ArgumentException($"无效的数据库类型: {databaseType}");
                }
                
                configs.Databases[databaseType] = new EventConfig();
            }

            var existingConfig = configs.Databases[databaseType];
            
            // 更新可配置的字段
            existingConfig.ScanFrequency = config.ScanFrequency;
            existingConfig.BatchSize = config.BatchSize;
            existingConfig.Enabled = config.Enabled;
            existingConfig.TableName = config.TableName;
            existingConfig.PrimaryKey = config.PrimaryKey;
            existingConfig.TimestampField = config.TimestampField;
            
            // 保留以下字段，避免被覆盖
            existingConfig.TotalEventsProcessed = config.TotalEventsProcessed > 0 
                ? config.TotalEventsProcessed 
                : existingConfig.TotalEventsProcessed;
            
            // 如果提供了启动条件，也更新
            if (config.StartCondition != null)
            {
                existingConfig.StartCondition = config.StartCondition;
            }

            await SaveConfigsAsync(configs);
            return existingConfig;
        }

        public async Task<bool> ToggleEnabledAsync(string databaseType, bool enabled)
        {
            var configs = await GetConfigsAsync();
            
            if (configs.Databases.TryGetValue(databaseType, out var config))
            {
                config.Enabled = enabled;
                await SaveConfigsAsync(configs);
                _logger.LogInformation("数据库 {DatabaseType} 的启用状态已更改为: {Enabled}", databaseType, enabled);
                return true;
            }
            
            _logger.LogWarning("尝试切换不存在的数据库类型 {DatabaseType} 的启用状态", databaseType);
            return false;
        }

        public async Task<EventConfig> ResetToDefaultAsync(string databaseType)
        {
            var configs = await GetConfigsAsync();
            
            // 验证数据库类型是否存在
            var validTypes = await LoadDatabaseTypesAsync();
            if (!validTypes.ContainsKey(databaseType))
            {
                throw new ArgumentException($"无效的数据库类型: {databaseType}");
            }
            
            var defaultConfig = GetDefaultConfig();
            
            // 保留初始化相关的数据
            if (configs.Databases.TryGetValue(databaseType, out var existingConfig))
            {
                defaultConfig.TotalEventsProcessed = existingConfig.TotalEventsProcessed;
                configs.Databases[databaseType] = defaultConfig;
            }
            else
            {
                configs.Databases[databaseType] = defaultConfig;
            }
            
            await SaveConfigsAsync(configs);
            _logger.LogInformation("数据库 {DatabaseType} 的配置已重置为默认值", databaseType);
            return defaultConfig;
        }

        private EventConfig GetDefaultConfig()
        {
            return new EventConfig
            {
                ScanFrequency = 60,
                BatchSize = 100,
                Enabled = true,
                TableName = "tblevent",
                PrimaryKey = "Id",
                TimestampField = "CreateDatetime",
                TotalEventsProcessed = 0,
                StartCondition = new StartCondition
                {
                    Type = "time",
                    TimeValue = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm"),
                    IdValue = ""
                }
            };
        }

        public async Task<List<DatabaseTypeInfo>> GetDatabaseTypesAsync()
        {
            var types = new List<DatabaseTypeInfo>();
            var databaseTypes = await LoadDatabaseTypesAsync();
            
            foreach (var type in databaseTypes)
            {
                types.Add(new DatabaseTypeInfo
                {
                    Value = type.Key,
                    Label = type.Value,
                });
            }

            return types;
        }
        
        public void ClearCache()
        {
            _dataService.ClearCache(_configFileName);
            
            // 清空数据库类型缓存
            lock (_databaseTypesLock)
            {
                _databaseTypes.Clear();
            }
        }
        
        public async Task<StartCondition?> GetStartConditionAsync(string databaseType)
        {
            var configs = await GetConfigsAsync();
            
            if (configs.Databases.TryGetValue(databaseType, out var config))
            {
                return config.StartCondition ?? new StartCondition
                {
                    Type = "time",
                    TimeValue = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm"),
                    IdValue = ""
                };
            }
            
            return null;
        }
        
        public async Task<bool> UpdateStartConditionAsync(string databaseType, StartCondition condition)
        {
            var configs = await GetConfigsAsync();
            
            if (configs.Databases.TryGetValue(databaseType, out var config))
            {
                config.StartCondition = condition;
                await SaveConfigsAsync(configs);
                _logger.LogInformation("数据库 {DatabaseType} 的启动条件已更新", databaseType);
                return true;
            }
            
            _logger.LogWarning("尝试更新不存在的数据库类型 {DatabaseType} 的启动条件", databaseType);
            return false;
        }
    }
}