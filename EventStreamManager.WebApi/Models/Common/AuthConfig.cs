namespace EventStreamManager.WebApi.Models.Common;

public class AuthConfig
{
    /// <summary>
    /// 是否启用登录认证
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    /// 登录密码
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
