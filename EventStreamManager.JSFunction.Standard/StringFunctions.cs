namespace EventStreamManager.JSFunction.Standard;

public static class StringFunctions
{
    public static IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "string_format",
            Category = "String",
            Description = "格式化字符串",
            FunctionDelegate = new Func<string?, object[], string>((format, args) => 
                string.Format(format ?? string.Empty, args)),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "format", Type = typeof(string), Description = "格式字符串" },
                new() { Name = "args", Type = typeof(object[]), Description = "参数数组" }
            },
            ReturnType = typeof(string),
            Example = "var msg = string_format('Hello {0}', ['World']);"
        };

        yield return new FunctionMetadata
        {
            Name = "string_join",
            Category = "String",
            Description = "连接字符串数组",
            FunctionDelegate = new Func<string, object[], string>((separator, values) =>
                string.Join(separator, values.Select(v => v.ToString() ?? string.Empty))),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "separator", Type = typeof(string), Description = "分隔符" },
                new() { Name = "values", Type = typeof(object[]), Description = "要连接的值" }
            },
            ReturnType = typeof(string),
            Example = "var result = string_join(',', [1,2,3,4]);"
        };
    }
}