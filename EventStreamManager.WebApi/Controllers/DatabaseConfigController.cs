// WebApi/Controllers/DatabaseConfigController.cs
using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseConfigController : BaseController
    {
        private readonly IDatabaseSchemeService _databaseSchemeService;
        private readonly ITableInitializationService _tableInitializationService;
        private readonly IDatabaseConnectionService _connectionService;
        private readonly IDataService _dataService;
        private readonly ILogger<DatabaseConfigController> _logger;

        public DatabaseConfigController(
            IDatabaseSchemeService databaseSchemeService,
            IDatabaseConnectionService connectionService,
            ITableInitializationService tableInitializationService,
            IDataService dataService,
            ILogger<DatabaseConfigController> logger)
        {
            _databaseSchemeService = databaseSchemeService;
            _connectionService = connectionService;
            _tableInitializationService = tableInitializationService;
            _dataService = dataService;
            _logger = logger;
        }

        // 获取所有数据库配置
        [HttpGet]
        public async Task<IActionResult> GetAllConfigs()
        {
            var configs = await _databaseSchemeService.GetAllConfigsAsync();
            return Ok(configs, "获取所有配置成功");
        }

        // 获取指定类型的数据库配置
        [HttpGet("{databaseType}")]
        public async Task<IActionResult> GetConfigsByType(string databaseType)
        {
            var configs = await _databaseSchemeService.GetConfigsByTypeAsync(databaseType);
            return Ok(configs, $"获取{databaseType}配置成功");
        }

        // 获取指定ID的配置
        [HttpGet("{databaseType}/{id}")]
        public async Task<IActionResult> GetConfigById(string databaseType, string id)
        {
            var config = await _databaseSchemeService.GetConfigByIdAsync(databaseType, id);
            if (config == null)
            {
                return Fail("配置不存在", 404);
            }
            return Ok(config, "获取配置成功");
        }

        // 创建新配置
        [HttpPost("{databaseType}")]
        public async Task<IActionResult> CreateConfig(string databaseType, [FromBody] DatabaseConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.Name))
            {
                return Fail("配置名称不能为空");
            }

            var newConfig = await _databaseSchemeService.AddConfigAsync(databaseType, config);
            return Ok(newConfig, "创建配置成功");
        }

        // 更新配置
        [HttpPut("{databaseType}/{id}")]
        public async Task<IActionResult> UpdateConfig(string databaseType, string id, [FromBody] DatabaseConfig config)
        {
            var updatedConfig = await _databaseSchemeService.UpdateConfigAsync(databaseType, id, config);
            if (updatedConfig == null)
            {
                return Fail("配置不存在", 404);
            }
            return Ok(updatedConfig, "更新配置成功");
        }

        // 删除配置
        [HttpDelete("{databaseType}/{id}")]
        public async Task<IActionResult> DeleteConfig(string databaseType, string id)
        {
            var deleted = await _databaseSchemeService.DeleteConfigAsync(databaseType, id);
            if (!deleted)
            {
                return Fail("删除失败：至少需要保留一个配置或配置不存在");
            }
            return OkMessage("删除成功");
        }

        // 测试连接
        [HttpPost("test-connection")]
        public async Task<IActionResult> TestConnection([FromBody] ConnectionTestRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ConnectionString))
            {
                return Fail("连接字符串不能为空");
            }

            var result = await _connectionService.TestConnectionAsync(request);
            return Ok(result, "连接测试完成");
        }

        // 获取所有数据库类型
        [HttpGet("types")]
        public async Task<IActionResult> GetDatabaseTypes()
        {
            var types = await _databaseSchemeService.GetAllDatabaseTypesAsync();
            return Ok(types, "获取数据库类型成功");
        }
        
        // 获取带有激活配置的数据库类型列表
        [HttpGet("types-with-active-config")]
        public async Task<IActionResult> GetDatabaseTypesWithActiveConfig()
        {
            var result = await _databaseSchemeService.GetAllDatabaseTypesWithActiveConfigAsync();
            return Ok(result, "获取带有激活配置的数据库类型成功");
        }

        // 添加新数据库类型
        [HttpPost("types")]
        public async Task<IActionResult> AddDatabaseType([FromBody] DatabaseType databaseType)
        {
            if (string.IsNullOrWhiteSpace(databaseType.Value) || string.IsNullOrWhiteSpace(databaseType.Label))
            {
                return Fail("类型标识和显示名称不能为空");
            }

            var newType = await _databaseSchemeService.AddDatabaseTypeAsync(databaseType);
            return Ok(newType, "添加数据库类型成功");
        }

        // 删除数据库类型
        [HttpDelete("types/{typeValue}")]
        public async Task<IActionResult> DeleteDatabaseType(string typeValue)
        {
            var deleted = await _databaseSchemeService.DeleteDatabaseTypeAsync(typeValue);
            if (!deleted)
            {
                return Fail("删除失败：至少需要保留一个类型或类型不存在");
            }
            return OkMessage("数据库类型删除成功");
        }

        // 获取所有连接字符串示例
        [HttpGet("connection-examples")]
        public async Task<IActionResult> GetAllConnectionExamples()
        {
            var examples = await _dataService.ReadTemplateSingleAsync<Dictionary<string, string>>("connection-examples.json");
            if (examples == null)
            {
                return Fail("连接示例模板文件未找到");
            }
            return Ok(examples, "获取连接示例成功");
        }

        // 获取连接字符串示例
        [HttpGet("connection-examples/{driver}")]
        public async Task<IActionResult> GetConnectionStringExample(string driver)
        {
            var examples = await _dataService.ReadTemplateSingleAsync<Dictionary<string, string>>("connection-examples.json");
            if (examples == null)
            {
                return Fail("连接示例模板文件未找到");
            }

            if (examples.TryGetValue(driver, out var example))
            {
                return Ok(new { driver, example }, "获取连接示例成功");
            }

            return Ok(new { driver, example = "Server=localhost;Database=mydb;User Id=user;Password=pass;" }, "获取连接示例成功");
        }

        // 获取当前激活的配置
        [HttpGet("{databaseType}/active")]
        public async Task<IActionResult> GetActiveConfig(string databaseType)
        {
            var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(databaseType);
            return Ok(activeConfig, "获取激活配置成功");
        }

        // 设置为当前使用的配置
        [HttpPost("{databaseType}/{id}/activate")]
        public async Task<IActionResult> SetActiveConfig(string databaseType, string id)
        {
            var success = await _databaseSchemeService.SetActiveConfigAsync(databaseType, id);
            if (!success)
            {
                return Fail("配置不存在", 404);
            }
            return OkMessage("已设置为当前使用的配置");
        }
        
        [HttpPost("{databaseType}/{id}/initialize-tables")]
        public async Task<IActionResult> InitializeTables(string databaseType, string id)
        {
            _logger.LogInformation("开始初始化表结构 - 数据库类型: {DatabaseType}, 配置ID: {Id}", databaseType, id);

            var config = await _databaseSchemeService.GetConfigByIdAsync(databaseType, id);
            if (config == null)
            {
                _logger.LogWarning("配置不存在 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
                return Fail("配置不存在", 404);
            }
            
            var response = await _tableInitializationService.InitializeTablesAsync(config);
            
            if (response.Success)
            {
                return Ok(response, "表结构初始化成功");
            }

            return Error(response.Message, 500, response);
        }
    }
}