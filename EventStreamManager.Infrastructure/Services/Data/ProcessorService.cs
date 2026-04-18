using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class ProcessorService : IProcessorService
{
    private readonly IDataService _dataService;
    
    private readonly ISqlTemplateService _sqlTemplateService;
   
    private const string FileName = "processors.json";

    public ProcessorService(IDataService dataService, ISqlTemplateService sqlTemplateService)
    {
        _dataService = dataService;
        _sqlTemplateService = sqlTemplateService;
    }

    public async Task<List<JsProcessor>> GetAllAsync()
    {
        return await _dataService.ReadAsync<JsProcessor>(FileName);
    }

    public async Task<JsProcessor?> GetByIdAsync(string id)
    {
        var list = await _dataService.ReadAsync<JsProcessor>(FileName);
        var processor = list.FirstOrDefault(x => x.Id == id);
    
        if (processor != null && !string.IsNullOrEmpty(processor.SqlTemplateId))
        {
            // 根据模板类型获取SQL模板内容
            if (processor.SqlTemplateType == SqlTemplateType.System)
            {
                var systemTemplates = await _sqlTemplateService.GetSystemTemplatesAsync();
                var template = systemTemplates.FirstOrDefault(t => t.Id == processor.SqlTemplateId);
                if (template != null)
                {
                    processor.SqlTemplate = template.SqlTemplate;
                }
            }
            else if (processor.SqlTemplateType == SqlTemplateType.Custom)
            {
                var customTemplates = await _sqlTemplateService.GetCustomTemplatesAsync();
                var template = customTemplates.FirstOrDefault(t => t.Id == processor.SqlTemplateId);
                if (template != null)
                {
                    processor.SqlTemplate = template.SqlTemplate;
                }
            }
        }
    
        return processor;
    }

    public async Task<JsProcessor> CreateAsync(JsProcessor processor)
    {
        processor.Id = Guid.NewGuid().ToString();
        var list = await _dataService.ReadAsync<JsProcessor>(FileName);
        list.Add(processor);
        await _dataService.WriteAsync(FileName, list);
        return processor;
    }

    public async Task<bool> UpdateAsync(string id, JsProcessor processor)
    {
        var list = await _dataService.ReadAsync<JsProcessor>(FileName);
        var index = list.FindIndex(x => x.Id == id);
        if (index == -1) return false;
        
        processor.Id = id;
        list[index] = processor;
        await _dataService.WriteAsync(FileName, list);
        return true;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var list = await _dataService.ReadAsync<JsProcessor>(FileName);
        var newList = list.Where(x => x.Id != id).ToList();
        if (newList.Count == list.Count) return false;
        
        await _dataService.WriteAsync(FileName, newList);
        return true;
    }

    public async Task<JsProcessor?> ToggleAsync(string id)
    {
        var list = await _dataService.ReadAsync<JsProcessor>(FileName);
        var item = list.FirstOrDefault(x => x.Id == id);
        if (item == null) return null;
        
        item.Enabled = !item.Enabled;
        await _dataService.WriteAsync(FileName, list);
        return item;
    }

    public async Task<string> GetDefaultTemplateAsync()
    {
        try
        {
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "DefaultProcessor.js");
            
            if (!File.Exists(templatePath))
            {
                return GetDefaultTemplateCode();
            }
            
            return await File.ReadAllTextAsync(templatePath);
        }
        catch
        {
            return GetDefaultTemplateCode();
        }
    }

    private string GetDefaultTemplateCode()
    {
        return @"class ProcessResult {
  constructor() {
    this.needToSend = true;
    this.reason = '';
    this.error = null;
    this.requestInfo = null;
  }
  
  setSuccess(requestInfo) {
    this.needToSend = true;
    this.reason = '';
    this.error = null;
    this.requestInfo = requestInfo;
    return this;
  }
  
  setFailure(reason, error = null) {
    this.needToSend = false;
    this.reason = reason;
    this.error = error;
    this.requestInfo = null;
    return this;
  }
  
  setNoSend(reason = '') {
    this.needToSend = false;
    this.reason = reason || '仅执行脚本，无需发送';
    this.error = null;
    this.requestInfo = null;
    return this;
  }
}

function process(data) {
  const result = new ProcessResult();
  console_log('收到数据:', data);
  return result.setFailure('未处理数据');
}";
    }
}