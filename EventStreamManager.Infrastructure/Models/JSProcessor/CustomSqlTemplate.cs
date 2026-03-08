namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class CustomSqlTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<string> EventCodes { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
}