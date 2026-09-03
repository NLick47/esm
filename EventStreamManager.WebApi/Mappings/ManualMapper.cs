using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.DataBase;
using EventStreamManager.Infrastructure.Models.EventListener;
using EventStreamManager.Infrastructure.Models.EventLog;
using EventStreamManager.Infrastructure.Models.Execution.Debug;
using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.Infrastructure.Models.SystemVariable;
using EventStreamManager.WebApi.Models.Requests;
using EventStreamManager.WebApi.Models.Responses;

namespace EventStreamManager.WebApi.Mappings;

public static class ManualMapper
{
    #region Processor (Request → Entity)

    public static JsProcessor ToEntity(this CreateProcessorRequest request)
    {
        return new JsProcessor
        {
            Name = request.Name,
            DatabaseTypes = request.DatabaseTypes,
            EventCodes = request.EventCodes,
            SqlTemplateType = request.SqlTemplateType,
            SqlTemplateId = request.SqlTemplateId,
            Code = request.Code,
            Enabled = request.Enabled,
            Description = request.Description,
            SortOrder = request.SortOrder
        };
    }

    public static JsProcessor ToEntity(this UpdateProcessorRequest request)
    {
        return new JsProcessor
        {
            Name = request.Name,
            DatabaseTypes = request.DatabaseTypes,
            EventCodes = request.EventCodes,
            SqlTemplateType = request.SqlTemplateType,
            SqlTemplateId = request.SqlTemplateId,
            Code = request.Code,
            Enabled = request.Enabled,
            Description = request.Description,
            SortOrder = request.SortOrder
        };
    }

    public static JsProcessorListResponse ToListResponse(this JsProcessor processor)
    {
        return new JsProcessorListResponse
        {
            Id = processor.Id,
            Name = processor.Name,
            DatabaseTypes = processor.DatabaseTypes,
            EventCodes = processor.EventCodes,
            SqlTemplateType = processor.SqlTemplateType,
            SqlTemplateId = processor.SqlTemplateId,
            Enabled = processor.Enabled,
            Description = processor.Description,
            SortOrder = processor.SortOrder
        };
    }

    public static JsProcessorDetailResponse ToDetailResponse(this JsProcessor processor)
    {
        return new JsProcessorDetailResponse
        {
            Id = processor.Id,
            Name = processor.Name,
            DatabaseTypes = processor.DatabaseTypes,
            EventCodes = processor.EventCodes,
            SqlTemplateType = processor.SqlTemplateType,
            SqlTemplateId = processor.SqlTemplateId,
            Enabled = processor.Enabled,
            Description = processor.Description,
            SortOrder = processor.SortOrder,
            SqlTemplate = processor.SqlTemplate,
            Code = processor.Code
        };
    }

    #endregion

    #region CustomSqlTemplate (Request → Entity)

    public static CustomSqlTemplate ToEntity(this CreateCustomSqlTemplateRequest request)
    {
        return new CustomSqlTemplate
        {
            Name = request.Name,
            SqlTemplate = request.SqlTemplate,
        };
    }

    public static CustomSqlTemplate ToEntity(this UpdateCustomSqlTemplateRequest request)
    {
        return new CustomSqlTemplate
        {
            Name = request.Name,
            SqlTemplate = request.SqlTemplate,
        };
    }

    #endregion

    #region DatabaseConfig (Request → Entity)

    public static DatabaseConfig ToEntity(this CreateDatabaseConfigRequest request)
    {
        return new DatabaseConfig
        {
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            Driver = request.Driver,
            IsActive = false,
            Timeout = request.Timeout
        };
    }

    public static DatabaseConfig ToEntity(this UpdateDatabaseConfigRequest request, string id)
    {
        return new DatabaseConfig
        {
            Id = id,
            Name = request.Name,
            ConnectionString = request.ConnectionString,
            Driver = request.Driver,
            IsActive = request.IsActive,
            Timeout = request.Timeout
        };
    }

    #endregion

    #region DatabaseType (Request → Entity)

    public static DatabaseType ToEntity(this CreateDatabaseTypeRequest request)
    {
        return new DatabaseType
        {
            Value = request.Value,
            Label = request.Label
        };
    }

