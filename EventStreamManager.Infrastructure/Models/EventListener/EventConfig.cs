namespace EventStreamManager.Infrastructure.Models.EventListener;

public class EventConfig
{
    public int ScanFrequency { get; set; } = 60;
    public int BatchSize { get; set; } = 50;
    public bool Enabled { get; set; } = true;
    public string TableName { get; set; } = "tblevent";
    public string PrimaryKey { get; set; } = "event_id";
    public string TimestampField { get; set; } = "create_time";
  
    public int TotalEventsProcessed { get; set; }
    
    public StartCondition? StartCondition { get; set; }
}