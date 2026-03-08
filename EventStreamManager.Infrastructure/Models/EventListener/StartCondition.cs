namespace EventStreamManager.Infrastructure.Models.EventListener;

public class StartCondition
{
    public string Type { get; set; } = "time"; // "time" 或 "id"
    public string TimeValue { get; set; } = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm");
    public string IdValue { get; set; } = "";
}