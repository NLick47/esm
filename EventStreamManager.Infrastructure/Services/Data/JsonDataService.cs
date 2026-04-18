using System.Text.Json;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;

namespace EventStreamManager.Infrastructure.Services.Data;

public class JsonDataService : IDataService, IDisposable
{
    private readonly string _dataPath;
    private readonly string _templatesPath;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Dictionary<string, object> _memoryCache = new();
    private bool _disposed;
    
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
        if (!Directory.Exists(_templatesPath)) Directory.CreateDirectory(_templatesPath);
    }
    
    public async Task<List<T>> ReadAsync<T>(string fileName)
    {
        ValidateFileName(fileName);
        ThrowIfDisposed();

        await _lock.WaitAsync();
        try
        {
            // 检查内存中是否已有该文件的数据
            if (_memoryCache.TryGetValue(fileName, out var cachedData))
            {
                return DeepClone(cachedData as List<T>) ?? new List<T>();
            }
            
            // 如果内存中没有，从文件读取
            var filePath = Path.Combine(_dataPath, fileName);
            if (!File.Exists(filePath))
            {
                var emptyList = new List<T>();
                _memoryCache[fileName] = emptyList;
                return new List<T>();
            }
            
            var json = await File.ReadAllTextAsync(filePath);
            List<T> data;
            try
            {
                data = JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
            }
            catch (JsonException)
            {
                // 文件损坏时备份并返回空列表
                var backupPath = filePath + $".corrupted.{DateTime.Now:yyyyMMddHHmmss}.bak";
                File.Move(filePath, backupPath);
                data = new List<T>();
            }

            _memoryCache[fileName] = data;
            return DeepClone(data) ?? new List<T>();
        }
        finally
        {
            _lock.Release();
        }
    }
    
    public async Task WriteAsync<T>(string fileName, List<T> data)
    {
        ValidateFileName(fileName);
        if (data == null) throw new ArgumentNullException(nameof(data));
        ThrowIfDisposed();

        await _lock.WaitAsync();
        try
        {
            // 写入文件（原子写入：先写临时文件，再覆盖）
            var filePath = Path.Combine(_dataPath, fileName);
            var tempPath = filePath + ".tmp";
            var json = JsonSerializer.Serialize(data, _options);
            await File.WriteAllTextAsync(tempPath, json);
            File.Move(tempPath, filePath, overwrite: true);

            // 写文件成功后再更新缓存
            _memoryCache[fileName] = DeepClone(data) ?? new List<T>();
        }
        finally
        {
            _lock.Release();
        }
    }

    public void ClearCache(string? fileName = null)
    {
        ThrowIfDisposed();

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
        ValidateFileName(templateFileName);
        ThrowIfDisposed();

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
        ValidateFileName(templateFileName);
        ThrowIfDisposed();

        var filePath = Path.Combine(_templatesPath, templateFileName);
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Template file not found: {templateFileName}");
        }
    
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<T>(json, _options);
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("文件名不能为空", nameof(fileName));

        if (fileName.Contains("..") || fileName.Contains('/') || fileName.Contains('\\'))
        {
            throw new ArgumentException("文件名包含非法字符", nameof(fileName));
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(JsonDataService));
    }

    private List<T>? DeepClone<T>(List<T>? source)
    {
        if (source == null) return null;
        var json = JsonSerializer.Serialize(source, _options);
        return JsonSerializer.Deserialize<List<T>>(json, _options);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _lock.Dispose();
    }
}
