namespace EventStreamManager.WebApi.Models.Responses;

public class JsProcessorDetailResponse : JsProcessorListResponse
{
    public string SqlTemplate { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
    
    public string? SqlTemplateName { get; set; }
    
    public DateTime? CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
}