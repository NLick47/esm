using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using SqlSugar;

namespace EventStreamManager.Infrastructure.Repositories;

public abstract class BaseRepository
{
    private readonly ISqlSugarContext _db;

    protected BaseRepository(ISqlSugarContext db)
    {
        _db = db;
    }

    protected async Task<ISqlSugarClient> GetClientAsync(string databaseType)
    {
        return await _db.GetClientAsync(databaseType);
    }
}
