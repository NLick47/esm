using EventStreamManager.Infrastructure.Repositories;
using EventStreamManager.Infrastructure.Repositories.Interfaces;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using EventStreamManager.JSFunction.Runtime;
using Microsoft.Extensions.DependencyInjection;

namespace EventStreamManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // JS 执行运行时（依赖 IJsFunctionProvider，需在 AddJsFunctionLoader 之后调用）
        services.AddJsFunctionRuntime();

        // 核心服务
        services.AddScoped<ISqlSugarContext, SqlSugarContext>();

        // 数据服务
        services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
        services.AddSingleton<IDatabaseSchemeService, DatabaseSchemeService>();
        services.AddSingleton<IEventListenerConfigService, EventListenerConfigService>();
        services.AddSingleton<IInterfaceConfigService, InterfaceConfigService>();
        services.AddSingleton<IDataService, JsonDataService>();
        services.AddSingleton<IProcessorService, ProcessorService>();
        services.AddSingleton<IProcessorVersionService, ProcessorVersionService>();
        services.AddSingleton<ISqlTemplateService, SqlTemplateService>();
        services.AddSingleton<ISystemVariableService, SystemVariableService>();
        services.AddScoped<ITableInitializationService, TableInitializationService>();
        services.AddScoped<IEventLogService, EventLogService>();

        // 调试服务
        services.AddScoped<IDebugService, DebugService>();

        // HTTP 请求服务
        services.AddHttpClient();
        services.AddScoped<IHttpSendService, HttpSendService>();

        // 脚本数据构建服务
        services.AddScoped<IEventDataBuilderService, EventDataBuilderService>();

        // Repository
        services.AddScoped<IEventHandleRepository, EventHandleRepository>();
        services.AddScoped<IEventRepository, EventRepository>();

        return services;
    }
}
