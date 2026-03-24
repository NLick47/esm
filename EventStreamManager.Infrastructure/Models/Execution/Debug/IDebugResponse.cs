namespace EventStreamManager.Infrastructure.Models.Execution.Debug;

public interface IDebugResponse
{
    bool Success { get; set; }
    string? ErrorMessage { get; set; }
    List<DebugLogEntry> Logs { get; set; }
    long ExecutionTimeMs { get; set; }
}