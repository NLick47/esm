using EventStreamManager.Infrastructure.Models.Execution;
using EventStreamManager.JSFunction;
using Jint;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services;

/// <summary>
/// JS函数注册服务
/// </summary>
public class JSFunctionRegistry
{
    private readonly IEnumerable<IJSFunctionProvider> _providers;
    private readonly Dictionary<string, FunctionDefinition> _functionMap;
    private readonly ILogger<JSFunctionRegistry> _logger;

    public JSFunctionRegistry(IEnumerable<IJSFunctionProvider> providers, ILogger<JSFunctionRegistry> logger)
    {
        _providers = providers ?? Enumerable.Empty<IJSFunctionProvider>();
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
                if (!_functionMap.ContainsKey(func.Name))
                {
                    _functionMap[func.Name] = func;
                }
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
                    Category = func.Category ?? "Uncategorized",
                    FunctionDelegate = func.FunctionDelegate,
                    Parameters = func.Parameters ?? new List<FunctionParameter>(),
                    ReturnType = func.ReturnType ?? typeof(object),
                    Example = func.Example ?? string.Empty,
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
            .Where(f => f.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取所有分类
    /// </summary>
    public IEnumerable<string> GetAllCategories()
    {
        return GetAvailableFunctions()
            .Select(f => f.Category ?? "Uncategorized")
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
        return _functionMap.TryGetValue(name, out var func) ? func : null;
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

