using System.Runtime.Loader;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.JSFunction.Loader;

/// <summary>
/// JS函数加载器 - 负责扫描和加载所有函数插件
/// </summary>
public class JsFunctionLoader : IDisposable
{
    private readonly List<AssemblyLoadContext> _loadContexts = new();
    private readonly List<IJSFunctionProvider> _providers = new();
    private readonly string _pluginPath;
    private readonly bool _loadBuiltInProviders;
    private readonly ILogger<JsFunctionLoader>? _logger;
    
    public JsFunctionLoader(
        ILogger<JsFunctionLoader>? logger = null, 
        string pluginPath = "Plugins", 
        bool loadBuiltInProviders = true)
    {
        _logger = logger;
        _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginPath);
        _loadBuiltInProviders = loadBuiltInProviders;
        
        if (!Directory.Exists(_pluginPath))
        {
            Directory.CreateDirectory(_pluginPath);
            _logger?.LogInformation("创建插件目录: {PluginPath}", _pluginPath);
        }
    }

    /// <summary>
    /// 加载所有函数提供者
    /// </summary>
    public IEnumerable<IJSFunctionProvider> LoadAllProviders()
    {
        _providers.Clear();
        _logger?.LogDebug("开始加载所有函数提供者");
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        // 加载内置提供者
        if (_loadBuiltInProviders)
        {
            LoadBuiltInProviders();
        }
        
        // 加载插件提供者
        LoadPluginProviders();
        
        stopwatch.Stop();
        _logger?.LogInformation("函数提供者加载完成，共 {Count} 个，耗时 {ElapsedMs}ms", 
            _providers.Count, stopwatch.ElapsedMilliseconds);
        
        return _providers;
    }
    
    /// <summary>
    /// 加载内置提供者
    /// </summary>
    private void LoadBuiltInProviders()
    {
        try
        {
            var builtInProviders = BuiltInProviderRegistry.CreateAll();
            var jsFunctionProviders = builtInProviders as IJSFunctionProvider[] ?? builtInProviders.ToArray();
            _providers.AddRange(jsFunctionProviders);
            
            foreach (var provider in jsFunctionProviders)
            {
                _logger?.LogDebug("已加载内置函数提供者: {ProviderName}", provider.Name);
            }
            
            _logger?.LogInformation("内置函数提供者加载完成，共 {Count} 个", jsFunctionProviders.Length);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "加载内置提供者失败");
        }
    }
    
    /// <summary>
    /// 加载插件提供者
    /// </summary>
    private void LoadPluginProviders()
    {
        if (!Directory.Exists(_pluginPath))
        {
            _logger?.LogWarning("插件目录不存在: {PluginPath}", _pluginPath);
            return;
        }

        var dllFiles = Directory.GetFiles(_pluginPath, "*.dll", SearchOption.AllDirectories);
        _logger?.LogDebug("在插件目录中找到 {Count} 个 DLL 文件", dllFiles.Length);
        
        var loadedPluginCount = 0;
        var failedPluginCount = 0;

        foreach (var dllFile in dllFiles)
        {
            try
            {
                var fileName = Path.GetFileName(dllFile);
                _logger?.LogDebug("正在加载插件: {FileName}", fileName);
                
                var loadContext = new PluginLoadContext(dllFile);
                _loadContexts.Add(loadContext);
                
                var assembly = loadContext.LoadFromAssemblyPath(dllFile);
                
                var providers = assembly.GetTypes()
                    .Where(t => typeof(IJSFunctionProvider).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
                    .Select(t => Activator.CreateInstance(t) as IJSFunctionProvider)
                    .Where(p => p != null)
                    .ToList();

                if (providers.Any())
                {
                    _providers.AddRange(providers!);
                    loadedPluginCount++;
                    
                    _logger?.LogInformation("从 {FileName} 加载了 {Count} 个函数提供者", 
                        fileName, providers.Count);
                    
                    foreach (var provider in providers!)
                    {
                        _logger?.LogDebug("  - 提供者: {ProviderName}, 版本: {Version}", 
                            provider.Name, provider.Version ?? "未知");
                    }
                }
                else
                {
                    _logger?.LogWarning("插件 {FileName} 中未找到 IJSFunctionProvider 实现", fileName);
                }
            }
            catch (Exception ex)
            {
                failedPluginCount++;
                _logger?.LogError(ex, "加载插件失败 {DllFile}", dllFile);
            }
        }
        
        if (loadedPluginCount > 0 || failedPluginCount > 0)
        {
            _logger?.LogInformation("插件加载统计: 成功 {Loaded} 个, 失败 {Failed} 个", 
                loadedPluginCount, failedPluginCount);
        }
    }

    /// <summary>
    /// 重新加载所有提供者
    /// </summary>
    public IEnumerable<IJSFunctionProvider> ReloadAllProviders()
    {
        _logger?.LogInformation("开始重新加载所有函数提供者");
        Dispose();
        return LoadAllProviders();
    }

    /// <summary>
    /// 获取所有已加载的函数
    /// </summary>
    public IEnumerable<FunctionMetadata> GetAllFunctions()
    {
        var functions = _providers.SelectMany(p => p.GetFunctions()).ToList();
        _logger?.LogDebug("获取所有函数，共 {Count} 个", functions.Count);
        return functions;
    }

    /// <summary>
    /// 按分类获取函数
    /// </summary>
    public IEnumerable<FunctionMetadata> GetFunctionsByCategory(string category)
    {
        var functions = GetAllFunctions()
            .Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        _logger?.LogDebug("按分类 {Category} 获取函数，共 {Count} 个", category, functions.Count);
        return functions;
    }
    
    /// <summary>
    /// 获取特定名称的函数
    /// </summary>
    public FunctionMetadata? GetFunction(string name)
    {
        var function = GetAllFunctions()
            .FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        
        if (function != null)
        {
            _logger?.LogDebug("找到函数: {FunctionName}, 提供者: {ProviderName}", 
                name, function.Name);
        }
        else
        {
            _logger?.LogDebug("未找到函数: {FunctionName}", name);
        }
        
        return function;
    }

    public void Dispose()
    {
        _logger?.LogInformation("开始卸载所有插件，共 {Count} 个加载上下文", _loadContexts.Count);
        
        var successCount = 0;
        var failCount = 0;
        
        foreach (var context in _loadContexts)
        {
            try
            {
                context.Unload();
                successCount++;
            }
            catch (Exception ex)
            {
                failCount++;
                _logger?.LogWarning(ex, "卸载插件上下文失败");
            }
        }
        
        _loadContexts.Clear();
        _providers.Clear();
        
        _logger?.LogInformation("插件卸载完成: 成功 {SuccessCount} 个, 失败 {FailCount} 个", 
            successCount, failCount);
    }
}