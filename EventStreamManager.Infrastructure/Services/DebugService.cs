using System.Text.Json;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Models.Execution.Debug;
using EventStreamManager.Infrastructure.Models.Execution.Parameter;
using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services
{
    /// <summary>
    /// 通用调试服务实现
    /// </summary>
    public class DebugService : IDebugService
    {
        private readonly IJavaScriptExecutionService _jsService;
        private readonly ILogger<DebugService> _logger;
        private readonly IProcessorService _processorService;
        private readonly ISqlSugarContext _sqlSugarContext;
        private readonly IDatabaseSchemeService _databaseSchemeService;
        private readonly IEventListenerConfigService _eventListenerConfigService;
        private readonly IInterfaceConfigService _interfaceConfigService;
        private readonly IHttpSendService _httpSendService;
        
        public DebugService(
            IJavaScriptExecutionService jsService,
            ILogger<DebugService> logger,
            IProcessorService processorService,
            ISqlSugarContext sqlSugarContext,
            IDatabaseSchemeService databaseSchemeService,
            IEventListenerConfigService eventListenerConfigService,
            IInterfaceConfigService interfaceConfigService,
            IHttpSendService httpSendService)
        {
            _jsService = jsService;
            _logger = logger;
            _processorService = processorService;
            _sqlSugarContext = sqlSugarContext;
            _databaseSchemeService = databaseSchemeService;
            _eventListenerConfigService = eventListenerConfigService;
            _interfaceConfigService = interfaceConfigService;
            _httpSendService = httpSendService;
        }

        #region 普通调试

        /// <inheritdoc />
        public async Task<DebugResponse> ExecuteDebugAsync(DebugRequest request)
        {
            var startTime = DateTime.Now;
            var logEntries = new List<DebugLogEntry>();

            try
            {
                _logger.LogInformation("开始普通调试 - 处理器ID: {ProcessorId}, 数据库: {DatabaseType}, 事件ID: {EventId}",
                    request.ProcessorId, request.DatabaseType, request.EventId);

                AddLog(logEntries, "info", $"开始{GetDebugTypeName(DebugType.Normal)}");

                //获取处理器
                var processor = await GetProcessorAsync(request.ProcessorId, logEntries);
                if (processor == null)
                    return CreateErrorResponse<DebugResponse>(logEntries, startTime, $"未找到处理器: {request.ProcessorId}");

                //获取数据库配置
                if (!await EnsureDatabaseConfigAsync(request.DatabaseType, logEntries))
                    return CreateErrorResponse<DebugResponse>(logEntries, startTime, $"未找到数据库类型 {request.DatabaseType} 的激活配置");

                //获取事件监听配置
                var eventConfig = await GetEventListenerConfigAsync(request.DatabaseType, logEntries);
                if (eventConfig == null)
                    return CreateErrorResponse<DebugResponse>(logEntries, startTime, $"未找到事件监听配置 {request.DatabaseType}");

                //获取事件数据
                var eventData = await GetEventDataAsync(request.DatabaseType, eventConfig, request.EventCode, request.EventId, logEntries);
                if (eventData == null)
                    return CreateErrorResponse<DebugResponse>(logEntries, startTime, "获取事件数据失败");

                //执行SQL查询扩展数据
                var rows = await ExecuteSqlQueryForProcessorAsync(processor, eventData, request.DatabaseType, logEntries);

                //构建增强数据对象
                var enhancedData = BuildEnhancedData(eventData, processor, rows, request.DatabaseType);

                //执行JavaScript处理器
                var (executionResult, _) = await ExecuteJavaScriptCodeAsync(processor.Code, enhancedData, logEntries);

                //构建响应
                return BuildDebugResponse<DebugResponse>(logEntries, startTime, executionResult, enhancedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "普通调试执行失败");
                AddLog(logEntries, "error", $"调试执行异常: {ex.Message}");
                return CreateErrorResponse<DebugResponse>(logEntries, startTime, $"调试执行失败: {ex.Message}");
            }
        }

        #endregion

        #region Examine调试

        public async Task<EditorDebugResponse> ExecuteExamineDebugAsync(EditorDebugRequest request)
        {
            var startTime = DateTime.Now;
            var logEntries = new List<DebugLogEntry>();

            try
            {
                _logger.LogInformation("开始Examine调试 - ExamineID: {ExamineId}, 数据库类型: {DatabaseType}",
                    request.ExamineId, request.DatabaseType);

                AddLog(logEntries, "info", $"开始{GetDebugTypeName(DebugType.Normal)} (Examine)");
                
                JSProcessor? processor = null;
                if (!string.IsNullOrEmpty(request.ProcessorId))
                {
                    processor = await GetProcessorAsync(request.ProcessorId, logEntries);
                    if (processor == null)
                        return CreateErrorResponse<EditorDebugResponse>(logEntries, startTime, $"未找到处理器: {request.ProcessorId}");
                }
                
                if (string.IsNullOrEmpty(request.SqlTemplate))
                {
                    AddLog(logEntries, "error", "未配置查询SQL");
                    return CreateErrorResponse<EditorDebugResponse>(logEntries, startTime, "未配置查询SQL");
                }

                if (!await EnsureDatabaseConfigAsync(request.DatabaseType, logEntries))
                    return CreateErrorResponse<EditorDebugResponse>(logEntries, startTime, $"未找到数据库类型 {request.DatabaseType} 的激活配置");

                var jsCode = request.JavaScriptCode ?? processor?.Code;
                if (string.IsNullOrEmpty(jsCode))
                {
                    AddLog(logEntries, "error", "JavaScript代码不能为空");
                    return CreateErrorResponse<EditorDebugResponse>(logEntries, startTime, "JavaScript代码不能为空");
                }

                var parameters = new { strexamineId = request.ExamineId };
                var sql = request.SqlTemplate.Replace("${strEventReferenceId}", "@strexamineId");
                var rows = await ExecuteSqlQueryAsync(request.DatabaseType, sql, parameters, logEntries);

                var enhancedData = BuildEnhancedDataForExamine(rows, request.DatabaseType, processor);

                //执行JavaScript代码
                var (executionResult, _) = await ExecuteJavaScriptCodeAsync(jsCode, enhancedData, logEntries);

                return BuildDebugResponse<EditorDebugResponse>(logEntries, startTime, executionResult, enhancedData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Examine调试执行失败");
                AddLog(logEntries, "error", $"调试执行异常: {ex.Message}");
                return CreateErrorResponse<EditorDebugResponse>(logEntries, startTime, $"调试执行失败: {ex.Message}");
            }
        }

        #endregion

        #region 接口调试

        /// <inheritdoc />
        public async Task<InterfaceDebugResponse> DebugInterfaceAsync(InterfaceDebugRequest request)
        {
            var startTime = DateTime.Now;
            var logEntries = new List<DebugLogEntry>();

            try
            {
                _logger.LogInformation("开始接口调试 - 接口配置ID: {ConfigId}, 处理器ID: {ProcessorId}, 数据库: {DatabaseType}",
                    request.InterfaceConfigId, request.ProcessorId, request.DatabaseType);

                AddLog(logEntries, "info", $"开始{GetDebugTypeName(DebugType.Interface)}");

                //获取接口配置
                var interfaceConfig = await GetInterfaceConfigAsync(request.InterfaceConfigId, logEntries);
                if (interfaceConfig == null)
                    return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, $"未找到接口配置: {request.InterfaceConfigId}");

                //获取处理器
                var processor = await GetProcessorAsync(request.ProcessorId, logEntries);
                if (processor == null)
                    return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, $"未找到处理器: {request.ProcessorId}");

                //获取数据库配置
                if (!await EnsureDatabaseConfigAsync(request.DatabaseType, logEntries))
                    return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, $"未找到数据库类型 {request.DatabaseType} 的激活配置");

                //获取事件监听配置
                var eventConfig = await GetEventListenerConfigAsync(request.DatabaseType, logEntries);
                if (eventConfig == null)
                    return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, $"未找到事件监听配置 {request.DatabaseType}");

                //获取事件数据
                var eventData = await GetEventDataAsync(request.DatabaseType, eventConfig, request.EventCode, request.EventId, logEntries);
                if (eventData == null)
                    return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, "获取事件数据失败");

                //执行SQL查询扩展数据
                var rows = await ExecuteSqlQueryForProcessorAsync(processor, eventData, request.DatabaseType, logEntries);

                //构建增强数据对象
                var enhancedData = BuildEnhancedData(eventData, processor, rows, request.DatabaseType);

                //执行JavaScript处理器
                var (executionResult, processorExecutionTime) = await ExecuteJavaScriptCodeAsync(processor.Code, enhancedData, logEntries);

                if (!executionResult.Success)
                    return BuildInterfaceProcessorErrorResponse(logEntries, startTime, processorExecutionTime, executionResult);

                //如果不需要发送，直接返回
                if (!executionResult.NeedToSend)
                    return BuildInterfaceNoSendResponse(logEntries, startTime, processorExecutionTime, executionResult);

                //构建并发送HTTP请求
                var sendDebugInfo = await SendHttpRequestAsync(
                    interfaceConfig, request.DatabaseType, executionResult, eventData, logEntries);

                //构建响应
                return BuildInterfaceSuccessResponse(
                    logEntries, startTime, processorExecutionTime, sendDebugInfo.ExecutionTimeMs,
                    executionResult, sendDebugInfo.RequestInfo,new ResponseInfo()
                    {
                        StatusCode = sendDebugInfo.Result.StatusCode,
                        StatusMessage = sendDebugInfo.Result.ErrorMessage,
                        Body = sendDebugInfo.Result.ResponseContent,
                        IsSuccess = sendDebugInfo.Result.Success,
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "接口调试执行失败");
                AddLog(logEntries, "error", $"调试执行异常: {ex.Message}");
                return CreateErrorResponse<InterfaceDebugResponse>(logEntries, startTime, $"调试执行失败: {ex.Message}");
            }
        }

        #endregion

        #region 私有辅助方法

        private string GetDebugTypeName(DebugType type)
        {
            return type switch
            {
                DebugType.Normal => "普通调试",
                DebugType.Interface => "接口调试",
                _ => "调试"
            };
        }

        private void AddLog(List<DebugLogEntry> logs, string type, string message)
        {
            logs.Add(new DebugLogEntry
            {
                Type = type,
                Message = message,
                Timestamp = DateTime.Now
            });
        }

        // 获取处理器
        private async Task<JSProcessor?> GetProcessorAsync(string processorId, List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 获取处理器信息");
            var processor = await _processorService.GetByIdAsync(processorId);
            if (processor != null)
                AddLog(logs, "info", $"处理器: {processor.Name}");
            return processor;
        }

        // 确保数据库配置存在
        private async Task<bool> EnsureDatabaseConfigAsync(string databaseType, List<DebugLogEntry> logs)
        {
            var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(databaseType);
            if (activeConfig == null)
            {
                AddLog(logs, "error", $"未找到数据库类型 {databaseType} 的激活配置");
                return false;
            }
            return true;
        }

        // 获取事件监听配置
        private async Task<EventConfig?> GetEventListenerConfigAsync(string databaseType, List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 获取事件监听配置");
            var eventConfig = await _eventListenerConfigService.GetConfigByTypeAsync(databaseType);
            if (eventConfig == null)
                AddLog(logs, "error", $"未找到事件监听配置 {databaseType}");
            return eventConfig;
        }

        // 获取事件数据
        private async Task<Event?> GetEventDataAsync(
            string databaseType,
            EventConfig eventConfig,
            string? eventCode,
            string? eventId,
            List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 获取事件数据");
            var client = await _sqlSugarContext.GetClientAsync(databaseType);

            if (string.IsNullOrEmpty(eventId))
            {
                AddLog(logs, "info", $"随机查询事件数据: EventCode={eventCode}");
                var count = await client.Queryable<Event>()
                    .AS(eventConfig.TableName)
                    .Where(e => e.EventCode == eventCode)
                    .CountAsync();

                if (count > 0)
                {
                    var randomIndex = new Random().Next(0, count);
                    var eventData = await client.Queryable<Event>()
                        .AS(eventConfig.TableName)
                        .Where(e => e.EventCode == eventCode)
                        .Skip(randomIndex)
                        .FirstAsync();
                    AddLog(logs, "success", $"事件查询成功: ID={eventData.Id}, Code={eventData.EventCode}, Name={eventData.EventName}");
                    return eventData;
                }

                AddLog(logs, "error", $"未找到事件码为: {eventCode} 的数据");
                return null;
            }
            else
            {
                AddLog(logs, "info", $"查询事件数据: EventId={eventId}");
                var eventData = await client.Queryable<Event>().AS(eventConfig.TableName)
                    .Where(e => e.Id.ToString() == eventId)
                    .FirstAsync();

                if (eventData == null)
                    AddLog(logs, "error", $"未找到事件ID为: {eventId} 的数据");
                else
                    AddLog(logs, "success", $"事件查询成功: ID={eventData.Id}, Code={eventData.EventCode}, Name={eventData.EventName}");

                return eventData;
            }
        }
        
        private async Task<List<Dictionary<string, object>>> ExecuteSqlQueryForProcessorAsync(
            JSProcessor processor,
            Event eventData,
            string databaseType,
            List<DebugLogEntry> logs)
        {
            if (string.IsNullOrEmpty(processor.SqlTemplate))
                return new List<Dictionary<string, object>>();

            var sql = processor.SqlTemplate
                .Replace("${strEventReferenceId}", $"'{eventData.StrEventReferenceId}'")
                .Replace("${eventId}", eventData.Id.ToString())
                .Replace("${eventType}", $"'{eventData.EventType}'")
                .Replace("${eventCode}", $"'{eventData.EventCode}'")
                .Replace("${operatorCode}", $"'{eventData.OperatorCode}'");

            return await ExecuteSqlQueryAsync(databaseType, sql, null, logs);
        }

        //SQL执行方法
        private async Task<List<Dictionary<string, object>>> ExecuteSqlQueryAsync(
            string databaseType,
            string sql,
            object? parameters,
            List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 执行SQL查询扩展数据");
            AddLog(logs, "info", $"执行SQL: {sql}");

            try
            {
                var rows = await _sqlSugarContext.ExecuteQueryAsync(databaseType, sql, parameters);
                AddLog(logs, "success", $"扩展数据查询完成，共 {rows.Count} 行");

                if (rows.Count > 0)
                {
                    AddLog(logs, "output", "查询结果: " + JsonSerializer.Serialize(rows, new JsonSerializerOptions { WriteIndented = true }));
                }

                return rows;
            }
            catch (Exception ex)
            {
                AddLog(logs, "warn", $"扩展数据查询失败: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }

        // 构建增强数据（用于普通/接口调试）
        private EnhancedQueryData BuildEnhancedData(
            Event eventData,
            JSProcessor processor,
            List<Dictionary<string, object>> rows,
            string databaseType)
        {
            return new EnhancedQueryData
            {
                Rows = rows,
                Database = new DatabaseInfo { Type = databaseType },
                Context = new ContextInfo
                {
                    EventId = eventData.Id.ToString(),
                    strEventReferenceId = eventData.StrEventReferenceId,
                    EventType = eventData.EventType,
                    EventName = eventData.EventName,
                    EventCode = eventData.EventCode,
                    OperatorName = eventData.OperatorName,
                    OperatorCode = eventData.OperatorCode,
                    CreateDatetime = eventData.CreateDatetime,
                    ExtenData = eventData.ExtenData ?? ""
                },
                Processor = new ProcessorInfo
                {
                    Id = processor.Id,
                    Name = processor.Name,
                    Enabled = processor.Enabled
                }
            };
        }

        // 构建增强数据（用于Examine调试）
        private EnhancedQueryData BuildEnhancedDataForExamine(
            List<Dictionary<string, object>> rows,
            string databaseType,
            JSProcessor? processor)
        {
            return new EnhancedQueryData
            {
                Rows = rows,
                Database = new DatabaseInfo { Type = databaseType },
                Context = new ContextInfo(), // 无事件上下文
                Processor = processor == null ? new ProcessorInfo() : new ProcessorInfo
                {
                    Id = processor.Id,
                    Name = processor.Name,
                    Enabled = processor.Enabled
                }
            };
        }

        // 执行JavaScript代码
        private async Task<(Infrastructure.Models.Execution.ExecutionResult Result, long ExecutionTime)> ExecuteJavaScriptCodeAsync(
            string code,
            EnhancedQueryData enhancedData,
            List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 执行JavaScript代码");
            var processorStartTime = DateTime.Now;
            var executionResult = await _jsService.ExecuteProcessAsync(code, enhancedData);
            var processorExecutionTime = (long)(DateTime.Now - processorStartTime).TotalMilliseconds;

            // 合并执行过程中的输出日志
            foreach (var output in executionResult.Output)
            {
                AddLog(logs, output.Type, output.Message);
            }

            if (!executionResult.Success)
            {
                AddLog(logs, "error", $"JavaScript执行失败: {executionResult.ErrorMessage}");
            }
            else
            {
                AddLog(logs, executionResult.NeedToSend ? "info" : "warn",
                    executionResult.NeedToSend
                        ? "✅ 处理器判定需要发送数据"
                        : $"⏭️ 处理器判定不需要发送数据: {executionResult.Reason}");
            }

            return (executionResult, processorExecutionTime);
        }

   
        private async Task<InterfaceConfig?> GetInterfaceConfigAsync(string configId, List<DebugLogEntry> logs)
        {
            AddLog(logs, "info", "步骤: 获取接口配置信息");
            var config = await _interfaceConfigService.GetConfigByIdAsync(configId);
            if (config != null)
                AddLog(logs, "info", $"接口配置: {config.Name}, URL: {config.Url}, 方法: {config.Method}");
            return config;
        }

     
        private async Task<HttpSendDebugInfo> SendHttpRequestAsync(
            InterfaceConfig interfaceConfig,
            string databaseType,
            Infrastructure.Models.Execution.ExecutionResult executionResult,
            Event eventData,
            List<DebugLogEntry> logs)
        {
            // 构建请求体
            string requestBody = BuildRequestBody(interfaceConfig, executionResult, eventData, logs);
            
            AddLog(logs, "info", $"发送{interfaceConfig.Method}请求到 {interfaceConfig.Url}");
            AddLog(logs, "info", $"请求体大小: {requestBody.Length} 字符");

            try
            {
                //发送请求
                var resultDebug = await _httpSendService.SendWithDebugAsync(databaseType, interfaceConfig, requestBody);

                AddLog(logs, "info", $"请求耗时: {resultDebug.ExecutionTimeMs}ms");
                AddLog(logs, "info", $"响应状态: {resultDebug.Result?.StatusCode ?? 0}");

                if (resultDebug.Result?.Success == true)
                {
                    AddLog(logs, "success", "✅ 接口请求成功");
                }
                else
                {
                    AddLog(logs, "error", $"❌ 接口请求失败: {resultDebug.Result?.StatusCode} - {resultDebug.Result?.ErrorMessage}");
                }

                if (!string.IsNullOrEmpty(resultDebug.Result?.ResponseContent))
                {
                    var responsePreview = resultDebug.Result.ResponseContent.Length > 1000 
                        ? resultDebug.Result.ResponseContent.Substring(0, 1000) + "..." 
                        : resultDebug.Result.ResponseContent;
                    AddLog(logs, "output", "响应体: " + responsePreview);
                }

        
                if (resultDebug.RequestInfo == null)
                {
                    resultDebug.RequestInfo = new RequestInfo
                    {
                        Url = interfaceConfig.Url,
                        Method = interfaceConfig.Method,
                        Headers = interfaceConfig.Headers?.ToDictionary(h => h.Key, h => h.Value) ?? new Dictionary<string, string>(),
                        Body = requestBody
                    };
                }

                return resultDebug;
            }
            catch (TaskCanceledException)
            {
                AddLog(logs, "error", $"❌ 请求超时 (超时时间: {interfaceConfig.Timeout}秒)");
                
                return new HttpSendDebugInfo
                {
                    ExecutionTimeMs = 0,
                    Result = new SendResult
                    {
                        Success = false,
                        StatusCode = 408,
                        ErrorMessage = $"请求超时 (超时时间: {interfaceConfig.Timeout}秒)"
                    },
                    RequestInfo = new RequestInfo
                    {
                        Url = interfaceConfig.Url,
                        Method = interfaceConfig.Method,
                        Headers = interfaceConfig.Headers?.ToDictionary(h => h.Key, h => h.Value) ?? new Dictionary<string, string>(),
                        Body = requestBody
                    }
                };
            }
            catch (Exception ex)
            {
                AddLog(logs, "error", $"❌ 请求异常: {ex.Message}");
                
             
                return new HttpSendDebugInfo
                {
                    ExecutionTimeMs = 0,
                    Result = new SendResult
                    {
                        Success = false,
                        StatusCode = 500,
                        ErrorMessage = ex.Message
                    },
                    RequestInfo = new RequestInfo
                    {
                        Url = interfaceConfig.Url,
                        Method = interfaceConfig.Method,
                        Headers = interfaceConfig.Headers?.ToDictionary(h => h.Key, h => h.Value) ?? new Dictionary<string, string>(),
                        Body = requestBody
                    }
                };
            }
        }

        private string BuildRequestBody(
            InterfaceConfig interfaceConfig,
            Infrastructure.Models.Execution.ExecutionResult executionResult,
            Event eventData,
            List<DebugLogEntry> logs)
        {
            try
            {
                var processedData = executionResult.RequestInfo ?? "{}";
                var template = interfaceConfig.RequestTemplate;

                var body = template
                    .Replace("${data}", processedData)
                    .Replace("${timestamp}", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())
                    .Replace("${eventId}", eventData.Id.ToString())
                    .Replace("${eventCode}", eventData.EventCode);

                // 添加更多变量替换
                if (!string.IsNullOrEmpty(eventData.StrEventReferenceId))
                {
                    body = body.Replace("${strEventReferenceId}", eventData.StrEventReferenceId);
                }

                if (!string.IsNullOrEmpty(eventData.OperatorCode))
                {
                    body = body.Replace("${operatorCode}", eventData.OperatorCode);
                }

                AddLog(logs, "output", "请求体: " + body);
                return body;
            }
            catch (Exception ex)
            {
                AddLog(logs, "error", $"构建请求体失败: {ex.Message}");
                throw;
            }
        }

        // 泛型错误响应创建
        private T CreateErrorResponse<T>(List<DebugLogEntry> logs, DateTime startTime, string errorMessage) where T : class, IDebugResponse, new()
        {
            return new T
            {
                Success = false,
                ErrorMessage = errorMessage,
                Logs = logs,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds
            };
        }

        // 构建普通/Examine调试响应
        private T BuildDebugResponse<T>(
            List<DebugLogEntry> logs,
            DateTime startTime,
            Infrastructure.Models.Execution.ExecutionResult executionResult,
            EnhancedQueryData enhancedData) where T : class, IDebugResponse, new()
        {
            var response = new T
            {
                Success = executionResult.Success,
                Logs = logs,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds
            };
            
            SetProperty(response, "RawData", enhancedData);
            SetProperty(response, "Result", new ProcessResultDto
            {
                NeedToSend = executionResult.NeedToSend,
                Reason = executionResult.Reason,
                Error = executionResult.ProcessError,
                RequestInfo = executionResult.RequestInfo
            });

            if (!executionResult.Success)
                response.ErrorMessage = executionResult.ErrorMessage;

            return response;
        }

       
        private InterfaceDebugResponse BuildInterfaceProcessorErrorResponse(
            List<DebugLogEntry> logs,
            DateTime startTime,
            long processorExecutionTime,
            Infrastructure.Models.Execution.ExecutionResult executionResult)
        {
            return new InterfaceDebugResponse
            {
                Success = false,
                ErrorMessage = executionResult.ErrorMessage,
                Logs = logs,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                ProcessorExecutionTime = processorExecutionTime,
                ProcessorResult = new ProcessResultDto
                {
                    NeedToSend = executionResult.NeedToSend,
                    Reason = executionResult.Reason,
                    Error = executionResult.ProcessError,
                    RequestInfo = executionResult.RequestInfo
                }
            };
        }

    
        private InterfaceDebugResponse BuildInterfaceNoSendResponse(
            List<DebugLogEntry> logs,
            DateTime startTime,
            long processorExecutionTime,
            Infrastructure.Models.Execution.ExecutionResult executionResult)
        {
            return new InterfaceDebugResponse
            {
                Success = true,
                Logs = logs,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                ProcessorExecutionTime = processorExecutionTime,
                ProcessorResult = new ProcessResultDto
                {
                    NeedToSend = executionResult.NeedToSend,
                    Reason = executionResult.Reason,
                    RequestInfo = executionResult.RequestInfo
                }
            };
        }

       
        private InterfaceDebugResponse BuildInterfaceSuccessResponse(
            List<DebugLogEntry> logs,
            DateTime startTime,
            long processorExecutionTime,
            long interfaceExecutionTime,
            Infrastructure.Models.Execution.ExecutionResult executionResult,
            RequestInfo requestInfo,
            ResponseInfo? responseInfo)
        {
            return new InterfaceDebugResponse
            {
                Success = responseInfo?.IsSuccess ?? false,
                Logs = logs,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                ProcessorExecutionTime = processorExecutionTime,
                InterfaceExecutionTime = interfaceExecutionTime,
                ProcessorResult = new ProcessResultDto
                {
                    NeedToSend = executionResult.NeedToSend,
                    Reason = executionResult.Reason,
                    RequestInfo = executionResult.RequestInfo
                },
                RequestInfo = requestInfo,
                ResponseInfo = responseInfo,
                ErrorMessage = responseInfo == null || responseInfo.IsSuccess ? null : $"HTTP {responseInfo.StatusCode}: {responseInfo.StatusMessage}"
            };
        }

     
        private void SetProperty<T>(T obj, string propertyName, object? value)
        {
            var property = typeof(T).GetProperty(propertyName);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obj, value);
            }
        }

        #endregion
    }
}