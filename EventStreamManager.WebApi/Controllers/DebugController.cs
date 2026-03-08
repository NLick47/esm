using EventStreamManager.Infrastructure.Models.Execution.Debug;
using EventStreamManager.Infrastructure.Models.Execution.Parameter;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Services;
using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    private readonly IJavaScriptExecutionService _jsService;
    private readonly ILogger<DebugController> _logger;
    private readonly IProcessorService _processorService;
    private readonly ISqlSugarContext _sqlSugarContext;
    private readonly IDatabaseSchemeService _databaseSchemeService;

    public DebugController(
        IJavaScriptExecutionService jsService,
        ILogger<DebugController> logger,
        IProcessorService processorService,
        IDatabaseSchemeService databaseSchemeService,
        ISqlSugarContext sqlSugarContext)
    {
        _jsService = jsService;
        _logger = logger;
        _processorService = processorService;
        _databaseSchemeService = databaseSchemeService;
        _sqlSugarContext = sqlSugarContext;
    }

    /// <summary>
    /// 编辑器调试执行 - 专门用于Examine事件调试
    /// </summary>
    [HttpPost("execute-examine")]
    public async Task<IActionResult> ExecuteExamineDebug([FromBody] EditorDebugRequest request)
    {
        var startTime = DateTime.Now;
        var logEntries = new List<DebugLogEntry>();

        try
        {
            _logger.LogInformation("开始编辑器调试 - ExamineID: {ExamineId}, 数据库类型: {DatabaseType}",
                request.ExamineId, request.DatabaseType);

            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = $"开始编辑器调试 - ExamineID: {request.ExamineId}",
                Timestamp = DateTime.Now
            });

            JSProcessor? processor = null;
            if (!string.IsNullOrEmpty(request.ProcessorId))
            {
                processor = await _processorService.GetByIdAsync(request.ProcessorId);
                if (processor == null)
                {
                    logEntries.Add(new DebugLogEntry
                    {
                        Type = "error",
                        Message = $"未找到处理器: {request.ProcessorId}",
                        Timestamp = DateTime.Now
                    });

                    return Ok(new EditorDebugResponse
                    {
                        Success = false,
                        ErrorMessage = $"未找到处理器: {request.ProcessorId}",
                        Logs = logEntries,
                        ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                    });
                }
            }

            if (string.IsNullOrEmpty(request.SqlTemplate))
            {
                logEntries.Add(new DebugLogEntry
                {
                    Type = "error",
                    Message = "未配置查询sql",
                    Timestamp = DateTime.Now
                });
                return Ok(new EditorDebugResponse
                {
                    Success = false,
                    ErrorMessage = "未配置查询sql",
                    Logs = logEntries,
                    ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                });
            }

            var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(request.DatabaseType);
            if (activeConfig == null)
            {
                logEntries.Add(new DebugLogEntry
                {
                    Type = "error",
                    Message = $"未找到{request.DatabaseType} 的激活配置方案",
                    Timestamp = DateTime.Now
                });

                return Ok(new EditorDebugResponse
                {
                    Success = false,
                    ErrorMessage = $"未找到{request.DatabaseType} 的激活配置方案",
                    Logs = logEntries,
                    ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                });
            }

            // 使用传入的代码或处理器的代码
            var jsCode = request.JavaScriptCode ?? processor?.Code;
            if (string.IsNullOrEmpty(jsCode))
            {
                logEntries.Add(new DebugLogEntry
                {
                    Type = "error",
                    Message = "JavaScript代码不能为空",
                    Timestamp = DateTime.Now
                });

                return Ok(new EditorDebugResponse
                {
                    Success = false,
                    ErrorMessage = "JavaScript代码不能为空",
                    Logs = logEntries,
                    ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds
                });
            }

            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = $"开始数据查询 - ExamineID: {request.ExamineId}, 数据库类型: {request.DatabaseType}",
                Timestamp = DateTime.Now
            });

            var parameters = new { strexamineId = request.ExamineId };
            var sql = request.SqlTemplate.Replace("${strEventReferenceId}", "@strexamineId");

            var row = await _sqlSugarContext.ExecuteQueryAsync(request.DatabaseType, sql, parameters);

            var enhancedData = new EnhancedQueryData()
            {
                Rows = row,
                Database = new DatabaseInfo()
                {
                    Type = request.DatabaseType,
                },
                Context = new ContextInfo(),
                Processor = new ProcessorInfo()
                {
                    Id = processor == null ? "" : processor.Id,
                    Name = processor == null ? "" : processor.Name,
                    Enabled = processor?.Enabled,
                }
            };
            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = "查询完成",
                Timestamp = DateTime.Now
            });

            // 执行JavaScript代码
            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = "开始执行JavaScript代码",
                Timestamp = DateTime.Now
            });

            var executionResult = await _jsService.ExecuteProcessAsync(jsCode, enhancedData);

            logEntries.Add(new DebugLogEntry
            {
                Type = executionResult.Success ? "success" : "error",
                Message = executionResult.Success
                    ? $"JavaScript代码执行完成，耗时: {executionResult.ExecutionTimeMs}ms"
                    : $"JavaScript代码执行失败: {executionResult.ErrorMessage}",
                Timestamp = DateTime.Now
            });

            // 合并执行过程中的输出日志
            logEntries.AddRange(executionResult.Output.Select(o => new DebugLogEntry
            {
                Type = o.Type,
                Message = o.Message,
                Timestamp = o.Timestamp
            }));

            // 构建响应
            var response = new EditorDebugResponse
            {
                Success = executionResult.Success,
                ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds,
                RawData = enhancedData,
                Logs = logEntries
            };

            // 解析ProcessResult
            if (executionResult.ReturnValue != null)
            {
                response.Result = new ProcessResultDto
                {
                    NeedToSend = executionResult.NeedToSend,
                    Reason = executionResult.Reason,
                    Error = executionResult.ProcessError,
                    RequestInfo = executionResult.RequestInfo
                };
            }

            if (!executionResult.Success)
            {
                response.ErrorMessage = executionResult.ErrorMessage;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "编辑器调试执行失败");

            logEntries.Add(new DebugLogEntry
            {
                Type = "error",
                Message = $"调试执行异常: {ex.Message}",
                Timestamp = DateTime.Now
            });

            return Ok(new EditorDebugResponse
            {
                Success = false,
                ErrorMessage = $"调试执行失败: {ex.Message}",
                Logs = logEntries,
                ExecutionTimeMs = (DateTime.Now - startTime).TotalMilliseconds
            });
        }
    }

    /// <summary>
    /// 调试执行处理器 - 使用真实事件数据
    /// </summary>
    [HttpPost("execute")]
    public async Task<IActionResult> ExecuteDebug([FromBody] DebugRequest request)
    {
        var startTime = DateTime.Now;
        var logEntries = new List<DebugLogEntry>();

        try
        {
            _logger.LogInformation("开始调试处理器: {ProcessorId}, 数据库: {DatabaseType}, 事件ID: {EventId}",
                request.ProcessorId, request.DatabaseType, request.EventId);

           
            var processor = await _processorService.GetByIdAsync(request.ProcessorId);
            if (processor == null)
            {
                return Ok(new DebugResponse
                {
                    Success = false,
                    ErrorMessage = $"未找到处理器: {request.ProcessorId}"
                });
            }

            // 获取数据库配置
            var activeConfig = await _databaseSchemeService.GetActiveConfigAsync(request.DatabaseType);
            if (activeConfig == null)
            {
                return Ok(new DebugResponse
                {
                    Success = false,
                    ErrorMessage = $"未找到数据库类型 {request.DatabaseType} 的激活配置"
                });
            }

            // 查询事件数据
            if (string.IsNullOrEmpty(request.EventId))
            {
                return Ok(new DebugResponse
                {
                    Success = false,
                    ErrorMessage = "事件ID不能为空"
                });
            }

            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = $"查询事件数据: EventId={request.EventId}",
                Timestamp = DateTime.Now
            });

            var client = await _sqlSugarContext.GetClientAsync(request.DatabaseType);
            var eventData = await client.Queryable<EventProcessor.Entities.Event>()
                .Where(e => e.Id.ToString() == request.EventId)
                .FirstAsync();

            if (eventData == null)
            {
                return Ok(new DebugResponse
                {
                    Success = false,
                    ErrorMessage = $"未找到事件: {request.EventId}"
                });
            }

            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = $"事件查询完成: EventCode={eventData.EventCode}, EventName={eventData.EventName}",
                Timestamp = DateTime.Now
            });

            // 查询扩展数据
            List<Dictionary<string, object>> rows = new();
            if (!string.IsNullOrEmpty(processor.SqlTemplate))
            {
                logEntries.Add(new DebugLogEntry
                {
                    Type = "info",
                    Message = "执行SQL模板查询扩展数据",
                    Timestamp = DateTime.Now
                });

                var sql = processor.SqlTemplate
                    .Replace("${strEventReferenceId}", $"'{eventData.StrEventReferenceId}'")
                    .Replace("${eventId}", eventData.Id.ToString())
                    .Replace("${eventType}", $"'{eventData.EventType}'")
                    .Replace("${eventCode}", $"'{eventData.EventCode}'")
                    .Replace("${operatorCode}", $"'{eventData.OperatorCode}'");

                try
                {
                    rows = await _sqlSugarContext.ExecuteQueryAsync(request.DatabaseType, sql);
                    logEntries.Add(new DebugLogEntry
                    {
                        Type = "info",
                        Message = $"扩展数据查询完成，共 {rows.Count} 行",
                        Timestamp = DateTime.Now
                    });
                }
                catch (Exception ex)
                {
                    logEntries.Add(new DebugLogEntry
                    {
                        Type = "warn",
                        Message = $"扩展数据查询失败: {ex.Message}",
                        Timestamp = DateTime.Now
                    });
                }
            }

           
            var enhancedData = new EnhancedQueryData
            {
                Rows = rows,
                Database = new DatabaseInfo
                {
                    Type = request.DatabaseType,
                },
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

           
            logEntries.Add(new DebugLogEntry
            {
                Type = "info",
                Message = "开始执行JavaScript代码",
                Timestamp = DateTime.Now
            });

            var executionResult = await _jsService.ExecuteProcessAsync(processor.Code, enhancedData);

            logEntries.Add(new DebugLogEntry
            {
                Type = executionResult.Success ? "success" : "error",
                Message = executionResult.Success
                    ? $"JavaScript执行完成，耗时: {executionResult.ExecutionTimeMs}ms"
                    : $"JavaScript执行失败: {executionResult.ErrorMessage}",
                Timestamp = DateTime.Now
            });

            // 合并执行过程中的输出日志
            logEntries.AddRange(executionResult.Output.Select(o => new DebugLogEntry
            {
                Type = o.Type,
                Message = o.Message,
                Timestamp = o.Timestamp
            }));
            
            var response = new DebugResponse
            {
                Success = executionResult.Success,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds,
                RawData = enhancedData,
                Logs = logEntries
            };

            if (executionResult.ReturnValue != null)
            {
                response.Result = new ProcessResultDto
                {
                    NeedToSend = executionResult.NeedToSend,
                    Reason = executionResult.Reason,
                    Error = executionResult.ProcessError,
                    RequestInfo = executionResult.RequestInfo
                };
            }

            if (!executionResult.Success)
            {
                response.ErrorMessage = executionResult.ErrorMessage;
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "调试执行失败");

            logEntries.Add(new DebugLogEntry
            {
                Type = "error",
                Message = $"调试执行异常: {ex.Message}",
                Timestamp = DateTime.Now
            });

            return Ok(new DebugResponse
            {
                Success = false,
                ErrorMessage = $"调试执行失败: {ex.Message}",
                Logs = logEntries,
                ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds
            });
        }
    }
}
