using System.Text.RegularExpressions;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services.Data;

public class SqlTemplateService : ISqlTemplateService
{
    private static readonly Regex SqlInjectionPattern = new Regex(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|GRANT|REVOKE|EXEC|EXECUTE|MERGE|REPLACE)\b|\-\-|;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    private readonly IDataService _dataService;
    private readonly ILogger<SqlTemplateService> _logger;
    private const string SystemFile = "systemtemplates.json";
    private const string CustomFile = "customtemplates.json";

    public SqlTemplateService(IDataService dataService,ILogger<SqlTemplateService> logger)
    {
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<List<SystemSqlTemplate>> GetSystemTemplatesAsync()
    {
        return await _dataService.ReadTemplateAsync<SystemSqlTemplate>(SystemFile);
    }

    public async Task<List<CustomSqlTemplate>> GetCustomTemplatesAsync()
    {
        return await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
    }

    public async Task<CustomSqlTemplate> CreateCustomAsync(CustomSqlTemplate template)
    {
        template.Id = Guid.NewGuid().ToString();
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        list.Add(template);
        await _dataService.WriteAsync(CustomFile, list);
        return template;
    }
    
    
    /// <summary>
    /// 验证SQL是否为安全的查询语句
    /// </summary>
    private async Task ValidateSqlQueryAsync(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            _logger.LogWarning($"sql 为空");
            throw new ArgumentNullException(nameof(sql));
        }

        if (SqlInjectionPattern.IsMatch(sql))
        {
            _logger.LogError($"{sql} 存在注入问题");
            throw new InvalidOperationException("SQL包含潜在的危险操作或注入风险");
        }
        
        var trimmedSql = sql.TrimStart();
        if (!trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) && 
            !trimmedSql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("只允许SELECT查询语句");

    }

    public async Task<bool> UpdateCustomAsync(string id, CustomSqlTemplate template)
    {
        await ValidateSqlQueryAsync(template.SqlTemplate);
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        var index = list.FindIndex(x => x.Id == id);
        if (index == -1) return false;
        
        template.Id = id;
        list[index] = template;
       
        await _dataService.WriteAsync(CustomFile, list);
        return true;
    }

    public async Task<bool> DeleteCustomAsync(string id)
    {
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        var newList = list.Where(x => x.Id != id).ToList();
        if (newList.Count == list.Count) return false;
        
        await _dataService.WriteAsync(CustomFile, newList);
        return true;
    }
}