namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class RollbackResult
{
    public JsProcessorVersion Version { get; set; } = null!;
    public List<string> RecoveredTemplates { get; set; } = new();
    public List<string> MissingEventCodes { get; set; } = new();
    public bool HasWarnings => RecoveredTemplates.Count > 0 || MissingEventCodes.Count > 0;
}
