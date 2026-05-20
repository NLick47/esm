namespace EventStreamManager.Infrastructure.Models.EventLog;

public class EventHandleStats
{
    public int Total { get; set; }
    public int Success { get; set; }
    public int Failed { get; set; }
    public int DeadLetter { get; set; }
    public int Processing { get; set; }
}
