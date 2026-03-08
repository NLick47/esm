namespace EventStreamManager.JSFunction;

public interface  IJSFunctionProvider
{
    /// <summary>
    /// 提供者名称
    /// </summary>
    string Name { get; }
    
    
    /// <summary>
    /// 提供者描述
    /// </summary>
    string Description { get; }
    
    
    /// <summary>
    /// 版本号
    /// </summary>
    string Version { get; }
    
    
    /// <summary>
    /// 获取此提供者提供的所有JS函数
    /// </summary>
    IEnumerable<FunctionMetadata> GetFunctions();
}