using EventStreamManager.JSFunction;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.JSFunction.Loader;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJsFunctionLoader(this IServiceCollection services)
    {
        services.AddSingleton<JsFunctionLoader>(serviceProvider =>
        {
            var logger = serviceProvider.GetService<ILogger<JsFunctionLoader>>();
            return new JsFunctionLoader(logger);
        });

        services.AddSingleton<IEnumerable<IJsFunctionProvider>>(serviceProvider =>
        {
            var loader = serviceProvider.GetRequiredService<JsFunctionLoader>();
            return loader.LoadAllProviders().ToList();
        });

        return services;
    }
}
