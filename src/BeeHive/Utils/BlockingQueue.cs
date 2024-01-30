using System.Collections;
using System.Collections.Concurrent;

namespace BeeHive;

internal class BlockingQueue<TItem> : IEnumerable<TItem>
{
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly ConcurrentQueue<TItem> _queue = new();

    private readonly int _waitForNextMilliseconds;

    public BlockingQueue(int waitForNextMilliseconds) => _waitForNextMilliseconds = waitForNextMilliseconds;

    public int Count => _queue.Count;
    
    public void Enqueue(TItem item)
    {
        OnEnqueueing(item);
        
        _queue.Enqueue(item);
        _semaphore.Release();
    }

    public DequeueResult<TItem> DequeueOrWait(Func<bool> canSkipWaitingForNext, CancellationToken cancellationToken)
    {
        if (_queue.TryDequeue(out var next))
            return new(true, next);

        while (true)
        {
            if (!WaitForNext(cancellationToken) && canSkipWaitingForNext())
                return new(false, default);

            if (_queue.TryDequeue(out next))
                return new(true, next);
        }
    }

    public IEnumerator<TItem> GetEnumerator() => _queue.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _queue.GetEnumerator();

    private bool WaitForNext(CancellationToken cancellationToken)
    {
        try
        {
            return _semaphore.Wait(_waitForNextMilliseconds, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    protected virtual void OnEnqueueing(TItem item)
    {
    }
}

public readonly record struct DequeueResult<TItem>(
    bool HasItem,
    TItem? Item
);