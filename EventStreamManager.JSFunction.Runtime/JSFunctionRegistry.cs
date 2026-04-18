using Jint;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.JSFunction.Runtime;

/// <summary>
/// JS函数注册服务
/// </summary>
public class JsFunctionRegistry
{
    private readonly IEnumerable<IJsFunctionProvider> _providers;
    private readonly Dictionary<string, FunctionDefinition> _functionMap;
    private readonly ILogger<JsFunctionRegistry> _logger;

    public JsFunctionRegistry(IEnumerable<IJsFunctionProvider>? providers, ILogger<JsFunctionRegistry> logger)
    {
        _providers = providers ?? Enumerable.Empty<IJsFunctionProvider>();
        _functionMap = new Dictionary<string, FunctionDefinition>();
        _logger = logger;
        
        LoadFunctions();
    }

    /// <summary>
    /// 加载所有函数到内存映射
    /// </summary>
    private void LoadFunctions()
    {
        try
        {
            var allFunctions = GetAllFunctionDefinitions().ToList();
            foreach (var func in allFunctions)
            {
                _functionMap.TryAdd(func.Name, func);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }
    }

    /// <summary>
    /// 获取所有函数定义（从提供者实时获取）
    /// </summary>
    public IEnumerable<FunctionDefinition> GetAllFunctionDefinitions()
    {
        foreach (var provider in _providers)
        {
            var functions = provider.GetFunctions();
            foreach (var func in functions)
            {
                yield return new FunctionDefinition
                {
                    Name = func.Name,
                    Description = func.Description,
                    Category = func.Category,
                    FunctionDelegate = func.FunctionDelegate,
                    Parameters = func.Parameters,
                    ReturnType = func.ReturnType,
                    Example = func.Example,
                    ProviderName = provider.Name,
                    ProviderVersion = provider.Version
                };
            }
        }
    }

    /// <summary>
    /// 获取所有函数（用于API返回）
    /// </summary>
    public IEnumerable<FunctionDefinition> GetAvailableFunctions()
    {
        return _functionMap.Values;
    }

    /// <summary>
    /// 按分类获取函数
    /// </summary>
    public IEnumerable<FunctionDefinition> GetFunctionsByCategory(string category)
    {
        return GetAvailableFunctions()
            .Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    public IEnumerable<string> GetAllCategories()
    {
        return GetAvailableFunctions()
            .Select(f => f.Category)
            .Distinct()
            .OrderBy(c => c);
    }

    /// <summary>
    /// 创建引擎并注入所有函数
    /// </summary>
    public Engine CreateEngine()
    {
        var engine = new Engine();
        
        foreach (var func in _functionMap.Values)
        {
            if (func.FunctionDelegate != null)
            {
                try
                {
                    engine.SetValue(func.Name, func.FunctionDelegate);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "注入函数 {FunctionName} 失败", func.Name);
                }
            }
        }
        
        return engine;
    }

    /// <summary>
    /// 注入函数到现有引擎
    /// </summary>
    public Engine InjectToEngine(Engine engine)
    {
        foreach (var func in _functionMap.Values)
        {
            if (func.FunctionDelegate != null)
            {
                try
                {
                    engine.SetValue(func.Name, func.FunctionDelegate);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "注入函数 {FunctionName} 失败", func.Name);
                }
            }
        }
        return engine;
    }
    
   

    /// <summary>
    /// 获取单个函数
    /// </summary>
    public FunctionDefinition? GetFunction(string name)
    {
        return _functionMap.GetValueOrDefault(name);
    }

    /// <summary>
    /// 重新加载函数
    /// </summary>
    public void Reload()
    {
        _functionMap.Clear();
        LoadFunctions();
    }
}
