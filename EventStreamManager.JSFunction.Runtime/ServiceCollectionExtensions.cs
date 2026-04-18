using Microsoft.Extensions.DependencyInjection;

namespace EventStreamManager.JSFunction.Runtime;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsFunctionRuntime(this IServiceCollection services)
    {
        services.AddSingleton<JsFunctionRegistry>();
        services.AddSingleton<IJavaScriptExecutionService, JavaScriptExecutionService>();
        return services;
    }
}
