namespace EventStreamManager.JSFunction.Runtime;

public class PipelineStageInfo
{
    public int StageIndex { get; set; }
    public int StageCount { get; set; }
    public string StageName { get; set; } = string.Empty;
    public bool IsSender { get; set; }
}
