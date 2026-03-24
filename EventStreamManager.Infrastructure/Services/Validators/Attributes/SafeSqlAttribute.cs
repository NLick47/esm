using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace EventStreamManager.Infrastructure.Services.Validators.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class SafeSqlAttribute : ValidationAttribute
{
    private static readonly Regex SqlInjectionPattern = new Regex(
        @"\b(INSERT|UPDATE|DELETE|DROP|ALTER|CREATE|TRUNCATE|GRANT|REVOKE|EXEC|EXECUTE|MERGE|REPLACE)\b|\-\-|;",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    private static readonly Regex DangerousFunctions = new Regex(
        @"\b(xp_cmdshell|sp_executesql|sp_addlogin|sp_dropextendedproc)\b",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    
    private static readonly HashSet<string> AllowedStatements = new(
        new[] { "SELECT", "WITH" }, 
        StringComparer.OrdinalIgnoreCase);
    
    private readonly bool _allowEmpty;
    private readonly string _errorMessage;
    
    public SafeSqlAttribute(bool allowEmpty = false, string? errorMessage = null)
    {
        _allowEmpty = allowEmpty;
        _errorMessage = errorMessage ?? "SQL模板不安全";
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var sql = value as string;
        
        if (string.IsNullOrWhiteSpace(sql))
        {
            if (_allowEmpty)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult(_errorMessage + "：SQL模板不能为空");
        }
        var errors = new List<string>();
        
        if (SqlInjectionPattern.IsMatch(sql))
        {
            errors.Add("SQL包含潜在的危险操作或注入风险");
        }
        if (DangerousFunctions.IsMatch(sql))
        {
            errors.Add("SQL包含不允许的系统函数");
        }
        
        var trimmedSql = sql.TrimStart();
        var firstWord = trimmedSql.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? string.Empty;
            
        if (!AllowedStatements.Contains(firstWord))
        {
            errors.Add($"只允许{string.Join("、", AllowedStatements)}查询语句，当前语句类型: {firstWord}");
        }
        
        
        var placeholderPattern = new Regex(@"\$\{[^}]+\}");
        var matches = placeholderPattern.Matches(sql);
        foreach (Match match in matches)
        {
            if (match.Value.Contains("'") || match.Value.Contains("\"") || match.Value.Contains(";"))
            {
                errors.Add("参数占位符包含非法字符，请使用 ${parameterName} 格式");
                break;
            }
        }
        
        int parentheses = 0;
        foreach (char c in sql)
        {
            if (c == '(') parentheses++;
            if (c == ')') parentheses--;
            if (parentheses < 0)
            {
                errors.Add("SQL括号不匹配");
                break;
            }
        }
        if (parentheses != 0 && !errors.Any(e => e.Contains("括号")))
        {
            errors.Add("SQL括号不匹配");
        }
        if (errors.Any())
        {
            var errorMessage = $"{_errorMessage}：{string.Join("；", errors)}";
            return new ValidationResult(errorMessage);
        }
        
        return ValidationResult.Success;
    }
}