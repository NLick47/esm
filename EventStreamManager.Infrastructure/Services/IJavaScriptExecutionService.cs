using EventStreamManager.Infrastructure.Models.Execution;

namespace EventStreamManager.Infrastructure.Services;

/// <summary>
/// JavaScript执行服务接口
/// </summary>
public interface IJavaScriptExecutionService
{
    /// <summary>
    /// 执行JavaScript脚本中的process函数
    /// </summary>
    /// <param name="script">包含process函数的脚本</param>
    /// <param name="inputData">要注入到data参数的数据</param>
    /// <returns>执行结果，包含process函数的返回值</returns>
    Task<ExecutionResult> ExecuteProcessAsync(string script, object? inputData = null);

    

    /// <summary>
    /// 验证脚本是否包含有效的process函数
    /// </summary>
    /// <param name="script">要验证的脚本</param>
    /// <returns>验证结果</returns>
    ValidationResult ValidateScript(string script);

    /// <summary>
    /// 获取所有可用的函数
    /// </summary>
    IEnumerable<FunctionInfo> GetAvailableFunctions();

    /// <summary>
    /// 获取所有分类
    /// </summary>
    IEnumerable<string> GetAllCategories();
}