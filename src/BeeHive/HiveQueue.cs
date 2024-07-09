using System.Collections.Concurrent;

namespace BeeHive;

public class HiveQueue<TRequest, TResult>
{
    private readonly ComputationQueue _computationQueue;
    private readonly HiveComputationFactory<TRequest, TResult> _computationFactory;
    
    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    internal HiveQueue(ComputationQueue computationQueue, Compute<TRequest, TResult> compute)
    {
        _computationQueue = computationQueue;
        _computationFactory = new HiveComputationFactory<TRequest, TResult>(compute, OnResult);
    }

    public HiveTask<TResult> Compute(TRequest request)
    {
        var (compute, task) = _computationFactory.Create(request);
        _computationQueue.EnqueueComputation(compute);

        return task;
    }

    public BlockingCollection<Result<TResult>> CreateNewResults()
    {
        void RemoveDisposedCollection(HiveResultCollection<TResult> collection) => _resultCollections.Remove(collection);

        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private void OnResult(Result<TResult> result) => _resultCollections.ForEach(collection => collection.Add(result));
}