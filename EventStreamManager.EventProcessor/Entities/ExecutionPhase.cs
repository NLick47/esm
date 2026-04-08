namespace EventStreamManager.EventProcessor.Entities;

public enum  ExecutionPhase
{
    NotStarted = 0,
    ScriptExecution = 1,
    DataQuery = 2,
    HttpSending = 3,
    Completed = 4
}