using EventStreamManager.Infrastructure.Models.DataBase;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface IDatabaseSchemeService
{
    Task<Dictionary<string, List<DatabaseConfig>>> GetAllConfigsAsync();
    Task<List<DatabaseConfig>> GetConfigsByTypeAsync(string databaseType);
    Task<DatabaseConfig?> GetConfigByIdAsync(string databaseType, string id);
    Task<DatabaseConfig> AddConfigAsync(string databaseType, DatabaseConfig config);
    Task<DatabaseConfig?> UpdateConfigAsync(string databaseType, string id, DatabaseConfig config);
    Task<bool> DeleteConfigAsync(string databaseType, string id);
    Task<DatabaseConfig?> GetActiveConfigAsync(string databaseType);
    Task<bool> SetActiveConfigAsync(string databaseType, string id);
    
    Task<List<DatabaseType>> GetAllDatabaseTypesAsync();

    Task<DatabaseType> AddDatabaseTypeAsync(DatabaseType databaseType);
    
    Task<bool> DeleteDatabaseTypeAsync(string typeValue);
    
    Task<List<DatabaseTypeWithActiveConfigDto>> GetAllDatabaseTypesWithActiveConfigAsync();
   
}