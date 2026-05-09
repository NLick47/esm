using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

/// <summary>
/// 更新监听起始条件请求
/// </summary>
public class UpdateStartConditionRequest
{
    [Required(ErrorMessage = "条件类型不能为空")]
    [RegularExpression("^(time|id)$", ErrorMessage = "条件类型必须是 time 或 id")]
    public string Type { get; set; } = "time";

    public string TimeValue { get; set; } = DateTime.Now.AddDays(-1).ToString("yyyy-MM-ddTHH:mm");

    public string IdValue { get; set; } = string.Empty;
}
