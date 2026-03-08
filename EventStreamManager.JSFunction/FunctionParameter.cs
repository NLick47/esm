namespace EventStreamManager.JSFunction;

public class FunctionParameter
{
    public string Name { get; set; } = string.Empty;
    public Type Type { get; set; } = typeof(string);
    public string Description { get; set; } = string.Empty;
    public bool IsOptional { get; set; }
    public object? DefaultValue { get; set; }
}