using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal abstract class LiteBlockingCollection<TItem> : IBlockingReadOnlyCollection<TItem>
{
    private readonly SemaphoreSlim _semaphore = new(0);

    public abstract int Count { get; }

    public bool TryTakeOrWait(int waitMilliseconds, CancellationToken cancellationToken, [MaybeNullWhen(false)] out TItem item)
    {
        item = default;
        
        if (!WaitForNext(waitMilliseconds, cancellationToken))
            return false;

        return TryTake(out item);
    }

    protected void SignalNewAdded() => _semaphore.Release();

    protected abstract bool TryTake([MaybeNullWhen(false)] out TItem item);

    private bool WaitForNext(int waitMilliseconds, CancellationToken cancellationToken)
    {
        try
        {
            return _semaphore.Wait(waitMilliseconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    public abstract IEnumerator<TItem> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}