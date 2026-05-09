namespace EventStreamManager.WebApi.Models.Responses;

/// <summary>
/// 事件监听配置响应
/// </summary>
public class EventConfigResponse
{
    public int ScanFrequency { get; set; }
    public int BatchSize { get; set; }
    public bool Enabled { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string PrimaryKey { get; set; } = string.Empty;
    public string TimestampField { get; set; } = string.Empty;
    public int TotalEventsProcessed { get; set; }
    public int MaxRetryCount { get; set; }
    public StartConditionResponse? StartCondition { get; set; }
}
