namespace EventStreamManager.JSFunction;

/// <summary>
/// 脚本输出捕获器
/// </summary>
public class ScriptOutput
{
    private readonly List<OutputMessage> _outputs = new();
    private readonly List<OutputMessage> _errors = new();

    /// <summary>
    /// 添加普通输出
    /// </summary>
    public void Write(string message, OutputType type = OutputType.Log)
    {
        var output = new OutputMessage
        {
            Type = type,
            Message = message,
            Timestamp = DateTime.Now
        };
        _outputs.Add(output);
    }

    /// <summary>
    /// 添加错误输出
    /// </summary>
    public void Error(string message)
    {
        var error = new OutputMessage
        {
            Type = OutputType.Error,
            Message = message,
            Timestamp = DateTime.Now
        };
        _errors.Add(error);
        _outputs.Add(error);
    }

    /// <summary>
    /// 清空输出
    /// </summary>
    public void Clear()
    {
        _outputs.Clear();
        _errors.Clear();
    }

    /// <summary>
    /// 获取所有输出
    /// </summary>
    public IReadOnlyList<OutputMessage> GetOutputs() => _outputs.AsReadOnly();

    /// <summary>
    /// 获取所有错误
    /// </summary>
    public IReadOnlyList<OutputMessage> GetErrors() => _errors.AsReadOnly();

    /// <summary>
    /// 获取标准输出文本
    /// </summary>
    public string GetOutputText() => string.Join(Environment.NewLine, _outputs.Where(o => o.Type != OutputType.Error).Select(o => o.Message));

    /// <summary>
    /// 获取错误文本
    /// </summary>
    public string GetErrorText() => string.Join(Environment.NewLine, _errors.Select(e => e.Message));

    /// <summary>
    /// 获取完整输出文本
    /// </summary>
    public string GetFullText() => string.Join(Environment.NewLine, _outputs.Select(o => $"[{o.Type}] {o.Message}"));
}

/// <summary>
/// 输出消息
/// </summary>
public class OutputMessage
{
    public OutputType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 输出类型
/// </summary>
public enum OutputType
{
    Log,
    Info,
    Warn,
    Error,
    Debug
}