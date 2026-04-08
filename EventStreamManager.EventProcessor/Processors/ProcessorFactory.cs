using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Processors;

/// <summary>
/// 处理器工厂 - 创建数据库类型处理器
/// </summary>
public class ProcessorFactory : IProcessorFactory
{
    private readonly IServiceProvider _serviceProvider;  
    private readonly IDatabaseSchemeService _schemeService;

    public ProcessorFactory(
        IServiceProvider serviceProvider,  
        IDatabaseSchemeService schemeService)
    {
        _serviceProvider = serviceProvider;
        _schemeService = schemeService;
    }

    /// <summary>
    /// 创建处理器
    /// </summary>
    public DatabaseTypeProcessor Create(string databaseType)
    {
        var logger = _serviceProvider.GetRequiredService<ILogger<DatabaseTypeProcessor>>();
        var configService = _serviceProvider.GetRequiredService<IEventListenerConfigService>();

        return new DatabaseTypeProcessor(
            databaseType,
            _serviceProvider, 
            configService,
            logger);
    }

    /// <summary>
    /// 获取已配置的数据库类型列表
    /// </summary>
    public async Task<List<string>> GetConfiguredTypesAsync()
    {
        var types = await _schemeService.GetAllDatabaseTypesWithActiveConfigAsync();
        return types.Where(t => t.ActiveConfig != null).Select(t => t.Value).ToList();
    }
}