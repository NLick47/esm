using System.Text.Json;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class JsonDataService : IDataService
{
    private readonly string _dataPath;
    private readonly string _templatesPath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, object> _memoryCache = new();
    
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true, 
        WriteIndented = true             
    };

  

    public JsonDataService()
    {
        var path = "Data";
        var templatesPath = "Templates";

        _dataPath = Path.Combine(Directory.GetCurrentDirectory(), path);
        _templatesPath = Path.Combine(Directory.GetCurrentDirectory(), templatesPath);
        if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_dataPath);
        if (!Directory.Exists(_dataPath)) Directory.CreateDirectory(_templatesPath);
    }
    
    public async Task<List<T>> ReadAsync<T>(string fileName)
    {
        await _lock.WaitAsync();
        try
        {
            // 检查内存中是否已有该文件的数据
            if (_memoryCache.TryGetValue(fileName, out var cachedData))
            {
                return cachedData as List<T> ?? new List<T>();
            }
            
            // 如果内存中没有，从文件读取
            var filePath = Path.Combine(_dataPath, fileName);
            if (!File.Exists(filePath))
            {
                var emptyList = new List<T>();
                _memoryCache[fileName] = emptyList;
                return emptyList;
            }
            
            var json = await File.ReadAllTextAsync(filePath);
            var data = JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();

            _memoryCache[fileName] = data;
            return data;
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task WriteAsync<T>(string fileName, List<T> data)
    {
        await _lock.WaitAsync();
        try
        {
            // 更新内存缓存
            _memoryCache[fileName] = data;
            
            // 写入文件
            var filePath = Path.Combine(_dataPath, fileName);
            var json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(filePath, json);
        }
        finally
        {
            _lock.Release();
        }
    }

  
    public void ClearCache(string? fileName = null)
    {
        _lock.Wait();
        try
        {
            if (fileName == null)
            {
                _memoryCache.Clear();
            }
            else
            {
                _memoryCache.Remove(fileName);
            }
        }
        finally
        {
            _lock.Release();
        }
    }
    
    
    public async Task<List<T>> ReadTemplateAsync<T>(string templateFileName)
    {
        var filePath = Path.Combine(_templatesPath, templateFileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Template file not found: {templateFileName}");
        }
    
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
    }

    public async Task<T?> ReadTemplateSingleAsync<T>(string templateFileName) where T : class
    {
        var filePath = Path.Combine(_templatesPath, templateFileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Template file not found: {templateFileName}");
        }
    
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, _options);
    }
}