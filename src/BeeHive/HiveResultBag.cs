using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class HiveResultBag<TRequest, TResult> : LiteBlockingCollection<Result<TRequest, TResult>>, IEnumerable<Result<TRequest, TResult>>
{
    private readonly ConcurrentBag<Result<TRequest, TResult>> _resultBag = new();

    public override int Count => _resultBag.Count;

    public void Add(Result<TRequest, TResult> result)
    {
        _resultBag.Add(result);
        SignalNewAdded();
    }

    public override bool TryTake([MaybeNullWhen(false)] out Result<TRequest, TResult> item) => _resultBag.TryTake(out item);

    public override IEnumerator<Result<TRequest, TResult>> GetEnumerator() => _resultBag.GetEnumerator();
}