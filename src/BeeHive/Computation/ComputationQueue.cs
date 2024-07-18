using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace BeeHive;

internal class ComputationQueue : LiteBlockingCollection<Action>
{    
    private readonly ConcurrentQueue<Action> _computationQueue = new();
    private readonly ConcurrentQueue<Action> _continuationQueue = new();

    public event Action? Enqueueing;

    public override int Count => _computationQueue.Count + _continuationQueue.Count;

    public void EnqueueComputation(Action compute) => EnqueueCore(_computationQueue, compute);

    public void EnqueueContinuation(Action continuation) => EnqueueCore(_continuationQueue, continuation);

    private void EnqueueCore(ConcurrentQueue<Action> queue, Action item)
    {
        Enqueueing?.Invoke();
        queue.Enqueue(item);
        
        SignalNewAdded();
    }

    public override IEnumerator<Action> GetEnumerator() => new AggregativeEnumerator<Action>(
        _computationQueue.GetEnumerator(),
        _continuationQueue.GetEnumerator());

    protected override bool TryTake([MaybeNullWhen(false)] out Action action)
    {
        action = null;
        
        if (_continuationQueue.TryDequeue(out var nextContinuation))
        {
            action = nextContinuation;
            return true;
        }

        if (_computationQueue.TryDequeue(out var nextComputation))
        {
            action = nextComputation;
            return true;
        }

        return false;
    }
}