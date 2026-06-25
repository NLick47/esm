using EventStreamManager.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly AuthConfig _authConfig;
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
        _authConfig = configuration.GetSection("Auth").Get<AuthConfig>() ?? new AuthConfig();
    }

    /// <summary>
    /// 获取认证状态
    /// </summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { enabled = _authConfig.Enabled });
    }

    /// <summary>
    /// 用户密码登录
    /// </summary>
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (!_authConfig.Enabled)
        {
            return Fail("登录功能未启用");
        }

        if (string.IsNullOrWhiteSpace(request?.Password))
        {
            return Fail("请输入密码");
        }

        if (request.Password != _authConfig.Password)
        {
            return Fail("密码错误");
        }

        var token = TokenHelper.GenerateToken(_authConfig.Password);
        return Ok(new { token }, "登录成功");
    }
}

public class LoginRequest
{
    public string Password { get; set; } = string.Empty;
}
