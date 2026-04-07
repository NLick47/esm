namespace EventStreamManager.WebApi.Models.Requests;

public class ToggleStateRequest
{
    public bool? ForceState { get; init; }
}