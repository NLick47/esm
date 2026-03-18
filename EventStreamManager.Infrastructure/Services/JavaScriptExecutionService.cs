using System.Diagnostics;
using System.Text;
using EventStreamManager.Infrastructure.Models.Execution;
using EventStreamManager.JSFunction;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Microsoft.Extensions.Logging;
using OutputMessage = EventStreamManager.Infrastructure.Models.Execution.OutputMessage;


namespace EventStreamManager.Infrastructure.Services;

public class JavaScriptExecutionService : IJavaScriptExecutionService, IDisposable
{
    private readonly JsFunctionRegistry _functionRegistry;
    private readonly ILogger<JavaScriptExecutionService> _logger;
    private readonly ExecutionServiceOptions _options;
    private readonly SemaphoreSlim _semaphore;

    public JavaScriptExecutionService(
        JsFunctionRegistry functionRegistry,
        ILogger<JavaScriptExecutionService> logger)
    {
        _functionRegistry = functionRegistry;
        _logger = logger;
        _options = new ExecutionServiceOptions();
        _semaphore = new SemaphoreSlim(_options.MaxConcurrentExecutions);
    }

    public async Task<ExecutionResult> ExecuteProcessAsync(string script, object? inputData = null)
    {
        return await ExecuteProcessInternalAsync(script, inputData, new ExecutionOptions());
    }

    public async Task<ExecutionResult> ExecuteProcessAsync(ExecutionOptions options, string script, object? inputData = null)
    {
        return await ExecuteProcessInternalAsync(script, inputData, options);
    }


    public ValidationResult ValidateScript(string script)
    {
        var result = new ValidationResult();

        try
        {
            var engine = CreateEngine(new ExecutionOptions { CaptureConsoleOutput = true }, new ScriptOutput());
            engine.Execute(script);

            var hasProcessFunction = engine.Evaluate("typeof process === 'function'").AsBoolean();
            result.HasProcessFunction = hasProcessFunction;

            if (hasProcessFunction)
            {
                var hasProcessResultClass = engine.Evaluate("typeof ProcessResult === 'function'").AsBoolean();

                try
                {
                    var testData = new { test = "data" };
                    engine.SetValue("testData", testData);
                    engine.Evaluate("process(testData)");

                    result.IsValid = true;
                    result.Message = hasProcessResultClass
                        ? "脚本有效，包含process函数和ProcessResult类，且语法正确"
                        : "脚本有效，包含process函数（建议同时定义ProcessResult类），且语法正确";
                }
                catch (JavaScriptException ex)
                {
                    result.IsValid = false;
                    result.Message = $"process函数语法错误: {ex.Message}";
                    result.LineNumber = ex.Location.Start.Line;
                    result.Column = ex.Location.Start.Column;
                    result.Source = ex.Source;
                }
            }
            else
            {
                result.IsValid = false;
                result.Message = "脚本中未找到process函数，请确保定义了function process(data) {...}";
            }
        }
        catch (JavaScriptException ex)
        {
            result.IsValid = false;
            result.HasProcessFunction = false;
            result.Message = ex.Message;
            result.LineNumber = ex.Location.Start.Line;
            result.Column = ex.Location.Start.Column;
            result.Source = ex.Source;
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.HasProcessFunction = false;
            result.Message = $"验证失败: {ex.Message}";
        }

        return result;
    }

    public IEnumerable<FunctionInfo> GetAvailableFunctions()
    {
        return _functionRegistry.GetAvailableFunctions().Select(f => new FunctionInfo
        {
            Name = f.Name,
            Description = f.Description,
            Category = f.Category,
            Example = f.Example,
            ProviderName = f.ProviderName,
            ProviderVersion = f.ProviderVersion,
            Parameters = f.Parameters.Select(p => new ParameterInfo()
            {
                Name = p.Name,
                IsOptional = p.IsOptional,
                DefaultValue = p.DefaultValue
            }).ToList(),
            ReturnType = f.ReturnType.Name
        });
    }

    public IEnumerable<string> GetAllCategories()
    {
        return _functionRegistry.GetAllCategories();
    }

