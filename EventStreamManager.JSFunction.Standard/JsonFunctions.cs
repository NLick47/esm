using System.Text.Json;

namespace EventStreamManager.JSFunction.Standard;

public static class JsonFunctions
{
      public static IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "json_parse",
            Category = "JSON",
            Description = "解析JSON字符串为对象（返回字典/列表结构，可在JS中直接使用）",
            FunctionDelegate = new Func<string, object?>(json =>
            {
                if (string.IsNullOrWhiteSpace(json))
                    return null;

                using var doc = JsonDocument.Parse(json);
                return ConvertJsonElementToObject(doc.RootElement);
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "json", Type = typeof(string), Description = "JSON字符串" }
            },
            ReturnType = typeof(object),
            Example = "var obj = json_parse('{\"name\":\"John\"}');"
        };

        yield return new FunctionMetadata
        {
            Name = "json_stringify",
            Category = "JSON",
            Description = "将对象转换为JSON字符串",
            FunctionDelegate = new Func<object?, string>(obj => JsonSerializer.Serialize(obj)),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "obj", Type = typeof(object), Description = "要转换的对象" }
            },
            ReturnType = typeof(string),
            Example = "var json = json_stringify({name: 'John'});"
        };

        yield return new FunctionMetadata
        {
            Name = "json_format",
            Category = "JSON",
            Description = "格式化JSON字符串",
            FunctionDelegate = new Func<string?, string>(json =>
            {
                using var doc = JsonDocument.Parse(json ?? "{}");
                return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "json", Type = typeof(string), Description = "JSON字符串" }
            },
            ReturnType = typeof(string),
            Example = "var formatted = json_format('{\"name\":\"John\"}');"
        };
    }

    /// <summary>
    /// 将 JsonElement 递归转换为可在脚本引擎中直接使用的对象（Dictionary/List）
    /// </summary>
    private static object? ConvertJsonElementToObject(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                var dict = new Dictionary<string, object?>();
                foreach (var prop in element.EnumerateObject())
                {
                    dict[prop.Name] = ConvertJsonElementToObject(prop.Value);
                }
                return dict;

            case JsonValueKind.Array:
                var list = new List<object?>();
                foreach (var item in element.EnumerateArray())
                {
                    list.Add(ConvertJsonElementToObject(item));
                }
                return list;

            case JsonValueKind.String:
                return element.GetString();

            case JsonValueKind.Number:
                if (element.TryGetInt32(out int intVal))
                    return intVal;
                if (element.TryGetInt64(out long longVal))
                    return longVal;
                return element.GetDouble();

            case JsonValueKind.True:
                return true;

            case JsonValueKind.False:
                return false;

            case JsonValueKind.Null:
                return null;

            default:
                return element.ToString();
        }
    }
}