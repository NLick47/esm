namespace EventStreamManager.WebApi.Models.Common;


public class ApiResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 响应码
    /// </summary>
    public int Code { get; set; }
    
    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; }
    
    /// <summary>
    /// 数据
    /// </summary>
    public object? Data { get; set; }

    public ApiResponse()
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    /// <summary>
    /// 成功响应（带数据）
    /// </summary>
    public static ApiResponse Ok(object? data = null, string message = "操作成功")
    {
        return new ApiResponse
        {
            Success = true,
            Code = 200,
            Message = message,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// 成功响应（仅消息）
    /// </summary>
    public static ApiResponse OkMessage(string message = "操作成功")
    {
        return new ApiResponse
        {
            Success = true,
            Code = 200,
            Message = message,
            Data = null,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResponse Fail(string message = "操作失败", int code = 400, object? data = null)
    {
        return new ApiResponse
        {
            Success = false,
            Code = code,
            Message = message,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// 错误响应
    /// </summary>
    public static ApiResponse Error(string message = "服务器内部错误", int code = 500, object? data = null)
    {
        return new ApiResponse
        {
            Success = false,
            Code = code,
            Message = message,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}

public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// 数据
    /// </summary>
    public new T? Data { get; set; }

    /// <summary>
    /// 成功响应（带泛型数据）
    /// </summary>
    public static ApiResponse<T> Ok(T? data = default, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Code = 200,
            Message = message,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// 失败响应
    /// </summary>
    public static ApiResponse<T> Fail(string message = "操作失败", int code = 400, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Code = code,
            Message = message,
            Data = data,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }
}