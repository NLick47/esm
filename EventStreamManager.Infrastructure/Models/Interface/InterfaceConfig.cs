namespace EventStreamManager.Infrastructure.Models.Interface;

public class InterfaceConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public List<string> ProcessorIds { get; set; } = new();
    public List<string> ProcessorNames { get; set; } = new();
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "POST";
    public List<HeaderItem> Headers { get; set; } = new();
    public int Timeout { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
    public int RetryInterval { get; set; } = 5;
    public bool Enabled { get; set; }
    public string RequestTemplate { get; set; }
    public string Description { get; set; } = string.Empty;
}