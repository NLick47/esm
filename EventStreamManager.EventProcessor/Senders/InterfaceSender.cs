using System.Diagnostics;
using System.Text;
using EventStreamManager.EventProcessor.Entities;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Interface;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Senders;

/// <summary>
/// 接口发送器
/// </summary>
public class InterfaceSender
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<InterfaceSender> _logger;

    public InterfaceSender(IHttpClientFactory httpFactory, ILogger<InterfaceSender> logger)
    {
        _httpFactory = httpFactory;
        _logger = logger;
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    public async Task<SendResult> SendAsync(string databaseType, InterfaceConfig config, string data)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new SendResult();

        try
        {
            var client = _httpFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(config.Timeout);

            var content = new StringContent(data, Encoding.UTF8, "application/json");

            foreach (var header in config.Headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            HttpResponseMessage response = config.Method.ToUpper() switch
            {
                "GET" => await client.GetAsync(config.Url),
                "PUT" => await client.PutAsync(config.Url, content),
                _ => await client.PostAsync(config.Url, content)
            };

            stopwatch.Stop();
            result.StatusCode = (int)response.StatusCode;
            result.ResponseContent = await response.Content.ReadAsStringAsync();
            result.Success = response.IsSuccessStatusCode;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogInformation("[{DatabaseType}] 接口调用: Url={Url}, Status={Status}, Time={Time}ms",
                databaseType, config.Url, result.StatusCode, result.ExecutionTimeMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "[{DatabaseType}] 接口调用失败: Url={Url}", databaseType, config.Url);
        }

        return result;
    }

    /// <summary>
    /// 带重试的发送
    /// </summary>
    public async Task<SendResult> SendWithRetryAsync(
        string databaseType, InterfaceConfig config, string data, int retries = 3, int intervalMs = 5000)
    {
        SendResult? lastResult = null;

        for (int i = 0; i < retries; i++)
        {
            lastResult = await SendAsync(databaseType, config, data);
            if (lastResult.Success) return lastResult;

            if (i < retries - 1)
            {
                _logger.LogWarning("[{DatabaseType}] 发送失败，{Interval}ms后重试 ({Attempt}/{Max})",
                    databaseType, intervalMs, i + 1, retries);
                await Task.Delay(intervalMs);
            }
        }

        return lastResult ?? new SendResult { Success = false, ErrorMessage = "重试次数已用尽" };
    }
}
