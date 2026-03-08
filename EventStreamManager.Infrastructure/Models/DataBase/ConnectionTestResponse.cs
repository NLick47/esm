namespace EventStreamManager.Infrastructure.Models.DataBase;

public class ConnectionTestResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public long? ResponseTime { get; set; }
    public string? DatabaseVersion { get; set; }
}