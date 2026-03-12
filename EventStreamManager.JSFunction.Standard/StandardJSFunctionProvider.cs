using System.Text;
using System.Text.Json;
using System.Globalization;

namespace EventStreamManager.JSFunction.Standard;

public class StandardJsFunctionProvider : IJSFunctionProvider
{
    
    public string Name => "Standard JS Functions";
    public string Description => "提供基础的JS扩展函数Base64、JSON等";
    public string Version => "1.0.0";

    public IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "base64_encode",
            Category = "Base64",
            Description = "将字符串转换为Base64编码",
            FunctionDelegate = new Func<string, string>(str =>
                Convert.ToBase64String(Encoding.UTF8.GetBytes(str ?? string.Empty))),
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
            FunctionDelegate = new Func<string, string>(str => Encoding.UTF8.GetString(Convert.FromBase64String(str ?? string.Empty))),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "input", Type = typeof(string), Description = "Base64字符串" }
            },
            ReturnType = typeof(string),
            Example = "var decoded = base64_decode('SGVsbG8gV29ybGQ=');"
        };

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
            FunctionDelegate = new Func<string, string>(json =>
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

        yield return new FunctionMetadata
        {
            Name = "string_format",
            Category = "String",
            Description = "格式化字符串",
            FunctionDelegate = new Func<string, object[], string>((format, args) => string.Format(format ?? string.Empty, args)),
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
                string.Join(separator ?? ",", values.Select(v => v?.ToString() ?? string.Empty))),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "separator", Type = typeof(string), Description = "分隔符" },
                new() { Name = "values", Type = typeof(object[]), Description = "要连接的值" }
            },
            ReturnType = typeof(string),
            Example = "var result = string_join(',', [1,2,3,4]);"
        };

        yield return new FunctionMetadata
        {
            Name = "math_sum",
            Category = "Math",
            Description = "计算数字总和",
            FunctionDelegate = new Func<object[], double>(numbers =>
            {
                double sum = 0;
                foreach (var n in numbers)
                {
                    sum += Convert.ToDouble(n);
                }
                return sum;
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "numbers", Type = typeof(object[]), Description = "数字数组" }
            },
            ReturnType = typeof(double),
            Example = "var total = math_sum([1,2,3,4,5]);"
        };

        yield return new FunctionMetadata
        {
            Name = "math_average",
            Category = "Math",
            Description = "计算平均值",
            FunctionDelegate = new Func<object[], double>(numbers =>
            {
                double sum = 0;
                int count = 0;
                foreach (var n in numbers)
                {
                    sum += Convert.ToDouble(n);
                    count++;
                }
                return count > 0 ? sum / count : 0;
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "numbers", Type = typeof(object[]), Description = "数字数组" }
            },
            ReturnType = typeof(double),
            Example = "var avg = math_average([1,2,3,4,5]);"
        };
        
        
        // 添加到 GetFunctions() 方法中

yield return new FunctionMetadata
{
    Name = "datetime_combine",
    Category = "DateTime",
    Description = "将日期和时间拼接，可指定返回格式（默认返回DateTime对象，也可返回指定格式的字符串）",
    FunctionDelegate = new Func<string, string, string?, object?>((datePart, timePart, format) =>
    {
        try
        {
            // 解析日期部分
            DateTime date;
            if (DateTime.TryParse(datePart, out DateTime parsedDate))
            {
                date = parsedDate;
            }
            else
            {
                
                if (DateTime.TryParseExact(datePart, new[] { 
                    "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", 
                    "dd-MM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy" 
                }, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime exactDate))
                {
                    date = exactDate;
                }
                else
                {
                    throw new ArgumentException($"无法解析日期: {datePart}");
                }
            }

            // 解析时间部分
            TimeSpan time;
            if (TimeSpan.TryParse(timePart, out TimeSpan parsedTime))
            {
                time = parsedTime;
            }
            else
            {
               
                if (DateTime.TryParse(timePart, out DateTime timeTemp))
                {
                    time = timeTemp.TimeOfDay;
                }
                else if (timePart.Length <= 5 && timePart.Contains(':'))
                {
                    if (TimeSpan.TryParseExact(timePart, @"hh\:mm", CultureInfo.InvariantCulture, out TimeSpan exactTime))
                    {
                        time = exactTime;
                    }
                    else
                    {
                        throw new ArgumentException($"无法解析时间: {timePart}");
                    }
                }
                else
                {
                    throw new ArgumentException($"无法解析时间: {timePart}");
                }
            }

            // 组合日期和时间
            DateTime combined = date.Date.Add(time);

            // 如果指定了格式，返回格式化字符串
            if (!string.IsNullOrEmpty(format))
            {
                return combined.ToString(format);
            }

            // 默认返回 DateTime 对象
            return combined;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
        }
    }),
    Parameters = new List<FunctionParameter>
    {
        new() { 
            Name = "datePart", 
            Type = typeof(string), 
            Description = "日期部分（支持格式：yyyy-MM-dd, yyyy/MM/dd, yyyyMMdd, dd-MM-yyyy 等）" 
        },
        new() { 
            Name = "timePart", 
            Type = typeof(string), 
            Description = "时间部分（支持格式：HH:mm:ss, HH:mm, HHmmss 等）" 
        },
        new() { 
            Name = "format", 
            Type = typeof(string), 
            IsOptional = true, 
            Description = "可选：返回格式（如 'yyyy-MM-dd HH:mm:ss'），不指定则返回DateTime对象" 
        }
    },
    ReturnType = typeof(object),
    Example = @"
// 返回 DateTime 对象
var dt1 = datetime_combine('2024-01-01', '13:30:00');
var dt2 = datetime_combine('2024/01/01', '14:30');

// 返回指定格式字符串
var str1 = datetime_combine('2024-01-01', '13:30:00', 'yyyy-MM-dd HH:mm:ss');
var str2 = datetime_combine('2024-01-01', '13:30', 'yyyy年MM月dd日 HH时mm分');

// 各种格式示例
var dt3 = datetime_combine('20240101', '1330');     // 2024-01-01 13:30:00
var dt4 = datetime_combine('01-01-2024', '13:30');   // 2024-01-01 13:30:00
"
};

