namespace EventStreamManager.WebApi.Models.Responses;

public class ProcessorStatusResponse
{
    public string DatabaseType { get; init; } = string.Empty;
    public bool IsRunning { get; init; }
    public DateTime? LastScanTime { get; init; }
    public int ProcessedEventCount { get; init; }
    public string? LastError { get; init; }
}