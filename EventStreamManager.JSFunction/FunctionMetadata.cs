namespace EventStreamManager.JSFunction;

public class FunctionMetadata
{
    /// <summary>
    /// 函数名称（在JS中调用的名称）
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 函数描述
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    
    /// <summary>
    /// 函数分类
    /// </summary>
    public string Category { get; set; } = "General";
    
    
    /// <summary>
    /// 函数执行的实际委托
    /// </summary>
    public Delegate? FunctionDelegate { get; set; }
    
    
        
    /// <summary>
    /// 参数定义
    /// </summary>
    public List<FunctionParameter> Parameters { get; set; } = new();

    
    /// <summary>
    /// 返回值类型
    /// </summary>
    public Type ReturnType { get; set; } = typeof(object);
    
    
    /// <summary>
    /// 使用示例
    /// </summary>
    public string Example { get; set; } = string.Empty;
}