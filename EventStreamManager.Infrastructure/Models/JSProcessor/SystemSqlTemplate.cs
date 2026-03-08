namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class SystemSqlTemplate
{
    public string Id { get; set; } = string.Empty; // 如 "status-update"
    public string Name { get; set; } = string.Empty;
    public List<string> EventCodes { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
}