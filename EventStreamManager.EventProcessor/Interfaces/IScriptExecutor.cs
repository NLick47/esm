using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Entities;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IScriptExecutor
{
    Task<ExecutionResult> ExecuteAsync(ScriptContext context);
}