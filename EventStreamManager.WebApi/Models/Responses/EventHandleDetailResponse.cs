namespace EventStreamManager.WebApi.Models.Responses;

public class EventHandleDetailResponse : EventHandleResponse
{
    public EventHandleLogDetailResponse? Detail { get; set; }
}
