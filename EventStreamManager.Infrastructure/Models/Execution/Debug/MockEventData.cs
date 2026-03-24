namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class MockEventData
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string EventCode { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty;
    public DateTime EventTime { get; set; } = DateTime.Now;
    public Dictionary<string, object> Data { get; set; } = new();
}