    #endregion

    #region EventConfig (Request → Entity)

    public static EventConfig ToEntity(this UpdateEventConfigRequest request)
    {
        return new EventConfig
        {
            ScanFrequency = request.ScanFrequency,
            BatchSize = request.BatchSize,
            Enabled = request.Enabled,
            TableName = request.TableName,
            PrimaryKey = request.PrimaryKey,
            TimestampField = request.TimestampField,
            MaxRetryCount = request.MaxRetryCount,
            StartCondition = request.StartCondition?.ToEntity()
        };
    }

    #endregion

    #region StartCondition (Request → Entity)

    public static StartCondition ToEntity(this UpdateStartConditionRequest request)
    {
        return new StartCondition
        {
            Type = request.Type,
            TimeValue = request.TimeValue,
            IdValue = request.IdValue
        };
    }

    #endregion

    #region InterfaceConfig (Request → Entity)

    public static InterfaceConfig ToEntity(this CreateInterfaceConfigRequest request)
    {
        return new InterfaceConfig
        {
            Name = request.Name,
            ProcessorIds = request.ProcessorIds,
            ProcessorNames = request.ProcessorNames,
            Url = request.Url,
            Method = request.Method,
            Headers = request.Headers.ToEntities(),
            Timeout = request.Timeout,
            RetryCount = request.RetryCount,
            RetryInterval = request.RetryInterval,
            Enabled = request.Enabled,
            RequestTemplate = request.RequestTemplate,
            Description = request.Description
        };
    }

    public static InterfaceConfig ToEntity(this UpdateInterfaceConfigRequest request, string id)
    {
        return new InterfaceConfig
        {
            Id = id,
            Name = request.Name,
            ProcessorIds = request.ProcessorIds,
            ProcessorNames = request.ProcessorNames,
            Url = request.Url,
            Method = request.Method,
            Headers = request.Headers.ToEntities(),
            Timeout = request.Timeout,
            RetryCount = request.RetryCount,
            RetryInterval = request.RetryInterval,
            Enabled = request.Enabled,
            RequestTemplate = request.RequestTemplate,
            Description = request.Description
        };
    }

    #endregion

    #region HeaderItem (Request → Entity)

    public static HeaderItem ToEntity(this HeaderItemRequest request)
    {
        return new HeaderItem
        {
            Key = request.Key,
            Value = request.Value
        };
    }

    public static List<HeaderItem> ToEntities(this List<HeaderItemRequest>? requests)
    {
        return requests?.Select(r => r.ToEntity()).ToList() ?? new List<HeaderItem>();
    }

    #endregion

    #region RollbackOptions (Request → Entity)

    public static RollbackOptions ToEntity(this RollbackVersionRequest request)
    {
        return new RollbackOptions
        {
            RestoreCode = request.RestoreCode,
            RestoreSqlTemplate = request.RestoreSqlTemplate,
            RestoreEventCodes = request.RestoreEventCodes,
            RestoreDatabaseTypes = request.RestoreDatabaseTypes,
            RestoreMetadata = request.RestoreMetadata
        };
    }

    #endregion

    #region Debug (WebApi Request → Infrastructure DTO)

    public static ConnectionTestRequest ToInfrastructure(this TestConnectionRequest request)
    {
        return new ConnectionTestRequest
        {
            ConnectionString = request.ConnectionString,
            Driver = request.Driver
        };
    }

    public static DebugRequest ToInfrastructure(this ExecuteDebugRequest request)
    {
        return new DebugRequest
        {
            ProcessorId = request.ProcessorId,
            DatabaseType = request.DatabaseType,
            EventCode = request.EventCode,
            EventId = request.EventId
        };
    }

    public static EditorDebugRequest ToInfrastructure(this ExecuteExamineDebugRequest request)
    {
        return new EditorDebugRequest
        {
            ProcessorId = request.ProcessorId,
            JavaScriptCode = request.JavaScriptCode,
            ExamineId = request.ExamineId,
            DatabaseType = request.DatabaseType,
            SqlTemplate = request.SqlTemplate,
            ValidateCode = request.ValidateCode
        };
    }

