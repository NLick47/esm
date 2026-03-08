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
        
        private readonly Dictionary<string, string> _databaseTypes = new()
        {
            ["ultrasound"] = "超声数据库",
            ["radiology"] = "放射数据库",
            ["endoscopy"] = "内镜数据库"
        };

        public EventListenerConfigService(IDataService dataService, ILogger<EventListenerConfigService> logger)
        {
            _dataService = dataService;
            _logger = logger;
        }

        private async Task<EventListenerConfigs> GetConfigsAsync()
        {
            try
            {
                var configsList = await _dataService.ReadAsync<EventListenerConfigs>(_configFileName);
                var configs = configsList.FirstOrDefault();
                
                if (configs != null && configs.Databases != null && configs.Databases.Any())
                {
                    return configs;
                }
                
                // 如果没有配置，创建默认配置
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
                Databases = new Dictionary<string, EventConfig>
                {
                    ["ultrasound"] = new EventConfig
                    {
                        ScanFrequency = 60,
                        BatchSize = 100,
                        Enabled = false,
                        TableName = "tblevent",
                        PrimaryKey = "Id",
                        TimestampField = "CreateDatetime",
                        TotalEventsProcessed = 0
                    },
                    ["radiology"] = new EventConfig
                    {
                        ScanFrequency = 60,
                        BatchSize = 100,
                        Enabled = false,
                        TableName = "tblevent",
                        PrimaryKey = "Id",
                        TimestampField = "CreateDatetime",
                        TotalEventsProcessed = 0
                    },
                    ["endoscopy"] = new EventConfig
                    {
                        ScanFrequency = 60,
                        BatchSize = 100,
                        Enabled = false,
                        TableName = "tblevent",
                        PrimaryKey = "Id",
                        TimestampField = "CreateDatetime",
                        TotalEventsProcessed = 0
                    }
                },
                LastUpdated = DateTime.Now
            };

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
            
            if (configs.Databases.ContainsKey(databaseType))
            {
                return configs.Databases[databaseType];
            }
            return null;
        }

        public async Task<EventConfig> UpdateConfigAsync(string databaseType, EventConfig config)
        {
            var configs = await GetConfigsAsync();

            if (!configs.Databases.ContainsKey(databaseType))
            {
                configs.Databases[databaseType] = new EventConfig();
            }

            var existingConfig = configs.Databases[databaseType];
            existingConfig.ScanFrequency = config.ScanFrequency;
            existingConfig.BatchSize = config.BatchSize;
            existingConfig.Enabled = config.Enabled;
            existingConfig.TableName = config.TableName;
            existingConfig.PrimaryKey = config.PrimaryKey;
            existingConfig.TimestampField = config.TimestampField;
            // 保留以下字段，避免被覆盖
         
            existingConfig.TotalEventsProcessed = config.TotalEventsProcessed > 0 ? config.TotalEventsProcessed : existingConfig.TotalEventsProcessed;

            await SaveConfigsAsync(configs);
            return existingConfig;
        }

        public async Task<bool> ToggleEnabledAsync(string databaseType, bool enabled)
        {
            var configs = await GetConfigsAsync();
            
            if (configs.Databases.ContainsKey(databaseType))
            {
                configs.Databases[databaseType].Enabled = enabled;
                await SaveConfigsAsync(configs);
                return true;
            }
            return false;
        }

        public async Task<EventConfig> ResetToDefaultAsync(string databaseType)
        {
            var configs = await GetConfigsAsync();
            var defaultConfig = GetDefaultConfig(databaseType);
            
            // 保留初始化相关的数据
            if (configs.Databases.ContainsKey(databaseType))
            {
                var existingConfig = configs.Databases[databaseType];
                defaultConfig.TotalEventsProcessed = existingConfig.TotalEventsProcessed;
                
                configs.Databases[databaseType] = defaultConfig;
            }
            else
            {
                configs.Databases[databaseType] = defaultConfig;
            }
            
            await SaveConfigsAsync(configs);
            return defaultConfig;
        }

        private EventConfig GetDefaultConfig(string databaseType)
        {
            return databaseType switch
            {
                "ultrasound" => new EventConfig
                {
                    ScanFrequency = 60,
                    BatchSize = 50,
                    Enabled = true,
                    TableName = "tblevent",
                    PrimaryKey = "Id",
                    TimestampField = "CreateDatetime"
                },
                "radiology" => new EventConfig
                {
                    ScanFrequency = 30,
                    BatchSize = 100,
                    Enabled = false,
                    TableName = "tblevent",
                    PrimaryKey = "Id",
                    TimestampField = "CreateDatetime"
                },
                "endoscopy" => new EventConfig
                {
                    ScanFrequency = 45,
                    BatchSize = 75,
                    Enabled = true,
                    TableName = "tblevent",
                    PrimaryKey = "Id",
                    TimestampField = "CreateDatetime"
                },
                _ => new EventConfig()
            };
        }

      

     

        public async Task<List<DatabaseTypeInfo>> GetDatabaseTypesAsync()
        {
            var types = new List<DatabaseTypeInfo>();
            
            foreach (var type in _databaseTypes)
            {
                types.Add(new DatabaseTypeInfo
                {
                    Value = type.Key,
                    Label = type.Value,
                });
            }

            return await Task.FromResult(types);
        }

      
        public void ClearCache()
        {
            _dataService.ClearCache(_configFileName);
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
                return true;
            }
            
            return false;
        }
    }
}