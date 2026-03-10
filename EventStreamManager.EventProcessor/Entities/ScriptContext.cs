using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.EventProcessor.Entities;

/// <summary>
/// 脚本执行上下文
/// </summary>
public class ScriptContext
{
    public string ProcessorId { get; set; } = string.Empty;
    public string ProcessorName { get; set; } = string.Empty;
    public string DatabaseType { get; set; } = string.Empty;
    public Event Event { get; set; } = new();
    public Dictionary<string, object>? QueryResult { get; set; }
    public JSProcessor? ProcessorConfig { get; set; }
}