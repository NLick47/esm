using EventStreamManager.EventProcessor.Entities;
using ExecutionResult = EventStreamManager.Infrastructure.Entities.ExecutionResult;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IScriptExecutor
{
    Task<ExecutionResult> ExecuteAsync(ScriptContext context);
}