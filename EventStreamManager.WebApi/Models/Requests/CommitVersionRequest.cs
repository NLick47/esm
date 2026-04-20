using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

public class CommitVersionRequest
{
    [Required(ErrorMessage = "提交信息不能为空")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "提交信息长度必须在1-200之间")]
    public string CommitMessage { get; set; } = string.Empty;
}
