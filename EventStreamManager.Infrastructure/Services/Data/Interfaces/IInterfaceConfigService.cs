using EventStreamManager.Infrastructure.Models.Interface;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IInterfaceConfigService
{
    Task<List<InterfaceConfig>> GetAllConfigsAsync();
    Task<InterfaceConfig?> GetConfigByIdAsync(string id);
    Task<InterfaceConfig> CreateConfigAsync(InterfaceConfig config);
    Task<InterfaceConfig?> UpdateConfigAsync(string id, InterfaceConfig config);
    Task<bool> DeleteConfigAsync(string id);
    Task<InterfaceConfig?> ToggleConfigStatusAsync(string id);
    Task<InterfaceConfig?> DuplicateConfigAsync(string id);
    Task<List<AvailableProcessor>> GetAvailableProcessorsAsync();
    Task<bool> ValidateProcessorIdsAsync(List<string> processorIds);
}