yield return new FunctionMetadata
{
    Name = "datetime_combine_now",
    Category = "DateTime",
    Description = "将指定日期与当前时间拼接",
    FunctionDelegate = new Func<string, string?, object?>((datePart, format) =>
    {
        try
        {
            // 解析日期部分
            DateTime date;
            if (DateTime.TryParse(datePart, out DateTime parsedDate))
            {
                date = parsedDate;
            }
            else
            {
                throw new ArgumentException($"无法解析日期: {datePart}");
            }

            // 使用当前时间
            DateTime now = DateTime.Now;
            DateTime combined = date.Date.Add(now.TimeOfDay);

            // 如果指定了格式，返回格式化字符串
            if (!string.IsNullOrEmpty(format))
            {
                return combined.ToString(format);
            }

            return combined;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
        }
    }),
    Parameters = new List<FunctionParameter>
    {
        new() { 
            Name = "datePart", 
            Type = typeof(string), 
            Description = "日期部分" 
        },
        new() { 
            Name = "format", 
            Type = typeof(string), 
            IsOptional = true, 
            Description = "可选：返回格式" 
        }
    },
    ReturnType = typeof(object),
    Example = "var dt = datetime_combine_now('2024-01-01'); // 返回 2024-01-01 当前时间"
};

yield return new FunctionMetadata
{
    Name = "datetime_combine_today",
    Category = "DateTime",
    Description = "将指定时间与今天日期拼接",
    FunctionDelegate = new Func<string, string?, object?>((timePart, format) =>
    {
        try
        {
            // 解析时间部分
            TimeSpan time;
            if (TimeSpan.TryParse(timePart, out TimeSpan parsedTime))
            {
                time = parsedTime;
            }
            else
            {
                throw new ArgumentException($"无法解析时间: {timePart}");
            }

            // 使用今天日期
            DateTime today = DateTime.Today;
            DateTime combined = today.Add(time);

            // 如果指定了格式，返回格式化字符串
            if (!string.IsNullOrEmpty(format))
            {
                return combined.ToString(format);
            }

            return combined;
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
        }
    }),
    Parameters = new List<FunctionParameter>
    {
        new() { 
            Name = "timePart", 
            Type = typeof(string), 
            Description = "时间部分" 
        },
        new() { 
            Name = "format", 
            Type = typeof(string), 
            IsOptional = true, 
            Description = "可选：返回格式" 
        }
    },
    ReturnType = typeof(object),
    Example = "var dt = datetime_combine_today('14:30:00'); // 返回 今天日期 + 14:30:00"
};

yield return new FunctionMetadata
{
    Name = "datetime_to_string",
    Category = "DateTime",
    Description = "将DateTime对象转换为指定格式的字符串",
    FunctionDelegate = new Func<object, string, string>((dateTimeObj, format) =>
    {
        if (dateTimeObj is DateTime dt)
        {
            return dt.ToString(format);
        }
        else if (dateTimeObj is string str)
        {
            if (DateTime.TryParse(str, out DateTime parsed))
            {
                return parsed.ToString(format);
            }

            throw new ArgumentException($"无法转换为DateTime: {str}");
        }

        throw new ArgumentException($"参数必须是DateTime对象或日期字符串");
    }),
    Parameters = new List<FunctionParameter>
    {
        new()
        {
            Name = "dateTimeObj",
            Type = typeof(object),
            Description = "DateTime对象或日期字符串"
        },
        new()
        {
            Name = "format",
            Type = typeof(string),
            Description = "输出格式（如 'yyyy-MM-dd HH:mm:ss'）"
        }
    },
    ReturnType = typeof(string),
    Example = "var str = datetime_to_string(datetime_combine('2024-01-01', '13:30'), 'yyyy年MM月dd日 HH时mm分');"
};
    }

    /// <summary>
    /// 将 JsonElement 递归转换为可在脚本引擎中直接使用的对象（Dictionary/List）
    /// </summary>
    private object? ConvertJsonElementToObject(JsonElement element)
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
                // 尝试返回最合适的数值类型（int, long, double）
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

    /// <summary>
    /// 格式化参数为字符串
    /// </summary>
    private string FormatArguments(object?[]? args)
    {
        if (args == null || args.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) sb.Append(' ');

            var arg = args[i];
            if (arg == null)
            {
                sb.Append("null");
            }
            else if (arg is string str)
            {
                sb.Append(str);
            }
            else if (arg.GetType().IsPrimitive)
            {
                sb.Append(arg);
            }
            else
            {
                try
                {
                    // 尝试JSON序列化对象
                    var json = JsonSerializer.Serialize(arg);
                    sb.Append(json);
                }
                catch
                {
                    sb.Append(arg.ToString());
                }
            }
        }
        return sb.ToString();
    }
    
}