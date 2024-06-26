using System.Collections.Concurrent;

namespace BeeHive;

public class Hive<TRequest, TResult> : IDisposable
{
    private readonly HiveComputationFactory<TRequest, TResult> _computationFactory;
    private readonly HiveComputationQueue _computationQueue;
    private readonly HiveThreadPool _threadPool;

    private readonly ConcurrentSet<HiveResultCollection<TResult>> _resultCollections = new();

    internal Hive(Compute<TRequest, TResult> compute, HiveConfiguration configuration)
    {
        _computationFactory = new HiveComputationFactory<TRequest, TResult>(compute, OnResult);
        _computationQueue = new HiveComputationQueue(configuration.ThreadIdleBeforeStop);
        _threadPool = new HiveThreadPool(configuration, _computationQueue).Start();
    }

    public HiveTask<TResult> EnqueueTask(TRequest request)
    {
        var (computation, task) = _computationFactory.Create(request);
        _computationQueue.EnqueueComputation(computation);

        return task;
    }

    public Hive<TRequest, TResult> Start()
    {
        _threadPool.Start();
        return this;
    }

    public void Stop() => _threadPool.Stop();

    public void Dispose() => Stop();

    public BlockingCollection<Result<TResult>> CreateBlockingResults()
    {
        var collection = new HiveResultCollection<TResult>(RemoveDisposedCollection);
        _resultCollections.Add(collection);

        return collection;
    }

    private void OnResult(Result<TResult> result) => _resultCollections.ForEach(collection => collection.Add(result));

    private void RemoveDisposedCollection(HiveResultCollection<TResult> collection) => _resultCollections.Remove(collection);
}