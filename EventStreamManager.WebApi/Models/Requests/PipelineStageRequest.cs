using System.ComponentModel.DataAnnotations;
using EventStreamManager.Infrastructure.Models.JSProcessor;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 管道阶段请求
/// </summary>
public class PipelineStageRequest
{
    [Required(ErrorMessage = "处理器ID不能为空")]
    public string ProcessorId { get; set; } = string.Empty;

    [Required(ErrorMessage = "处理器名称不能为空")]
    public string ProcessorName { get; set; } = string.Empty;

    public int SortOrder { get; set; } = 0;

    public bool IsSender { get; set; } = false;

    public StageFailureAction OnFailure { get; set; } = StageFailureAction.Stop;

    public string? Condition { get; set; }
}
