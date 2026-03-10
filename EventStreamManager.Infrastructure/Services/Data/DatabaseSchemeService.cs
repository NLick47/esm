using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services.Data;

public class DatabaseSchemeService : IDatabaseSchemeService
{
    private readonly IDataService _dataService;
    private readonly ILogger<DatabaseSchemeService> _logger;
    private readonly string _configFileName = "database-configs.json";
    

    public DatabaseSchemeService(IDataService dataService, ILogger<DatabaseSchemeService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }


    public async Task<Dictionary<string, List<DatabaseConfig>>> GetAllConfigsAsync()
    {
        try
        {
            var configsList = await _dataService.ReadAsync<DatabaseConfigs>(_configFileName);
            var configs = configsList.FirstOrDefault();

            if (configs != null)
            {
                return configs.Databases;
            }

            return new Dictionary<string, List<DatabaseConfig>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有配置失败");
            return new Dictionary<string, List<DatabaseConfig>>();
        }
    }


    public async Task<List<DatabaseTypeWithActiveConfigDto>> GetAllDatabaseTypesWithActiveConfigAsync()
    {
        try
        {
            var types = await GetAllDatabaseTypesAsync();
            var result = new List<DatabaseTypeWithActiveConfigDto>();

            foreach (var type in types)
            {
                var activeConfig = await GetActiveConfigAsync(type.Value);

                var dto = new DatabaseTypeWithActiveConfigDto
                {
                    Value = type.Value,
                    Label = type.Label,
                    ActiveConfig = activeConfig != null
                        ? new DatabaseConfig()
                        {
                            Id = activeConfig.Id,
                            Name = activeConfig.Name,
                            ConnectionString = activeConfig.ConnectionString,
                            Driver = activeConfig.Driver,
                            Timeout = activeConfig.Timeout,
                            IsActive = activeConfig.IsActive
                        }
                        : null
                };

                result.Add(dto);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取所有数据库类型及其激活配置失败");
            return new List<DatabaseTypeWithActiveConfigDto>();
        }
    }

    public async Task<List<DatabaseConfig>> GetConfigsByTypeAsync(string databaseType)
    {
        try
        {
            var allConfigs = await GetAllConfigsAsync();

            if (!allConfigs.ContainsKey(databaseType))
            {
                allConfigs[databaseType] = new List<DatabaseConfig>();
                await SaveAllConfigsAsync(allConfigs);
            }

            return allConfigs[databaseType];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取{DatabaseType}配置失败", databaseType);
            return new List<DatabaseConfig>();
        }
    }

    public async Task<DatabaseConfig?> GetConfigByIdAsync(string databaseType, string id)
    {
        try
        {
            var configs = await GetConfigsByTypeAsync(databaseType);
            return configs.FirstOrDefault(c => c.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
            return null;
        }
    }

    public async Task<DatabaseConfig> AddConfigAsync(string databaseType, DatabaseConfig config)
    {
        try
        {
            var allConfigs = await GetAllConfigsAsync();

            if (!allConfigs.ContainsKey(databaseType))
            {
                allConfigs[databaseType] = new List<DatabaseConfig>();
            }

            config.Id = Guid.NewGuid().ToString();
            allConfigs[databaseType].Add(config);

            await SaveAllConfigsAsync(allConfigs);

            _logger.LogInformation("添加配置成功 - Type: {DatabaseType}, Id: {Id}", databaseType, config.Id);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加配置失败 - Type: {DatabaseType}", databaseType);
            throw;
        }
    }

    public async Task<DatabaseConfig?> UpdateConfigAsync(string databaseType, string id, DatabaseConfig config)
    {
        try
        {
            var allConfigs = await GetAllConfigsAsync();

            if (!allConfigs.ContainsKey(databaseType)) return null;

            var existingConfig = allConfigs[databaseType].FirstOrDefault(c => c.Id == id);
            if (existingConfig == null) return null;

            // 只更新允许修改的字段，保留原有的IsActive状态
            existingConfig.Name = config.Name;
            existingConfig.ConnectionString = config.ConnectionString;
            existingConfig.Driver = config.Driver;
            existingConfig.Timeout = config.Timeout;

            await SaveAllConfigsAsync(allConfigs);

            _logger.LogInformation("更新配置成功 - Type: {DatabaseType}, Id: {Id}", databaseType, id);

            return existingConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
            throw;
        }
    }

    public async Task<bool> DeleteConfigAsync(string databaseType, string id)
    {
        try
        {
            var allConfigs = await GetAllConfigsAsync();
            if (!allConfigs.ContainsKey(databaseType)) return false;

            var configs = allConfigs[databaseType];
            if (configs.Count <= 1) return false; // 至少保留一个配置

            var configToDelete = configs.FirstOrDefault(c => c.Id == id);
            if (configToDelete == null) return false;

            bool wasActive = configToDelete.IsActive;
            configs.Remove(configToDelete);

            // 如果删除的是激活配置，自动将第一个设为激活
            if (wasActive && configs.Any())
            {
                configs.First().IsActive = true;
            }

            await SaveAllConfigsAsync(allConfigs);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
            return false;
        }
    }

    public async Task<DatabaseConfig?> GetActiveConfigAsync(string databaseType)
    {
        try
        {
            var configs = await GetConfigsByTypeAsync(databaseType);
            return configs.FirstOrDefault(c => c.IsActive) ?? configs.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取激活配置失败 - Type: {DatabaseType}", databaseType);
            return null;
        }
    }

    public async Task<bool> SetActiveConfigAsync(string databaseType, string id)
    {
        try
        {
            var allConfigs = await GetAllConfigsAsync();
            if (!allConfigs.ContainsKey(databaseType)) return false;

            var configs = allConfigs[databaseType];
            var targetConfig = configs.FirstOrDefault(c => c.Id == id);

            if (targetConfig == null) return false;

            foreach (var config in configs)
            {
                config.IsActive = config.Id == id;
            }

            await SaveAllConfigsAsync(allConfigs);
            _logger.LogInformation("设置激活配置成功 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设置激活配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
            return false;
        }
    }


    public async Task<List<DatabaseType>> GetAllDatabaseTypesAsync()
    {
        try
        {
            var configsList = await _dataService.ReadAsync<DatabaseConfigs>(_configFileName);
            var configs = configsList.FirstOrDefault();

            return configs?.DatabaseTypes ?? new List<DatabaseType>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取数据库类型失败");
            return new List<DatabaseType>();
        }
    }

    public async Task<DatabaseType> AddDatabaseTypeAsync(DatabaseType databaseType)
    {
        try
        {
            var configsList = await _dataService.ReadAsync<DatabaseConfigs>(_configFileName);
            var configs = configsList.FirstOrDefault() ?? new DatabaseConfigs();

            if (configs.DatabaseTypes.Any(t => t.Value == databaseType.Value))
            {
                throw new InvalidOperationException("该类型标识已存在");
            }

            configs.DatabaseTypes.Add(databaseType);

            if (!configs.Databases.ContainsKey(databaseType.Value))
            {
                configs.Databases[databaseType.Value] = new List<DatabaseConfig>();
            }

            await _dataService.WriteAsync(_configFileName, new List<DatabaseConfigs> { configs });

            _logger.LogInformation("添加数据库类型成功 - Value: {Value}", databaseType.Value);

            return databaseType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "添加数据库类型失败");
            throw;
        }
    }

    public async Task<bool> DeleteDatabaseTypeAsync(string typeValue)
    {
        try
        {
            var configsList = await _dataService.ReadAsync<DatabaseConfigs>(_configFileName);
            var configs = configsList.FirstOrDefault();

            if (configs?.DatabaseTypes == null || configs.DatabaseTypes.Count <= 1) return false;

            var typeToDelete = configs.DatabaseTypes.FirstOrDefault(t => t.Value == typeValue);
            if (typeToDelete == null) return false;

            configs.DatabaseTypes.Remove(typeToDelete);
            configs.Databases.Remove(typeValue);

            await _dataService.WriteAsync(_configFileName, new List<DatabaseConfigs> { configs });

            _logger.LogInformation("删除数据库类型成功 - Value: {Value}", typeValue);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除数据库类型失败 - Value: {Value}", typeValue);
            return false;
        }
    }


    private async Task SaveAllConfigsAsync(Dictionary<string, List<DatabaseConfig>> databases)
    {
        var configsList = await _dataService.ReadAsync<DatabaseConfigs>(_configFileName);
        var existingConfigs = configsList.FirstOrDefault();

        var configs = new DatabaseConfigs
        {
            Databases = databases,
            DatabaseTypes = existingConfigs?.DatabaseTypes ?? new List<DatabaseType>()
        };

        await _dataService.WriteAsync(_configFileName, new List<DatabaseConfigs> { configs });
    }

    
}