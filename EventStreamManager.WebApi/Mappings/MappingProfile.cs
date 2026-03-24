using AutoMapper;
using EventStreamManager.Infrastructure.Models.JSProcessor;
using EventStreamManager.WebApi.Models.Requests;
using EventStreamManager.WebApi.Models.Responses;

namespace EventStreamManager.WebApi.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ProcessorRequest, JsProcessor>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.SqlTemplate, opt => opt.Ignore());
            
            
            CreateMap<JsProcessor, JsProcessorListResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.DatabaseTypes, opt => opt.MapFrom(src => src.DatabaseTypes))
                .ForMember(dest => dest.EventCodes, opt => opt.MapFrom(src => src.EventCodes))
                .ForMember(dest => dest.SqlTemplateType, opt => opt.MapFrom(src => src.SqlTemplateType))
                .ForMember(dest => dest.SqlTemplateId, opt => opt.MapFrom(src => src.SqlTemplateId))
                .ForMember(dest => dest.Enabled, opt => opt.MapFrom(src => src.Enabled))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description));
            
            CreateMap<JsProcessor, JsProcessorDetailResponse>()
                .IncludeBase<JsProcessor, JsProcessorListResponse>()
                .ForMember(dest => dest.SqlTemplate, opt => opt.MapFrom(src => src.SqlTemplate))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dest => dest.SqlTemplateName, opt => opt.Ignore()); 
            
            CreateMap<CustomSqlTemplateRequest, CustomSqlTemplate>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore());
        }
    }
}