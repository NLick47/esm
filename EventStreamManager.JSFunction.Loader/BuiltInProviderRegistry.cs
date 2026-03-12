using EventStreamManager.JSFunction.Standard;

namespace EventStreamManager.JSFunction.Loader;

public class BuiltInProviderRegistry
{
    private static readonly List<Func<IJSFunctionProvider>> ProviderFactories = new();
    
    static BuiltInProviderRegistry()
    {
        // 注册内置提供者
        Register(() => new StandardJsFunctionProvider());
    }
    
    public static void Register(Func<IJSFunctionProvider> factory)
    {
        ProviderFactories.Add(factory);
    }
    
    
    public static IEnumerable<IJSFunctionProvider> CreateAll()
    {
        return ProviderFactories.Select(factory => factory()).Where(p => p != null);
    }
}