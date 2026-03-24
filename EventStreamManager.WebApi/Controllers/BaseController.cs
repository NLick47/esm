using EventStreamManager.WebApi.Models.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventStreamManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    /// <summary>
    /// 返回成功（带数据）
    /// </summary>
    protected IActionResult Ok(object? data = null, string message = "操作成功")
    {
        return base.Ok(ApiResponse.Ok(data, message));
    }

    /// <summary>
    /// 返回成功（泛型数据）
    /// </summary>
    protected IActionResult Ok<T>(T? data = default, string message = "操作成功")
    {
        return base.Ok(ApiResponse<T>.Ok(data, message));
    }

    /// <summary>
    /// 返回成功（无数据）
    /// </summary>
    protected IActionResult OkMessage(string message = "操作成功")
    {
        return base.Ok(ApiResponse.OkMessage(message));
    }

    /// <summary>
    /// 返回失败
    /// </summary>
    protected IActionResult Fail(string message = "操作失败", int code = 400, object? data = null)
    {
        return base.Ok(ApiResponse.Fail(message, code, data));
    }

    /// <summary>
    /// 返回错误
    /// </summary>
    protected IActionResult Error(string message = "服务器内部错误", int code = 500, object? data = null)
    {
        return base.Ok(ApiResponse.Error(message, code, data));
    }

    /// <summary>
    /// 返回分页数据
    /// </summary>
    protected IActionResult PageData<T>(IEnumerable<T> items, int total, int page, int pageSize, string message = "获取成功")
    {
        var data = new
        {
            items,
            total,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(total / (double)pageSize)
        };
        return Ok(data, message);
    }
}