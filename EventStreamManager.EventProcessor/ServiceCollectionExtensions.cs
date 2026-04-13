using EventStreamManager.EventProcessor.Executors;
using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.EventProcessor.Processors;
using EventStreamManager.EventProcessor.Recorders;
using EventStreamManager.EventProcessor.Scanners;
using EventStreamManager.EventProcessor.Senders;
using EventStreamManager.EventProcessor.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EventStreamManager.EventProcessor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEventProcessorServices(this IServiceCollection services)
    {
        // 事件处理器
        services.AddSingleton<ProcessorFactory>();
        services.AddSingleton<IStateManagerService, StateManagerService>();
        services.AddSingleton<IProcessorManagerService, ProcessorManagerService>();
        services.AddSingleton<EventProcessorService>();

        services.AddHostedService(sp => sp.GetRequiredService<EventProcessorService>());

        // 扫描器、执行器、记录器、发送器
        services.AddScoped<IEventScanner, EventScanner>();
        services.AddScoped<IScriptExecutor, ScriptExecutor>();
        services.AddScoped<IHandleRecorder, HandleRecorder>();
        services.AddScoped<IInterfaceSender, InterfaceSender>();

        services.AddSingleton<IProcessorFactory, ProcessorFactory>();

        return services;
    }
}
