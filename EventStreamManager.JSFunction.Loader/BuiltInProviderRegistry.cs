using EventStreamManager.JSFunction.Sql;
using EventStreamManager.JSFunction.Standard;

namespace EventStreamManager.JSFunction.Loader;

public static class BuiltInProviderRegistry
{
    private static readonly List<Func<IJsFunctionProvider>> ProviderFactories = new();
    
    static BuiltInProviderRegistry()
    {
        // 注册内置提供者
        Register(() => new StandardJsFunctionProvider());
        Register(() => new SimpleSqlJsFunctionProvider());
    }

    private static void Register(Func<IJsFunctionProvider> factory)
    {
        ProviderFactories.Add(factory);
    }
    
    
    public static IEnumerable<IJsFunctionProvider> CreateAll()
    {
        return ProviderFactories.Select(factory => factory());
    }
}