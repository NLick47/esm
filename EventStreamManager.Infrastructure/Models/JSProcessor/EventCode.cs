namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public class EventCode
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
}