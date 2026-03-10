using EventStreamManager.Infrastructure.Models.Execution.Debug;

namespace EventStreamManager.WebApi.Service;

public interface IDebugService
{
    /// <summary>
    /// 执行普通调试
    /// </summary>
    Task<DebugResponse> ExecuteDebugAsync(DebugRequest request);

    /// <summary>
    /// 执行Examine调试
    /// </summary>
    Task<EditorDebugResponse> ExecuteExamineDebugAsync(EditorDebugRequest request);

    /// <summary>
    /// 执行接口调试
    /// </summary>
    Task<InterfaceDebugResponse> DebugInterfaceAsync(InterfaceDebugRequest request);
}