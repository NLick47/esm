using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EventStreamManager.Infrastructure.Services.Validators.Attributes;
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class SqlTemplateNameAttribute : ValidationAttribute
{
    private readonly int _maxLength;
    private readonly int _minLength;
    
    public SqlTemplateNameAttribute(int minLength = 1, int maxLength = 200, string? errorMessage = null)
    {
        _minLength = minLength;
        _maxLength = maxLength;
        ErrorMessage = errorMessage ?? $"模板名称长度必须在{minLength}-{maxLength}个字符之间";
    }
    
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var name = value as string;
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return new ValidationResult("模板名称不能为空");
        }
        
        if (name.Length < _minLength || name.Length > _maxLength)
        {
            return new ValidationResult(ErrorMessage);
        }
        
        var pattern = new Regex(@"^[\u4e00-\u9fa5a-zA-Z0-9_\-\s]+$");
        if (!pattern.IsMatch(name))
        {
            return new ValidationResult("模板名称只能包含中文字符、字母、数字、下划线、横线和空格");
        }
        
        return ValidationResult.Success;
    }
}