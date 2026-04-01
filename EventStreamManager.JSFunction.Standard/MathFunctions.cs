namespace EventStreamManager.JSFunction.Standard;

public static class MathFunctions
{
    public static IEnumerable<FunctionMetadata> GetFunctions()
    {
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
    }
}