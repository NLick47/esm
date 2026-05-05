namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class PipelineStage
{
    public string ProcessorId { get; set; } = string.Empty;
    public string ProcessorName { get; set; } = string.Empty;
    public int SortOrder { get; set; } = 0;
    public bool IsSender { get; set; } = false;
    public StageFailureAction OnFailure { get; set; } = StageFailureAction.Stop;
    public string? Condition { get; set; }
}
