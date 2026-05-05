namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class ProcessorPipeline
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> EventCodes { get; set; } = new();
    public List<string> DatabaseTypes { get; set; } = new();
    public List<PipelineStage> Stages { get; set; } = new();
    public bool Enabled { get; set; } = true;
    public int MaxRetryCount { get; set; } = 1;
}
