namespace EventStreamManager.EventProcessor.Entities;

public  sealed record ServiceStateSnapshot
{
    public  bool IsEnabled { get; init; }
    public  DateTime StartTime { get; init; }
    public  DateTime? PauseTime { get; init; }
    public TimeSpan TotalPausedDuration { get; init; }
    public DateTime LastUpdated { get; init; }
    public string Version { get; init; } = "1.0";

    public TimeSpan GetRunningDuration(DateTime? now = null)
    {
        var currentTime = now ?? DateTime.Now;
        var totalDuration = currentTime - StartTime;
        var currentPauseDuration = PauseTime.HasValue 
            ? currentTime - PauseTime.Value 
            : TimeSpan.Zero;

        return totalDuration - TotalPausedDuration - currentPauseDuration;
    }
}