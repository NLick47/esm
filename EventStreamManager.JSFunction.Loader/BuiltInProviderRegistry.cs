using EventStreamManager.JSFunction.Standard;

namespace EventStreamManager.JSFunction.Loader;

public class BuiltInProviderRegistry
{
    private static readonly List<Func<IJsFunctionProvider>> ProviderFactories = new();
    
    static BuiltInProviderRegistry()
    {
        // 注册内置提供者
        Register(() => new StandardJsFunctionProvider());
    }
    
    public static void Register(Func<IJsFunctionProvider> factory)
    {
        ProviderFactories.Add(factory);
    }
    
    
    public static IEnumerable<IJsFunctionProvider> CreateAll()
    {
        return ProviderFactories.Select(factory => factory()).Where(p => p != null);
    }
}