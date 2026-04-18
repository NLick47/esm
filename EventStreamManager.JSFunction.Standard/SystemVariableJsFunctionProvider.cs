using System.Text.Json;

namespace EventStreamManager.JSFunction.Standard;

public class SystemVariableJsFunctionProvider : IJsFunctionProvider
{
    public string Name => "System Variable Functions";
    public string Description => "系统变量读取函数，支持在JS脚本中获取持久化的系统变量";
    public string Version => "1.0.0";

    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public SystemVariableJsFunctionProvider()
    {
        _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "system-variables.json");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "sys_var_get",
            Category = "SystemVariable",
            Description = "根据键名获取系统变量的值。如果变量不存在返回null。",
            FunctionDelegate = new Func<string, string?>(GetVariableValue),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "key", Type = typeof(string), Description = "变量键名" }
            },
            ReturnType = typeof(string),
            Example = @"var dbConn = sys_var_get('mysql_connection');"
        };
    }

    private string? GetVariableValue(string key)
    {
        if (!File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var variables = JsonSerializer.Deserialize<List<SystemVariableRecord>>(json, _jsonOptions);
            var variable = variables?.FirstOrDefault(v =>
                v.Key.Equals(key, StringComparison.OrdinalIgnoreCase));
            return variable?.Value;
        }
        catch
        {
            return null;
        }
    }

    private class SystemVariableRecord
    {
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
