using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 创建处理器管道请求
/// </summary>
public class CreatePipelineRequest
{
    [Required(ErrorMessage = "管道名称不能为空")]
    [StringLength(100, ErrorMessage = "管道名称长度不能超过100个字符")]
    public string Name { get; set; } = string.Empty;

    public List<string> EventCodes { get; set; } = new();

    public List<string> DatabaseTypes { get; set; } = new();

    public List<PipelineStageRequest> Stages { get; set; } = new();

    public bool Enabled { get; set; } = true;

    [Range(0, 10, ErrorMessage = "最大重试次数必须在0-10之间")]
    public int MaxRetryCount { get; set; } = 1;
}
