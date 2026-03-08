using System.Runtime.Loader;


namespace EventStreamManager.JSFunction.Loader;

/// <summary>
/// JS函数加载器 - 负责扫描和加载所有函数插件
/// </summary>
public class JSFunctionLoader : IDisposable
{
    private readonly List<AssemblyLoadContext> _loadContexts = new();
    private readonly List<IJSFunctionProvider> _providers = new();
    private readonly string _pluginPath;

    public JSFunctionLoader(string pluginPath = "Plugins")
    {
        _pluginPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginPath);
        
        // 确保插件目录存在
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
        
        //从插件目录加载DLL
        LoadProvidersFromPluginDirectory();
        
        return _providers;
    }
    
    
    /// <summary>
    /// 从插件目录加载提供者
    /// </summary>
    private void LoadProvidersFromPluginDirectory()
    {
        if (!Directory.Exists(_pluginPath)) return;

        foreach (var dllFile in Directory.GetFiles(_pluginPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                // 创建独立的加载上下文
                var loadContext = new PluginLoadContext(dllFile);
                _loadContexts.Add(loadContext);
                
                var assembly = loadContext.LoadFromAssemblyPath(dllFile);
                
                var providers = assembly.GetTypes()
                    .Where(t => typeof(IJSFunctionProvider).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                    .Select(t => Activator.CreateInstance(t) as IJSFunctionProvider)
                    .Where(p => p != null);

                _providers.AddRange(providers!);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载插件失败 {dllFile}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 获取所有已加载的函数
    /// </summary>
    public IEnumerable<FunctionMetadata> GetAllFunctions()
    {
        return _providers.SelectMany(p => p.GetFunctions());
    }

    public void Dispose()
    {
        foreach (var context in _loadContexts)
        {
            context.Unload();
        }
        _loadContexts.Clear();
        _providers.Clear();
    }
}
