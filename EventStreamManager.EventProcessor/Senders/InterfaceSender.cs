using EventStreamManager.EventProcessor.Interfaces;
using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Interface;
using EventStreamManager.Infrastructure.Services;

namespace EventStreamManager.EventProcessor.Senders;

/// <summary>
/// 接口发送器
/// </summary>
public class InterfaceSender :  IInterfaceSender
{
    private readonly IHttpSendService _httpSendService;

    public InterfaceSender(IHttpSendService httpSendService)
    {
        _httpSendService = httpSendService;
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
