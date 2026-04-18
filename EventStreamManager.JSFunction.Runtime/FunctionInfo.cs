namespace EventStreamManager.JSFunction.Runtime;

public class FunctionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderVersion { get; set; } = string.Empty;
    public List<ParameterInfo> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = "object";
}
