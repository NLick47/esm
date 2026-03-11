using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Execution.Debug;
using EventStreamManager.Infrastructure.Models.Interface;

namespace EventStreamManager.Infrastructure.Services;

public interface IHttpSendService
{
    Task<SendResult> SendAsync(string databaseType, InterfaceConfig config, string data);
    Task<SendResult> SendWithRetryAsync(string databaseType, InterfaceConfig config, string data, 
        int retries = 3, int intervalMs = 5000);
    
    
    Task<HttpSendDebugInfo> SendWithDebugAsync(string databaseType, InterfaceConfig config, string data);
}