using System.Collections.Concurrent;

namespace BeeHive;

public class Hive<TRequest, TResult> : IDisposable
{
    private readonly HiveComputationFactory<TRequest, TResult> _computationFactory;
    private readonly ComputationQueue _computationQueue;
    private readonly HiveThreadPool _threadPool;

    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    internal Hive(Compute<TRequest, TResult> compute, HiveConfiguration configuration)
    {
        _computationFactory = new HiveComputationFactory<TRequest, TResult>(compute, OnResult);
        _computationQueue = new ComputationQueue(configuration.ThreadIdleBeforeStop);
        _threadPool = new HiveThreadPool(configuration, _computationQueue);
    }

    public Hive<TRequest, TResult> Run()
    {
        _threadPool.Run();
        return this;
    }

    public HiveTask<TResult> Compute(TRequest request)
    {
        var (compute, task) = _computationFactory.Create(request);
        _computationQueue.EnqueueComputation(compute);

        return task;
    }

    public void Dispose() => _threadPool.Dispose();

    public BlockingCollection<Result<TResult>> CreateNewResults()
    {
        void RemoveDisposedCollection(HiveResultCollection<TResult> collection) => _resultCollections.Remove(collection);

        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private void OnResult(Result<TResult> result) => _resultCollections.ForEach(collection => collection.Add(result));
}