using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal abstract class LiteTakeableCollection<TItem> : ILiteTakeableCollection<TItem>
{
    private readonly LiteSemaphore _semaphore = new();

    public abstract int Count { get; }

    public abstract bool TryTake([MaybeNullWhen(false)] out TItem item);
    
    public bool TryTakeOrWait(int waitMilliseconds, CancellationToken cancellationToken, [MaybeNullWhen(false)] out TItem item)
    {
        item = default;
        
        if (!WaitForNext(waitMilliseconds, cancellationToken))
            return false;

        return TryTake(out item);
    }

    protected void SignalNewAdded() => _semaphore.Release();

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
}