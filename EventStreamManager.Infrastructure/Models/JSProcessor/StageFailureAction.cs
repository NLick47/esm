namespace EventStreamManager.Infrastructure.Models.JSProcessor;

public enum StageFailureAction
{
    Stop,
    Continue,
    SkipToSender
}
