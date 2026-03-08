// Services/IEventListenerConfigService.cs

using EventStreamManager.Infrastructure.Models.EventListener;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IEventListenerConfigService
{
    // 获取所有配置
    Task<EventListenerConfigs> GetAllConfigsAsync();
    
    // 获取指定类型的配置
    Task<EventConfig?> GetConfigByTypeAsync(string databaseType);
    
    // 更新配置
    Task<EventConfig> UpdateConfigAsync(string databaseType, EventConfig config);
    
    // 切换启用状态
    Task<bool> ToggleEnabledAsync(string databaseType, bool enabled);
    
    // 重置为默认配置
    Task<EventConfig> ResetToDefaultAsync(string databaseType);
    
    
    // 获取数据库类型列表
    Task<List<DatabaseTypeInfo>> GetDatabaseTypesAsync();
    
   
    
    // 获取监听起始条件
    Task<StartCondition?> GetStartConditionAsync(string databaseType);
    
    // 更新监听起始条件
    Task<bool> UpdateStartConditionAsync(string databaseType, StartCondition condition);
    
    // 清除缓存
    void ClearCache();
}