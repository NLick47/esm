using EventStreamManager.Infrastructure.Entities;
using EventStreamManager.Infrastructure.Models.Interface;

namespace EventStreamManager.EventProcessor.Interfaces;

public interface IInterfaceSender
{
    Task<SendResult> SendAsync(string databaseType, InterfaceConfig config, string data);
    Task<SendResult> SendWithRetryAsync(string databaseType, InterfaceConfig config, 
        string data, int retries = 3, int intervalMs = 5000);
}