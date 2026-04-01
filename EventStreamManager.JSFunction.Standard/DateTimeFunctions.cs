using System.Globalization;

namespace EventStreamManager.JSFunction.Standard;

public static class DateTimeFunctions
{
     public static IEnumerable<FunctionMetadata> GetFunctions()
    {
        yield return new FunctionMetadata
        {
            Name = "datetime_combine",
            Category = "DateTime",
            Description = "将日期和时间拼接，可指定返回格式（默认返回DateTime对象，也可返回指定格式的字符串）",
            FunctionDelegate = new Func<string, string, string?, object?>((datePart, timePart, format) =>
            {
                try
                {
                    var date = ParseDate(datePart);
                    var time = ParseTime(timePart);
                    var combined = date.Date.Add(time);

                    return !string.IsNullOrEmpty(format) ? combined.ToString(format) : combined;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
                }
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "datePart", Type = typeof(string), Description = "日期部分" },
                new() { Name = "timePart", Type = typeof(string), Description = "时间部分" },
                new() { Name = "format", Type = typeof(string), IsOptional = true, Description = "可选：返回格式" }
            },
            ReturnType = typeof(object),
            Example = GetDateTimeCombineExample()
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
                    var date = ParseDate(datePart);
                    var now = DateTime.Now;
                    var combined = date.Date.Add(now.TimeOfDay);

                    return !string.IsNullOrEmpty(format) ? combined.ToString(format) : combined;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
                }
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "datePart", Type = typeof(string), Description = "日期部分" },
                new() { Name = "format", Type = typeof(string), IsOptional = true, Description = "可选：返回格式" }
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
                    var time = ParseTime(timePart);
                    var today = DateTime.Today;
                    var combined = today.Add(time);

                    return !string.IsNullOrEmpty(format) ? combined.ToString(format) : combined;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"日期时间拼接失败: {ex.Message}");
                }
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "timePart", Type = typeof(string), Description = "时间部分" },
                new() { Name = "format", Type = typeof(string), IsOptional = true, Description = "可选：返回格式" }
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
                var dt = dateTimeObj switch
                {
                    DateTime d => d,
                    string s when DateTime.TryParse(s, out var parsed) => parsed,
                    _ => throw new ArgumentException($"参数必须是DateTime对象或日期字符串")
                };

                return dt.ToString(format);
            }),
            Parameters = new List<FunctionParameter>
            {
                new() { Name = "dateTimeObj", Type = typeof(object), Description = "DateTime对象或日期字符串" },
                new() { Name = "format", Type = typeof(string), Description = "输出格式" }
            },
            ReturnType = typeof(string),
            Example = "var str = datetime_to_string(datetime_combine('2024-01-01', '13:30'), 'yyyy年MM月dd日 HH时mm分');"
        };
    }

    private static DateTime ParseDate(string datePart)
    {
        if (DateTime.TryParse(datePart, out var parsedDate))
            return parsedDate;

        if (DateTime.TryParseExact(datePart, new[] { 
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyyMMdd", 
            "dd-MM-yyyy", "dd/MM/yyyy", "MM/dd/yyyy" 
        }, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exactDate))
            return exactDate;

        throw new ArgumentException($"无法解析日期: {datePart}");
    }

    private static TimeSpan ParseTime(string timePart)
    {
        if (TimeSpan.TryParse(timePart, out var parsedTime))
            return parsedTime;

        if (DateTime.TryParse(timePart, out var timeTemp))
            return timeTemp.TimeOfDay;

        if (timePart.Length <= 5 && timePart.Contains(':') && 
            TimeSpan.TryParseExact(timePart, @"hh\:mm", CultureInfo.InvariantCulture, out var exactTime))
            return exactTime;

        throw new ArgumentException($"无法解析时间: {timePart}");
    }

    private static string GetDateTimeCombineExample()
    {
        return @"
        // 返回 DateTime 对象
        var dt1 = datetime_combine('2024-01-01', '13:30:00');
        var dt2 = datetime_combine('2024/01/01', '14:30');

        // 返回指定格式字符串
        var str1 = datetime_combine('2024-01-01', '13:30:00', 'yyyy-MM-dd HH:mm:ss');
        var str2 = datetime_combine('2024-01-01', '13:30', 'yyyy年MM月dd日 HH时mm分');

        // 各种格式示例
        var dt3 = datetime_combine('20240101', '1330');     // 2024-01-01 13:30:00
        var dt4 = datetime_combine('01-01-2024', '13:30');   // 2024-01-01 13:30:00
        ";
    }
}