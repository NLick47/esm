using EventStreamManager.Infrastructure.Models.DataBase;

namespace EventStreamManager.Infrastructure.Services.Data.Interfaces;

public interface  IDatabaseConnectionService
{
    Task<ConnectionTestResponse> TestConnectionAsync(ConnectionTestRequest request);
}