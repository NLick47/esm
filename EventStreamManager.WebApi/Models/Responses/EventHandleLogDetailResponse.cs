namespace EventStreamManager.WebApi.Models.Responses;

public class EventHandleLogDetailResponse
{
    public int LogId { get; set; }
    public string? ErrorStack { get; set; }
    public string? ConsoleOutput { get; set; }
    public int? ErrorLineNumber { get; set; }
    public int? ErrorColumn { get; set; }
    public string? ScriptSnapshot { get; set; }
    public string? InputDataSnapshot { get; set; }
}
