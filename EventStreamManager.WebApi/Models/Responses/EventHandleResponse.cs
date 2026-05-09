namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 事件处理记录响应
/// </summary>
public class EventHandleResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string? EventCode { get; set; }
    public string ProcessorId { get; set; } = string.Empty;
    public string ProcessorName { get; set; } = string.Empty;
    public int HandleTimes { get; set; }
    public string LastHandleStatus { get; set; } = string.Empty;
    public string? LastHandleMessage { get; set; }
    public DateTime? LastHandleDatetime { get; set; }
    public long? LastHandleElapsedMs { get; set; }
    public string? StrEventReferenceId { get; set; }
    public bool NeedToSend { get; set; }
    public string? Reason { get; set; }
    public bool? ScriptSuccess { get; set; }
    public bool? SendSuccess { get; set; }
    public bool IsDeadLetter { get; set; }
    public string? RequestData { get; set; }
    public string? ResponseData { get; set; }
    public bool IsFinished { get; set; }
    public DateTime CreateDatetime { get; set; }
    public string? EventName { get; set; }
}
