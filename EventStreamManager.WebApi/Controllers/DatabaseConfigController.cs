using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseConfigController : ControllerBase
    {
        private readonly IDatabaseSchemeService _databaseSchemeService;
        private readonly IDatabaseConnectionService _connectionService;
        private readonly ILogger<DatabaseConfigController> _logger;

        public DatabaseConfigController(
            IDatabaseSchemeService databaseSchemeService,
            IDatabaseConnectionService connectionService,
            ILogger<DatabaseConfigController> logger)
        {
            _databaseSchemeService = databaseSchemeService;
            _connectionService = connectionService;
            _logger = logger;
        }

        // 获取所有数据库配置
        [HttpGet]
        public async Task<ActionResult<Dictionary<string, List<DatabaseConfig>>>> GetAllConfigs()
        {
            try
            {
                var configs = await _databaseSchemeService.GetAllConfigsAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有配置失败");
                return StatusCode(500, new { message = "获取配置失败", error = ex.Message });
            }
        }

        // 获取指定类型的数据库配置
        [HttpGet("{databaseType}")]
        public async Task<ActionResult<List<DatabaseConfig>>> GetConfigsByType(string databaseType)
        {
            try
            {
                var configs = await _databaseSchemeService.GetConfigsByTypeAsync(databaseType);
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取{DatabaseType}配置失败", databaseType);
                return StatusCode(500, new { message = "获取配置失败", error = ex.Message });
            }
        }

        // 获取指定ID的配置
        [HttpGet("{databaseType}/{id}")]
        public async Task<ActionResult<DatabaseConfig>> GetConfigById(string databaseType, string id)
        {
            try
            {
                var config = await _databaseSchemeService.GetConfigByIdAsync(databaseType, id);
                if (config == null)
                {
                    return NotFound(new { message = "配置不存在" });
                }
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
                return StatusCode(500, new { message = "获取配置失败", error = ex.Message });
            }
        }

        // 创建新配置
        [HttpPost("{databaseType}")]
        public async Task<ActionResult<DatabaseConfig>> CreateConfig(string databaseType, [FromBody] DatabaseConfig config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(config.Name))
                {
                    return BadRequest(new { message = "配置名称不能为空" });
                }

                var newConfig = await _databaseSchemeService.AddConfigAsync(databaseType, config);
                return CreatedAtAction(nameof(GetConfigById), new { databaseType, id = newConfig.Id }, newConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建配置失败 - Type: {DatabaseType}", databaseType);
                return StatusCode(500, new { message = "创建配置失败", error = ex.Message });
            }
        }

        // 更新配置
        [HttpPut("{databaseType}/{id}")]
        public async Task<IActionResult> UpdateConfig(string databaseType, string id, [FromBody] DatabaseConfig config)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedConfig = await _databaseSchemeService.UpdateConfigAsync(databaseType, id, config);
                if (updatedConfig == null)
                {
                    return NotFound(new { message = "配置不存在" });
                }
                return Ok(updatedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
                return StatusCode(500, new { message = "更新配置失败", error = ex.Message });
            }
        }

        // 删除配置
        [HttpDelete("{databaseType}/{id}")]
        public async Task<IActionResult> DeleteConfig(string databaseType, string id)
        {
            try
            {
                var deleted = await _databaseSchemeService.DeleteConfigAsync(databaseType, id);
                if (!deleted)
                {
                    return BadRequest(new { message = "删除失败：至少需要保留一个配置或配置不存在" });
                }
                return Ok(new { message = "删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除配置失败 - Type: {DatabaseType}, Id: {Id}", databaseType, id);
                return StatusCode(500, new { message = "删除配置失败", error = ex.Message });
            }
        }

        // 测试连接
        [HttpPost("test-connection")]
        public async Task<ActionResult<ConnectionTestResponse>> TestConnection([FromBody] ConnectionTestRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(request.ConnectionString))
                {
                    return BadRequest(new { message = "连接字符串不能为空" });
                }

                var result = await _connectionService.TestConnectionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "测试连接失败");
                return StatusCode(500, new { message = "测试连接失败", error = ex.Message });
            }
        }

        // 获取所有数据库类型
        [HttpGet("types")]
        public async Task<IActionResult> GetDatabaseTypes()
        {
            try
            {
                var types = await _databaseSchemeService.GetAllDatabaseTypesAsync();
                return Ok(types);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取数据库类型失败");
                return StatusCode(500, new { message = "获取数据库类型失败", error = ex.Message });
            }
        }
        
        //获取带有激活配置的数据库类型列表
        [HttpGet("types-with-active-config")]
        public async Task<IActionResult> GetDatabaseTypesWithActiveConfig()
        {
            try
            {
                var result = await _databaseSchemeService.GetAllDatabaseTypesWithActiveConfigAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取带有激活配置的数据库类型失败");
                return StatusCode(500, new { message = "获取数据库类型失败", error = ex.Message });
            }
        }

        // 添加新数据库类型
        [HttpPost("types")]
        public async Task<IActionResult> AddDatabaseType([FromBody] DatabaseType databaseType)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(databaseType.Value) || string.IsNullOrWhiteSpace(databaseType.Label))
                {
                    return BadRequest(new { message = "类型标识和显示名称不能为空" });
                }

                var newType = await _databaseSchemeService.AddDatabaseTypeAsync(databaseType);
                return Ok(newType);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加数据库类型失败");
                return StatusCode(500, new { message = "添加数据库类型失败", error = ex.Message });
            }
        }

        // 删除数据库类型
        [HttpDelete("types/{typeValue}")]
        public async Task<IActionResult> DeleteDatabaseType(string typeValue)
        {
            try
            {
                var deleted = await _databaseSchemeService.DeleteDatabaseTypeAsync(typeValue);
                if (!deleted)
                {
                    return BadRequest(new { message = "删除失败：至少需要保留一个类型或类型不存在" });
                }
                return Ok(new { message = "数据库类型删除成功" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除数据库类型失败 - Value: {TypeValue}", typeValue);
                return StatusCode(500, new { message = "删除数据库类型失败", error = ex.Message });
            }
        }

        // 获取连接字符串示例
        [HttpGet("connection-examples/{driver}")]
        public IActionResult GetConnectionStringExample(string driver)
        {
            var examples = new Dictionary<string, string>
            {
                ["SQL Server"] = "Server=localhost,1433;Database=mydb;User Id=sa;Password=123456;TrustServerCertificate=true;",
                ["MySQL"] = "Server=localhost;Port=3306;Database=mydb;Uid=root;Pwd=123456;",
                ["PostgreSQL"] = "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=123456;",
                ["Oracle"] = "Data Source=localhost:1521/ORCL;User Id=system;Password=123456;"
            };

            if (examples.ContainsKey(driver))
            {
                return Ok(new { driver, example = examples[driver] });
            }

            return Ok(new { driver, example = "Server=localhost;Database=mydb;User Id=user;Password=pass;" });
        }

        // 获取当前激活的配置
        [HttpGet("{databaseType}/active")]
        public async Task<ActionResult<DatabaseConfig>> GetActiveConfig(string databaseType)
        {
            try
            {
                var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(databaseType);
                if (activeConfig == null)
                {
                    return Ok(null);
                }
                return Ok(activeConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取{DatabaseType}激活配置失败", databaseType);
                return StatusCode(500, new { message = "获取激活配置失败", error = ex.Message });
            }
        }

        // 设置为当前使用的配置
        [HttpPost("{databaseType}/{id}/set-active")]
        public async Task<IActionResult> SetActiveConfig(string databaseType, string id)
        {
            try
            {
                var success = await _databaseSchemeService.SetActiveConfigAsync(databaseType, id);
                if (!success)
                {
                    return NotFound(new { message = "配置不存在" });
                }
                return Ok(new { message = "已设置为当前使用的配置" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置{DatabaseType}激活配置失败 - Id: {Id}", databaseType, id);
                return StatusCode(500, new { message = "设置激活配置失败", error = ex.Message });
            }
        }
    }
}