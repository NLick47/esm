using EventStreamManager.Infrastructure.Models.DataBase;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface ITableInitializationService
{
    Task<InitializeTablesResult> InitializeTablesAsync(DatabaseConfig config);
}