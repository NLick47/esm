using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public class HttpSendDebugInfo
{
    public SendResult Result { get; set; }
    public RequestInfo RequestInfo { get; set; }
    public long ExecutionTimeMs { get; set; }
}