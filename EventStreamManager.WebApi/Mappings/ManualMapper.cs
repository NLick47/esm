using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.WebApi.Models.Requests;
using EventStreamManager.WebApi.Models.Responses;

namespace EventStreamManager.WebApi.Mappings;

public static class ManualMapper
{
    public static JsProcessor ToEntity(this ProcessorRequest request)
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
            Description = request.Description
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
            Description = processor.Description
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
            SqlTemplate = processor.SqlTemplate,
            Code = processor.Code
        };
    }

    public static CustomSqlTemplate ToEntity(this CustomSqlTemplateRequest request)
    {
        return new CustomSqlTemplate
        {
            Name = request.Name,
            SqlTemplate = request.SqlTemplate,
        };
    }
}
