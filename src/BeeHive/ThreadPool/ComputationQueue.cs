using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class ComputationQueue
{
    private readonly SemaphoreSlim _semaphore = new(0);
    private readonly ConcurrentQueue<Action> _queue = new();

    public void Enqueue(Action computation)
    {
        _queue.Enqueue(computation);
        _semaphore.Release();
    }

    public bool TryDequeue(Func<bool> canSkipWaitingForNext, CancellationToken cancellationToken, [MaybeNullWhen(false)]out Action next)
    {
        while (!_queue.TryDequeue(out next))
        {
            if (canSkipWaitingForNext() || !WaitForNext(cancellationToken))
                return false;
        }

        return true;
    }

    private bool WaitForNext(CancellationToken cancellationToken)
    {
        try
        {
            _semaphore.Wait(cancellationToken);
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}