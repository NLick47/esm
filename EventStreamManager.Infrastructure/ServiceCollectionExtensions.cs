using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace EventStreamManager.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        // 核心服务
        services.AddSingleton<IJavaScriptExecutionService, JavaScriptExecutionService>();
        services.AddScoped<ISqlSugarContext, SqlSugarContext>();

        // 数据服务
        services.AddSingleton<IDatabaseConnectionService, DatabaseConnectionService>();
        services.AddSingleton<IDatabaseSchemeService, DatabaseSchemeService>();
        services.AddSingleton<IEventListenerConfigService, EventListenerConfigService>();
        services.AddSingleton<IInterfaceConfigService, InterfaceConfigService>();
        services.AddSingleton<IDataService, JsonDataService>();
        services.AddSingleton<IProcessorService, ProcessorService>();
        services.AddSingleton<ISqlTemplateService, SqlTemplateService>();
        services.AddSingleton<ISystemVariableService, SystemVariableService>();
        services.AddScoped<ITableInitializationService, TableInitializationService>();
        services.AddScoped<IEventLogService, EventLogService>();

        // 调试服务
        services.AddScoped<IDebugService, DebugService>();

        // HTTP 请求服务
        services.AddHttpClient();
        services.AddScoped<IHttpSendService, HttpSendService>();

        // JS Function 注册表
        services.AddSingleton<JsFunctionRegistry>();

        // 脚本数据构建服务
        services.AddScoped<IEventDataBuilderService, EventDataBuilderService>();

        return services;
    }
}
