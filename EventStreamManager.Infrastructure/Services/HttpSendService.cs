using System.Diagnostics;
using System.Text;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Execution.Debug;
using EventStreamManager.Infrastructure.Models.Interface;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.Infrastructure.Services;

public class HttpSendService : IHttpSendService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<HttpSendService> _logger;

    public HttpSendService(IHttpClientFactory httpFactory, ILogger<HttpSendService> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<SendResult> SendAsync(string databaseType, InterfaceConfig config, string data)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SendResult();

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);

            string contentType = "application/json";
            var contentTypeHeader = config.Headers.FirstOrDefault(h => 
                h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase));
            
            if (contentTypeHeader != null && !string.IsNullOrWhiteSpace(contentTypeHeader.Value))
            {
                contentType = contentTypeHeader.Value;
            }
            
            var content = new StringContent(data, Encoding.UTF8, contentType);
            
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(config.Url),
                Method = new HttpMethod(config.Method.ToUpper()),
                Content = config.Method.ToUpper() switch
                {
                    "GET" => null,
                    _ => content
                }
            };
            
            foreach (var header in config.Headers.Where(h => 
                         !string.IsNullOrWhiteSpace(h.Key) && 
                         !h.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)))
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            using var response = await client.SendAsync(request);

            stopwatch.Stop();
            result.StatusCode = (int)response.StatusCode;
            result.Success = response.IsSuccessStatusCode;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            try
            {
                result.ResponseContent = await response.Content.ReadAsStringAsync();
            }
            catch (Exception readEx)
            {
                _logger.LogWarning(readEx, "[{DatabaseType}] 读取响应内容失败: Url={Url}, Status={Status}",
                    databaseType, config.Url, result.StatusCode);
                result.ResponseContent = null;
            }

            if (!result.Success)
            {
                var statusDesc = $"HTTP {(int)response.StatusCode}";
                result.ErrorMessage = result.ResponseContent != null
                    ? $"{statusDesc}: {(result.ResponseContent.Length > 500 ? result.ResponseContent[..500] + "..." : result.ResponseContent)}"
                    : $"{statusDesc}: (无响应内容)";
            }

            _logger.LogInformation("[{DatabaseType}] HTTP发送: Url={Url}, Status={Status}, Time={Time}ms",
                databaseType, config.Url, result.StatusCode, result.ExecutionTimeMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "[{DatabaseType}] HTTP发送失败: Url={Url}", databaseType, config.Url);
        }

        return result;
    }

    public async Task<SendResult> SendWithRetryAsync(string databaseType, InterfaceConfig config, string data, 
        int retries = 3, int intervalMs = 5000)
    {
        if (retries <= 0)
        {
            return new SendResult { Success = false, ErrorMessage = "重试次数为0，未执行发送" };
        }

        SendResult? lastResult = null;

        for (int i = 0; i < retries; i++)
        {
            lastResult = await SendAsync(databaseType, config, data);
            if (lastResult.Success) return lastResult;

            if (i < retries - 1)
            {
                string errorMsg = lastResult.ErrorMessage ?? $"HTTP {lastResult.StatusCode}";
                _logger.LogWarning("[{DatabaseType}] 发送失败({Attempt}/{Max}): {Error}",
                    databaseType, i + 1, retries, errorMsg);
                await Task.Delay(intervalMs);
            }
        }

        return lastResult;
    }

    public async Task<HttpSendDebugInfo> SendWithDebugAsync(string databaseType, InterfaceConfig config, string data)
    {
        var debugInfo = new HttpSendDebugInfo
        {
            RequestInfo = new RequestInfo
            {
                Url = config.Url,
                Method = config.Method,
                Headers = config.Headers.ToDictionary(h => h.Key, h => h.Value),
                Body = data
            }
        };

        var startTime = DateTime.Now;
        debugInfo.Result = await SendAsync(databaseType, config, data);
        debugInfo.ExecutionTimeMs = (long)(DateTime.Now - startTime).TotalMilliseconds;

        return debugInfo;
    }
}