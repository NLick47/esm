using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class HttpSendDebugInfo
{
    public SendResult Result { get; set; } = new();
    public RequestInfo RequestInfo { get; set; } = new();
    public long ExecutionTimeMs { get; set; }
}