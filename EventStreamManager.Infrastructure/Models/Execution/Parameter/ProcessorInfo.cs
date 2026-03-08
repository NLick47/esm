namespace EventStreamManager.Infrastructure.Models.Execution.Parameter;

public class ProcessorInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool? Enabled { get; set; }
}