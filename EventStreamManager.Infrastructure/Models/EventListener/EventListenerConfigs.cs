namespace EventStreamManager.Infrastructure.Models.EventListener;

public class EventListenerConfigs
{
    public Dictionary<string, EventConfig> Databases { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Version { get; set; } = "1.0.0";
}