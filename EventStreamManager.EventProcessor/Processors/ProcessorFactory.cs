using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Processors;

/// <summary>
/// 处理器工厂 - 创建数据库类型处理器
/// </summary>
public class ProcessorFactory
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDatabaseSchemeService _schemeService;
    private readonly IEventListenerConfigService _configService;

    public ProcessorFactory(
        IServiceScopeFactory scopeFactory,
        ILoggerFactory loggerFactory,
        IDatabaseSchemeService schemeService,
        IEventListenerConfigService configService)
    {
        _scopeFactory = scopeFactory;
        _loggerFactory = loggerFactory;
        _schemeService = schemeService;
        _configService = configService;
    }

    /// <summary>
    /// 创建处理器
    /// </summary>
    public DatabaseTypeProcessor Create(string databaseType)
    {
        return new DatabaseTypeProcessor(
            databaseType,
            _scopeFactory,
            _loggerFactory,
            _configService,
            _loggerFactory.CreateLogger<DatabaseTypeProcessor>());
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