using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace EventStreamManager.EventProcessor.Senders;

/// <summary>
/// 接口发送器
/// </summary>
public class InterfaceSender
{
    private readonly IHttpSendService _httpSendService;
    private readonly ILogger<InterfaceSender> _logger;

    public InterfaceSender(IHttpSendService httpSendService, ILogger<InterfaceSender> logger)
    {
        _httpSendService = httpSendService;
        _logger = logger;
    }

    /// <summary>
    /// 发送请求
    /// </summary>
    public Task<SendResult> SendAsync(string databaseType, InterfaceConfig config, string data)
    {
        return _httpSendService.SendAsync(databaseType, config, data);
    }

    /// <summary>
    /// 带重试的发送
    /// </summary>
    public Task<SendResult> SendWithRetryAsync(string databaseType, InterfaceConfig config, string data, 
        int retries = 3, int intervalMs = 5000)
    {
        return _httpSendService.SendWithRetryAsync(databaseType, config, data, retries, intervalMs);
    }
}
