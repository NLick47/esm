namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 初始化表结构响应
/// </summary>
public class InitializeTablesResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> CreatedTables { get; set; } = new();
    public Dictionary<string, string> TableResults { get; set; } = new();
}
