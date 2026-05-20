using EventStreamManager.Infrastructure.Services.Data.Interfaces;
using System.Collections.Concurrent;

namespace EventStreamManager.Infrastructure.Services.Data;

public class TableInitializationTracker : ITableInitializationTracker
{
    private readonly ConcurrentDictionary<string, byte> _initialized = new();

    public bool TryMarkInitializing(string databaseType)
    {
        return _initialized.TryAdd(databaseType, 0);
    }

    public void MarkFailed(string databaseType)
    {
        _initialized.TryRemove(databaseType, out _);
    }
}
