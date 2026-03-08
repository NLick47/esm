namespace EventStreamManager.Infrastructure.Models.Execution;

public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string Description { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
}