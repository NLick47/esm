using System.Text;

namespace EventStreamManager.JSFunction.Standard;

public static class Base64Functions
{
    public static IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "base64_encode",
            Category = "Base64",
            Description = "将字符串转换为Base64编码",
            FunctionDelegate = new Func<string, string>(str =>
                Convert.ToBase64String(Encoding.UTF8.GetBytes(str))),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "input", Type = typeof(string), Description = "要编码的字符串" }
            },
            ReturnType = typeof(string),
            Example = "var encoded = base64_encode('Hello World');"
        };

        yield return new FunctionMetadata
        {
            Name = "base64_decode",
            Category = "Base64",
            Description = "将Base64字符串解码",
            FunctionDelegate = new Func<string, string>(str => 
                Encoding.UTF8.GetString(Convert.FromBase64String(str))),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "input", Type = typeof(string), Description = "Base64字符串" }
            },
            ReturnType = typeof(string),
            Example = "var decoded = base64_decode('SGVsbG8gV29ybGQ=');"
        };
    }
}