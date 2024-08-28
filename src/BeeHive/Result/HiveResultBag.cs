using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class HiveResultBag<TRequest, TResult> : LiteTakeableCollection<Result<TRequest, TResult>>, IHiveResultBag<TRequest, TResult>
{
    private readonly ConcurrentBag<Result<TRequest, TResult>> _resultBag = new();
    private readonly Action<HiveResultBag<TRequest, TResult>> _onDisposed;

    private int _disposed;

    public HiveResultBag(Action<HiveResultBag<TRequest, TResult>> onDisposed)
    {
        _onDisposed = onDisposed;
    }

    public override int Count => _resultBag.Count;

    public void Add(Result<TRequest, TResult> result)
    {
        _resultBag.Add(result);
        SignalNewAdded();
    }

    public IEnumerator<Result<TRequest, TResult>> GetEnumerator() => _resultBag.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Dispose()
    {
        if (Interlocked.CompareExchange(ref _disposed, 1, 0) == 0)
            _onDisposed(this);
    }

    protected override bool TryTakeCore([MaybeNullWhen(false)] out Result<TRequest, TResult> item) => _resultBag.TryTake(out item);
}