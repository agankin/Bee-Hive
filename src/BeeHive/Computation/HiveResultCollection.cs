using System.Collections.Concurrent;

namespace BeeHive;

public class HiveResultCollection<TResult> : BlockingCollection<TResult>
{
    private readonly Action<HiveResultCollection<TResult>>? _onDisposed;

    internal HiveResultCollection(Action<HiveResultCollection<TResult>>? onDisposed) =>
        _onDisposed = onDisposed;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _onDisposed?.Invoke(this);
    }
}