using EventStreamManager.JSFunction;

namespace EventStreamManager.Infrastructure.Models.Execution;

public class FunctionDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "General";
    public Delegate? FunctionDelegate { get; set; }
    public List<FunctionParameter> Parameters { get; set; } = new();
    public Type ReturnType { get; set; } = typeof(object);
    public string Example { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderVersion { get; set; } = string.Empty;
}