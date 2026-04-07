namespace EventStreamManager.Infrastructure.Models.Execution.Parameter;

public class ContextInfo
{
    public string EventId { get; set; } = string.Empty;
    public string StrEventReferenceId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventName { get; set; } = string.Empty;
    public string EventCode { get; set; } = string.Empty;
    public string OperatorName { get; set; } = string.Empty;
    public string OperatorCode { get; set;} = string.Empty;
    public DateTime CreateDatetime { get; set; }
    public string? ExtenData { get; set; }
}