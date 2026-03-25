using System.ComponentModel.DataAnnotations;

namespace EventStreamManager.WebApi.Models.Requests;

public class ValidateScriptRequest
{
    [Required(ErrorMessage = "脚本代码不能为空")]
    public string Script { get; set; } = string.Empty;
}