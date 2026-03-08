namespace EventStreamManager.Infrastructure.Models.DataBase;

public class DatabaseConfigs
{
    public Dictionary<string, List<DatabaseConfig>> Databases { get; set; } = new();
    
    public List<DatabaseType> DatabaseTypes { get; set; } = new(); 
}