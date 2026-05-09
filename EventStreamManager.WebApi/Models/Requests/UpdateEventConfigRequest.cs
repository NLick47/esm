using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 更新事件监听配置请求
/// </summary>
public class UpdateEventConfigRequest
{
    [Range(1, 3600, ErrorMessage = "扫描频率必须在1-3600秒之间")]
    public int ScanFrequency { get; set; } = 60;

    [Range(1, 1000, ErrorMessage = "批次大小必须在1-1000之间")]
    public int BatchSize { get; set; } = 50;

    public bool Enabled { get; set; } = true;

    [Required(ErrorMessage = "事件表名不能为空")]
    [StringLength(100, ErrorMessage = "表名长度不能超过100个字符")]
    public string TableName { get; set; } = "tblevent";

    [Required(ErrorMessage = "主键字段不能为空")]
    [StringLength(100, ErrorMessage = "主键字段长度不能超过100个字符")]
    public string PrimaryKey { get; set; } = "event_id";

    [Required(ErrorMessage = "时间戳字段不能为空")]
    [StringLength(100, ErrorMessage = "时间戳字段长度不能超过100个字符")]
    public string TimestampField { get; set; } = "create_time";

    [Range(0, 10, ErrorMessage = "最大重试次数必须在0-10之间")]
    public int MaxRetryCount { get; set; } = 1;

    public UpdateStartConditionRequest? StartCondition { get; set; }
}