    private async Task<ExecutionResult> ExecuteProcessInternalAsync(string script, object? inputData, ExecutionOptions options)
    {
        var result = new ExecutionResult { InputData = inputData };
        var stopwatch = Stopwatch.StartNew();

        if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(_options.WaitTimeoutSeconds)))
        {
            throw new TimeoutException("系统繁忙，请稍后重试");
        }

        Engine? engine = null;

        try
        {
            var output = options.CaptureConsoleOutput ? new ScriptOutput() : null;

            // 每次执行都创建全新的引擎
            engine = CreateEngine(options, output);

            // 执行用户脚本（加载函数定义）
            engine.Execute(script);

            // 检查 process 函数是否存在
            var hasProcess = engine.Evaluate("typeof process === 'function'").AsBoolean();
            if (!hasProcess)
            {
                throw new InvalidOperationException("脚本中未定义process函数");
            }

            // 准备输入数据并调用 process
            var jsValue = ConvertToJsValue(engine, inputData);
            var processResult = engine.Invoke("process", jsValue);

            // 处理返回值
            result.ReturnValue = ConvertJsValueToObject(processResult);
            result.ReturnType = processResult.Type.ToString();

            // 尝试解析为 ProcessResult 对象
            if (processResult.IsObject())
            {
                var obj = processResult.AsObject();
                if (obj.HasOwnProperty("needToSend") || obj.HasOwnProperty("setSuccess") || obj.HasOwnProperty("setFailure"))
                {
                    if (obj.HasOwnProperty("needToSend"))
                    {
                        var needToSend = obj.Get("needToSend");
                        if (needToSend.IsBoolean())
                            result.NeedToSend = needToSend.AsBoolean();
                    }

                    if (obj.HasOwnProperty("reason"))
                    {
                        var reason = obj.Get("reason");
                        if (!reason.IsUndefined() && !reason.IsNull())
                            result.Reason = reason.ToString();
                    }

                    if (obj.HasOwnProperty("error"))
                    {
                        var error = obj.Get("error");
                        if (!error.IsUndefined() && !error.IsNull())
                            result.ProcessError = ConvertJsValueToObject(error)?.ToString();
                    }

                    if (obj.HasOwnProperty("requestInfo"))
                    {
                        var requestInfo = obj.Get("requestInfo");
                        if (!requestInfo.IsUndefined() && !requestInfo.IsNull())
                            result.RequestInfo = ConvertJsValueToObject(requestInfo)?.ToString();
                    }
                }
            }

            if (output != null)
            {
                result.Output = output.GetOutputs().Select(o => new OutputMessage
                {
                    Type = o.Type.ToString(),
                    Message = o.Message,
                    Timestamp = o.Timestamp
                }).ToList();
            }

            result.Success = true;
        }
        catch (JavaScriptException ex)
        {
            _logger.LogWarning(ex, "JavaScript执行错误，输入数据: {@InputData}", inputData);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ErrorStack = ex.StackTrace;
        }
        catch (TimeoutException ex)
        {
            _logger.LogWarning(ex, "脚本执行超时");
            result.Success = false;
            result.ErrorMessage = "脚本执行超时";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行脚本时发生未预期错误，输入数据: {@InputData}", inputData);
            result.Success = false;
            result.ErrorMessage = $"执行错误: {ex.Message}";
        }
        finally
        {
            _semaphore.Release();
            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            if (engine is IDisposable disposableEngine)
                disposableEngine.Dispose();
        }

        return result;
    }

    /// <summary>
    /// 创建 Jint 引擎实例
    /// </summary>
    private Engine CreateEngine(ExecutionOptions options, ScriptOutput? output = null)
    {
        var engine = new Engine(cfg =>
        {
            cfg.LimitRecursion(options.MaxRecursionDepth);
            cfg.MaxStatements(options.MaxStatements);
            cfg.TimeoutInterval(TimeSpan.FromSeconds(options.TimeoutSeconds));
        });

        // 注入所有注册的全局函数
        engine = _functionRegistry.InjectToEngine(engine);

        if (output != null)
        {
            InjectConsoleFunctions(engine, output);
        }

        return engine;
    }

    /// <summary>
    /// 注入控制台函数，每次调用都会重新绑定到当前 ScriptOutput 实例
    /// </summary>
    private void InjectConsoleFunctions(Engine engine, ScriptOutput output)
    {
        engine.SetValue("console_log", new Action<object?[]?>(args =>
            output.Write(FormatArguments(args))));

        engine.SetValue("console_info", new Action<object?[]?>(args =>
            output.Write(FormatArguments(args), OutputType.Info)));

        engine.SetValue("console_warn", new Action<object?[]?>(args =>
            output.Write(FormatArguments(args), OutputType.Warn)));

        engine.SetValue("console_error", new Action<object?[]?>(args =>
            output.Error(FormatArguments(args))));

        engine.SetValue("console_debug", new Action<object?[]?>(args =>
            output.Write(FormatArguments(args), OutputType.Debug)));

        engine.SetValue("console_clear", output.Clear);

        // 适配器脚本，使 console 函数支持 ...args 语法
        string adapterScript = @"
            var originalConsoleInfo = console_info;
            var originalConsoleLog = console_log;
            var originalConsoleWarn = console_warn;
            var originalConsoleError = console_error;
            var originalConsoleDebug = console_debug;

            console_info = function(...args) { originalConsoleInfo(args); };
            console_log = function(...args) { originalConsoleLog(args); };
            console_warn = function(...args) { originalConsoleWarn(args); };
            console_error = function(...args) { originalConsoleError(args); };
            console_debug = function(...args) { originalConsoleDebug(args); };
        ";

        engine.Execute(adapterScript);
    }

    private string FormatArguments(object?[]? args)
    {
        if (args == null || args.Length == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var arg in args)
        {
            if (sb.Length > 0)
                sb.Append(' ');

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
                    var options = new System.Text.Json.JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = false
                    };
                    var json = System.Text.Json.JsonSerializer.Serialize(arg, options);
                    sb.Append(json);
                }
                catch
                {
                    sb.Append(arg);
                }
            }
        }
        return sb.ToString();
    }

    private JsValue ConvertToJsValue(Engine engine, object? value)
    {
        return value == null ? JsValue.Null : JsValue.FromObject(engine, value);
    }

    private object? ConvertJsValueToObject(JsValue value)
    {
        if (value.IsNull() || value.IsUndefined())
            return null;

        if (value.IsBoolean())
            return value.AsBoolean();

        if (value.IsNumber())
            return value.AsNumber();

        if (value.IsString())
            return value.AsString();

        if (value.IsDate())
            return value.AsDate().ToDateTime();

        if (value.IsArray())
        {
            var array = value.AsArray();
            var result = new List<object?>();
            foreach (var item in array)
                result.Add(ConvertJsValueToObject(item));
            return result;
        }

        if (value.IsObject())
        {
            var obj = value.AsObject();
            var result = new Dictionary<string, object?>();
            foreach (var key in obj.GetOwnPropertyKeys())
                result[key.AsString()] = ConvertJsValueToObject(obj.Get(key));
            return result;
        }

        return value.ToString();
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }

    private class ExecutionServiceOptions
    {
        public int MaxConcurrentExecutions { get; set; } = 10;
        public int WaitTimeoutSeconds { get; set; } = 5;
    }
}