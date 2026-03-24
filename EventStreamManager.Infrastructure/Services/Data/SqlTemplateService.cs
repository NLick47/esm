using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services.Data;

public class SqlTemplateService : ISqlTemplateService
{
    private readonly IDataService _dataService;
    private readonly ILogger<SqlTemplateService> _logger;
    
   
    private const string SystemFile = "systemtemplates.json";
    private const string CustomFile = "customtemplates.json";

    public SqlTemplateService(
        IDataService dataService,
        ILogger<SqlTemplateService> logger)
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
        template.CreatedAt = DateTime.UtcNow;
        template.UpdatedAt = null;
        
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        list.Add(template);
        await _dataService.WriteAsync(CustomFile, list);
        
        _logger.LogInformation($"创建自定义SQL模板: {template.Name} (Id: {template.Id})");
        return template;
    }
    
    
   

    public async Task<bool> UpdateCustomAsync(string id, CustomSqlTemplate template)
    {
        
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        var index = list.FindIndex(x => x.Id == id);
        if (index == -1) return false;
        
        // 保留原始创建时间
        template.Id = id;
        template.CreatedAt = list[index].CreatedAt;
        template.UpdatedAt = DateTime.UtcNow;
        
        list[index] = template;
        await _dataService.WriteAsync(CustomFile, list);
        
        _logger.LogInformation($"更新自定义SQL模板: {template.Name} (Id: {id})");
        return true;
    }
    
    
    public async Task<string?> GetCustomSqlAsync(string templateId, Dictionary<string, object> parameters)
    {
        var templates = await GetCustomTemplatesAsync();
        var template = templates.FirstOrDefault(x => x.Id == templateId);
        if (template == null) return null;
        
        return ReplaceSqlParameters(template.SqlTemplate, parameters);
    }
    
    
    private string ReplaceSqlParameters(string sqlTemplate, Dictionary<string, object> parameters)
    {
        var result = sqlTemplate;
        foreach (var param in parameters)
        {
            var placeholder = $"${{{param.Key}}}";
            var value = param.Value.ToString() ?? string.Empty;
            
          
            if (param.Value is string)
            {
                value = value.Replace("'", "''"); 
            }
            
            result = result.Replace(placeholder, value);
        }
        return result;
    }

    
    

    public async Task<bool> DeleteCustomAsync(string id)
    {
        var list = await _dataService.ReadAsync<CustomSqlTemplate>(CustomFile);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) return false;
        
        var newList = list.Where(x => x.Id != id).ToList();
        await _dataService.WriteAsync(CustomFile, newList);
        
        _logger.LogInformation($"删除自定义SQL模板: {item.Name} (Id: {id})");
        return true;
    }
}