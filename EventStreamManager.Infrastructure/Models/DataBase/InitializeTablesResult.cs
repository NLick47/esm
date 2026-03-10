namespace EventStreamManager.Infrastructure.Models.DataBase;

public class InitializeTablesResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> CreatedTables { get; set; } = new();
    public Dictionary<string, string> TableResults { get; set; } = new();
}