    public static InterfaceDebugRequest ToInfrastructure(this DebugInterfaceRequest request)
    {
        return new InterfaceDebugRequest
        {
            InterfaceConfigId = request.InterfaceConfigId,
            ProcessorId = request.ProcessorId,
            DatabaseType = request.DatabaseType,
            EventCode = request.EventCode,
            EventId = request.EventId
        };
    }

    #endregion

    #region DatabaseConfig (Entity → Response)

    public static DatabaseConfigResponse ToResponse(this DatabaseConfig config)
    {
        return new DatabaseConfigResponse
        {
            Id = config.Id,
            Name = config.Name,
            ConnectionString = config.ConnectionString,
            Driver = config.Driver.ToString(),
            IsActive = config.IsActive,
            Timeout = config.Timeout
        };
    }

    public static List<DatabaseConfigResponse> ToResponses(this List<DatabaseConfig> configs)
    {
        return configs.Select(c => c.ToResponse()).ToList();
    }

    public static Dictionary<string, List<DatabaseConfigResponse>> ToResponseMap(this Dictionary<string, List<DatabaseConfig>> dict)
    {
        return dict.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToResponses()
        );
    }

    #endregion

    #region DatabaseType (Entity → Response)

    public static DatabaseTypeResponse ToResponse(this DatabaseType type)
    {
        return new DatabaseTypeResponse
        {
            Value = type.Value,
            Label = type.Label
        };
    }

    public static List<DatabaseTypeResponse> ToResponses(this List<DatabaseType> types)
    {
        return types.Select(t => t.ToResponse()).ToList();
    }

    #endregion

    #region DatabaseTypeWithActiveConfigDto (Entity → Response)

    public static DatabaseTypeWithActiveConfigResponse ToResponse(this DatabaseTypeWithActiveConfigDto dto)
    {
        return new DatabaseTypeWithActiveConfigResponse
        {
            Value = dto.Value,
            Label = dto.Label,
            ActiveConfig = dto.ActiveConfig?.ToResponse()
        };
    }

    public static List<DatabaseTypeWithActiveConfigResponse> ToResponses(this List<DatabaseTypeWithActiveConfigDto> dtos)
    {
        return dtos.Select(d => d.ToResponse()).ToList();
    }

    #endregion

    #region ConnectionTestResponse (Infrastructure → WebApi)

    public static Models.Responses.ConnectionTestResponse ToResponse(this Infrastructure.Models.DataBase.ConnectionTestResponse response)
    {
        return new Models.Responses.ConnectionTestResponse
        {
            Success = response.Success,
            Message = response.Message,
            ResponseTime = response.ResponseTime,
            DatabaseVersion = response.DatabaseVersion
        };
    }

    #endregion

    #region EventConfig (Entity → Response)

    public static EventConfigResponse ToResponse(this EventConfig config)
    {
        return new EventConfigResponse
        {
            ScanFrequency = config.ScanFrequency,
            BatchSize = config.BatchSize,
            Enabled = config.Enabled,
            TableName = config.TableName,
            PrimaryKey = config.PrimaryKey,
            TimestampField = config.TimestampField,
            TotalEventsProcessed = config.TotalEventsProcessed,
            MaxRetryCount = config.MaxRetryCount,
            StartCondition = config.StartCondition?.ToResponse()
        };
    }

    #endregion

    #region StartCondition (Entity → Response)

    public static StartConditionResponse ToResponse(this StartCondition condition)
    {
        return new StartConditionResponse
        {
            Type = condition.Type,
            TimeValue = condition.TimeValue,
            IdValue = condition.IdValue
        };
    }

    #endregion

    #region EventListenerConfigs (Entity → Response)

    public static EventListenerConfigsResponse ToResponse(this EventListenerConfigs configs)
    {
        return new EventListenerConfigsResponse
        {
            Databases = configs.Databases.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.ToResponse()
            ),
            LastUpdated = configs.LastUpdated,
            Version = configs.Version
        };
    }

    #endregion

    #region EventHandleResult (Entity → Response)

    public static EventHandleResponse ToResponse(this EventHandleResult result)
    {
        return new EventHandleResponse
        {
            Id = result.Id,
            EventId = result.EventId,
            EventCode = result.EventCode,
            ProcessorId = result.ProcessorId,
            ProcessorName = result.ProcessorName,
            HandleTimes = result.HandleTimes,
            LastHandleStatus = result.LastHandleStatus,
            LastHandleMessage = result.LastHandleMessage,
            LastHandleDatetime = result.LastHandleDatetime,
            LastHandleElapsedMs = result.LastHandleElapsedMs,
            StrEventReferenceId = result.StrEventReferenceId,
            NeedToSend = result.NeedToSend,
            Reason = result.Reason,
            ScriptSuccess = result.ScriptSuccess,
            SendSuccess = result.SendSuccess,
            IsDeadLetter = result.IsDeadLetter,
            RequestData = result.RequestData,
            ResponseData = result.ResponseData,
            IsFinished = result.IsFinished,
            CreateDatetime = result.CreateDatetime,
            EventName = result.EventName
        };
    }

    public static List<EventHandleResponse> ToResponses(this List<EventHandleResult> results)
    {
        return results.Select(r => r.ToResponse()).ToList();
    }

    public static EventHandleLogDetailResponse? ToResponse(this EventHandleLogDetail? detail)
    {
        if (detail == null) return null;
        return new EventHandleLogDetailResponse
        {
            LogId = detail.LogId,
            ErrorStack = detail.ErrorStack,
            ConsoleOutput = detail.ConsoleOutput,
            ErrorLineNumber = detail.ErrorLineNumber,
            ErrorColumn = detail.ErrorColumn,
            ErrorJavaScriptStackTrace = detail.ErrorJavaScriptStackTrace,
            ErrorSourceContext = detail.ErrorSourceContext,
            ScriptSnapshot = detail.ScriptSnapshot,
            InputDataSnapshot = detail.InputDataSnapshot
        };
    }

    public static EventHandleDetailResponse WithDetail(this EventHandleResponse response, EventHandleLogDetailResponse? detail)
    {
        return new EventHandleDetailResponse
        {
            Id = response.Id,
            EventId = response.EventId,
            EventCode = response.EventCode,
            ProcessorId = response.ProcessorId,
            ProcessorName = response.ProcessorName,
            HandleTimes = response.HandleTimes,
            LastHandleStatus = response.LastHandleStatus,
            LastHandleMessage = response.LastHandleMessage,
            LastHandleDatetime = response.LastHandleDatetime,
            LastHandleElapsedMs = response.LastHandleElapsedMs,
            StrEventReferenceId = response.StrEventReferenceId,
            NeedToSend = response.NeedToSend,
            Reason = response.Reason,
            ScriptSuccess = response.ScriptSuccess,
            SendSuccess = response.SendSuccess,
            IsDeadLetter = response.IsDeadLetter,
            RequestData = response.RequestData,
            ResponseData = response.ResponseData,
            IsFinished = response.IsFinished,
            CreateDatetime = response.CreateDatetime,
            EventName = response.EventName,
            Detail = detail
        };
    }

    #endregion

    #region InterfaceConfig (Entity → Response)

    public static InterfaceConfigResponse ToResponse(this InterfaceConfig config)
    {
        return new InterfaceConfigResponse
        {
            Id = config.Id,
            Name = config.Name,
            ProcessorIds = config.ProcessorIds,
            ProcessorNames = config.ProcessorNames,
            Url = config.Url,
            Method = config.Method,
            Headers = config.Headers.ToResponses(),
            Timeout = config.Timeout,
            RetryCount = config.RetryCount,
            RetryInterval = config.RetryInterval,
            Enabled = config.Enabled,
            RequestTemplate = config.RequestTemplate,
            Description = config.Description
        };
    }

    public static List<InterfaceConfigResponse> ToResponses(this List<InterfaceConfig> configs)
    {
        return configs.Select(c => c.ToResponse()).ToList();
    }

    #endregion

    #region HeaderItem (Entity → Response)

    public static HeaderItemResponse ToResponse(this HeaderItem item)
    {
        return new HeaderItemResponse
        {
            Key = item.Key,
            Value = item.Value
        };
    }

    public static List<HeaderItemResponse> ToResponses(this List<HeaderItem>? items)
    {
        return items?.Select(i => i.ToResponse()).ToList() ?? new List<HeaderItemResponse>();
    }

    #endregion

    #region AvailableProcessor (Entity → Response)

    public static AvailableProcessorResponse ToResponse(this AvailableProcessor processor)
    {
        return new AvailableProcessorResponse
        {
            Id = processor.Id,
            Name = processor.Name
        };
    }

    public static List<AvailableProcessorResponse> ToResponses(this List<AvailableProcessor> processors)
    {
        return processors.Select(p => p.ToResponse()).ToList();
    }

    #endregion

    #region ProcessorReferenceStatus (Entity → Response)

    public static ProcessorReferenceStatusResponse ToResponse(this ProcessorReferenceStatus status)
    {
        return new ProcessorReferenceStatusResponse
        {
            Id = status.Id,
            Name = status.Name,
            IsReferenced = status.IsReferenced,
            ReferencedByConfigId = status.ReferencedByConfigId,
            ReferencedByConfigName = status.ReferencedByConfigName
        };
    }

    public static List<ProcessorReferenceStatusResponse> ToResponses(this List<ProcessorReferenceStatus> statuses)
    {
        return statuses.Select(s => s.ToResponse()).ToList();
    }

    #endregion

    #region JsProcessorVersion (Entity → Response)

    public static ProcessorVersionResponse ToResponse(this JsProcessorVersion version)
    {
        return new ProcessorVersionResponse
        {
            Id = version.Id,
            ProcessorId = version.ProcessorId,
            Version = version.Version,
            CommitMessage = version.CommitMessage,
            Name = version.Name,
            DatabaseTypes = version.DatabaseTypes,
            EventCodes = version.EventCodes,
            Code = version.Code,
            SqlTemplate = version.SqlTemplate,
            SqlTemplateId = version.SqlTemplateId,
            SqlTemplateType = version.SqlTemplateType.ToString(),
            SqlTemplateName = version.SqlTemplateName,
            Enabled = version.Enabled,
            Description = version.Description,
            CreatedAt = version.CreatedAt
        };
    }

    public static List<ProcessorVersionResponse> ToResponses(this List<JsProcessorVersion> versions)
    {
        return versions.Select(v => v.ToResponse()).ToList();
    }

    #endregion

    #region RollbackResult (Entity → Response)

    public static RollbackResultResponse ToResponse(this RollbackResult result)
    {
        return new RollbackResultResponse
        {
            Version = result.Version.ToResponse(),
            RecoveredTemplates = result.RecoveredTemplates,
            MissingEventCodes = result.MissingEventCodes,
            HasWarnings = result.HasWarnings
        };
    }

    #endregion

    #region SystemSqlTemplate (Entity → Response)

    public static SystemSqlTemplateResponse ToResponse(this SystemSqlTemplate template)
    {
        return new SystemSqlTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            EventCodes = template.EventCodes,
            SqlTemplate = template.SqlTemplate
        };
    }

    public static List<SystemSqlTemplateResponse> ToResponses(this List<SystemSqlTemplate> templates)
    {
        return templates.Select(t => t.ToResponse()).ToList();
    }

    #endregion

    #region CustomSqlTemplate (Entity → Response)

    public static CustomSqlTemplateResponse ToResponse(this CustomSqlTemplate template)
    {
        return new CustomSqlTemplateResponse
        {
            Id = template.Id,
            Name = template.Name,
            SqlTemplate = template.SqlTemplate,
            CreatedAt = template.CreatedAt,
            UpdatedAt = template.UpdatedAt
        };
    }

    public static List<CustomSqlTemplateResponse> ToResponses(this List<CustomSqlTemplate> templates)
    {
        return templates.Select(t => t.ToResponse()).ToList();
    }

    #endregion

    #region SystemVariable (Entity → Response)

    public static SystemVariableResponse ToResponse(this SystemVariable variable)
    {
        return new SystemVariableResponse
        {
            Id = variable.Id,
            Key = variable.Key,
            Value = variable.Value,
            Description = variable.Description,
            Category = variable.Category,
            CreatedAt = variable.CreatedAt,
            UpdatedAt = variable.UpdatedAt
        };
    }

    public static List<SystemVariableResponse> ToResponses(this List<SystemVariable> variables)
    {
        return variables.Select(v => v.ToResponse()).ToList();
    }

    #endregion

    #region EventCode (Entity → Response)

    public static EventCodeResponse ToResponse(this EventCode eventCode)
    {
        return new EventCodeResponse
        {
            Code = eventCode.Code,
            Description = eventCode.Description,
            Enabled = eventCode.Enabled
        };
    }

    public static List<EventCodeResponse> ToResponses(this List<EventCode> eventCodes)
    {
        return eventCodes.Select(e => e.ToResponse()).ToList();
    }

    #endregion

    #region ValidationResult (Runtime → WebApi Response)

    public static ScriptValidationResponse ToResponse(this JSFunction.Runtime.ValidationResult result)
    {
        return new ScriptValidationResponse
        {
            IsValid = result.IsValid,
            Message = result.Message,
            LineNumber = result.LineNumber,
            Column = result.Column,
            Source = result.Source,
            HasProcessFunction = result.HasProcessFunction
        };
    }

    #endregion

    #region Debug (Infrastructure → WebApi Response)

    public static DebugLogEntryResponse ToResponse(this DebugLogEntry entry)
    {
        return new DebugLogEntryResponse
        {
            Type = entry.Type,
            Message = entry.Message,
            Timestamp = entry.Timestamp
        };
    }

    public static List<DebugLogEntryResponse> ToResponses(this List<DebugLogEntry> entries)
    {
        return entries.Select(e => e.ToResponse()).ToList();
    }

    public static ProcessResultResponse ToResponse(this ProcessResultDto? result)
    {
        if (result == null) return null!;
        return new ProcessResultResponse
        {
            NeedToSend = result.NeedToSend,
            Reason = result.Reason,
            Error = result.Error,
            RequestInfo = result.RequestInfo
        };
    }

    public static RequestInfoResponse ToResponse(this RequestInfo? info)
    {
        if (info == null) return null!;
        return new RequestInfoResponse
        {
            Url = info.Url,
            Method = info.Method,
            Headers = info.Headers,
            Body = info.Body
        };
    }

    public static ResponseInfoResponse ToResponse(this ResponseInfo? info)
    {
        if (info == null) return null!;
        return new ResponseInfoResponse
        {
            StatusCode = info.StatusCode,
            StatusMessage = info.StatusMessage,
            Body = info.Body,
            IsSuccess = info.IsSuccess
        };
    }

    public static CodeValidationResponse ToResponse(this CodeValidationResult? result)
    {
        if (result == null) return null!;
        return new CodeValidationResponse
        {
            HasProcessFunction = result.HasProcessFunction,
            SyntaxValid = result.SyntaxValid,
            Warnings = result.Warnings,
            Errors = result.Errors
        };
    }

    public static DebugExecuteResponse ToResponse(this DebugResponse response)
    {
        return new DebugExecuteResponse
        {
            Success = response.Success,
            Logs = response.Logs.ToResponses(),
            Result = response.Result.ToResponse(),
            ExecutionTimeMs = response.ExecutionTimeMs,
            ErrorMessage = response.ErrorMessage,
            RawData = response.RawData
        };
    }

    public static ExamineDebugResponse ToResponse(this EditorDebugResponse response)
    {
        return new ExamineDebugResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage,
            ExecutionTimeMs = response.ExecutionTimeMs,
            RawData = response.RawData,
            Result = response.Result.ToResponse(),
            Logs = response.Logs.ToResponses(),
            CodeValidation = response.CodeValidation.ToResponse()
        };
    }

    public static DebugInterfaceResponse ToResponse(this InterfaceDebugResponse response)
    {
        return new DebugInterfaceResponse
        {
            Success = response.Success,
            ErrorMessage = response.ErrorMessage,
            Logs = response.Logs.ToResponses(),
            ExecutionTimeMs = response.ExecutionTimeMs,
            ProcessorExecutionTime = response.ProcessorExecutionTime,
            InterfaceExecutionTime = response.InterfaceExecutionTime,
            ProcessorResult = response.ProcessorResult.ToResponse(),
            RequestInfo = response.RequestInfo.ToResponse(),
            ResponseInfo = response.ResponseInfo.ToResponse()
        };
    }

    #endregion
}
