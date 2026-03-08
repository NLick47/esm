namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class JSProcessor
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public List<string> DatabaseTypes { get; set; } = new();
    public List<string> EventCodes { get; set; } = new();
    public string SqlTemplate { get; set; } = string.Empty;
    public string Code { get; set; }
    public bool Enabled { get; set; }
    public string Description { get; set; } = string.Empty;
}