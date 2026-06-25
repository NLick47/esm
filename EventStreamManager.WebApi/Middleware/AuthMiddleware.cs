using System.Net;
using System.Text.Json;
using EventStreamManager.WebApi.Models.Common;

namespace EventStreamManager.WebApi.Middleware;

/// <summary>
/// 认证中间件 - 当 Auth.Enabled 为 true 时验证 token
/// </summary>
public class AuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthMiddleware> _logger;

    public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
    {
        var authConfig = configuration.GetSection("Auth").Get<AuthConfig>() ?? new AuthConfig();

        // 未启用认证，直接放行
        if (!authConfig.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? "";

        // 公开接口白名单：登录、认证状态、静态文件、前端路由
        if (IsPublicPath(path))
        {
            await _next(context);
            return;
        }

        // 验证 Token
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            await WriteUnauthorized(context, "未提供有效的认证凭据");
            return;
        }

        var token = authHeader["Bearer ".Length..];
        if (!TokenHelper.ValidateToken(token, authConfig.Password))
        {
            await WriteUnauthorized(context, "认证凭据无效或已过期");
            return;
        }

        await _next(context);
    }

    private static bool IsPublicPath(string path)
    {
        var lowerPath = path.ToLowerInvariant();
        return lowerPath.StartsWith("/api/auth/login")
            || lowerPath.StartsWith("/api/auth/status")
            || lowerPath.StartsWith("/api/eventprocessor/version")
            || lowerPath.StartsWith("/assets")
            || lowerPath.StartsWith("/favicon")
            || lowerPath == "/"
            || lowerPath == ""
            || lowerPath.Contains(".js")
            || lowerPath.Contains(".css")
            || lowerPath.Contains(".html")
            || lowerPath.Contains(".woff")
            || lowerPath.Contains(".ttf")
            || lowerPath.Contains(".ico")
            || lowerPath.Contains(".png")
            || lowerPath.Contains(".svg");
    }

    private static async Task WriteUnauthorized(HttpContext context, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.OK;

        var response = ApiResponse.Fail(message, 401);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
