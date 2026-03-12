using System.Runtime.Loader;

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

    public JsFunctionLoader(string pluginPath = "Plugins", bool loadBuiltInProviders = true)
    {
        _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginPath);
        _loadBuiltInProviders = loadBuiltInProviders;
        
        if (!Directory.Exists(_pluginPath))
        {
            Directory.CreateDirectory(_pluginPath);
        }
    }

    /// <summary>
    /// 加载所有函数提供者
    /// </summary>
    public IEnumerable<IJSFunctionProvider> LoadAllProviders()
    {
        _providers.Clear();
        
        // 加载内置提供者
        if (_loadBuiltInProviders)
        {
            LoadBuiltInProviders();
        }
        
        // 加载插件提供者
        LoadPluginProviders();
        
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
                Console.WriteLine($"已加载内置函数提供者: {provider.Name}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"加载内置提供者失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载插件提供者
    /// </summary>
    private void LoadPluginProviders()
    {
        if (!Directory.Exists(_pluginPath)) return;

        foreach (var dllFile in Directory.GetFiles(_pluginPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
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
                    Console.WriteLine($"从 {Path.GetFileName(dllFile)} 加载了 {providers.Count} 个函数提供者");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载插件失败 {dllFile}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 重新加载所有提供者
    /// </summary>
    public IEnumerable<IJSFunctionProvider> ReloadAllProviders()
    {
        Dispose();
        return LoadAllProviders();
    }

    /// <summary>
    /// 获取所有已加载的函数
    /// </summary>
    public IEnumerable<FunctionMetadata> GetAllFunctions()
    {
        return _providers.SelectMany(p => p.GetFunctions());
    }

    /// <summary>
    /// 按分类获取函数
    /// </summary>
    public IEnumerable<FunctionMetadata> GetFunctionsByCategory(string category)
    {
        return GetAllFunctions().Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }
    
    
    /// <summary>
    /// 获取特定名称的函数
    /// </summary>
    public FunctionMetadata? GetFunction(string name)
    {
        return GetAllFunctions().FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public void Dispose()
    {
        foreach (var context in _loadContexts)
        {
            try
            {
                context.Unload();
            }
            catch
            {
                // 忽略卸载异常
            }
        }
        _loadContexts.Clear();
        _providers.Clear();
    }
}