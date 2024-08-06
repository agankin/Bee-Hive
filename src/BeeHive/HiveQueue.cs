using System.Collections;

namespace BeeHive;

public class HiveQueue<TRequest, TResult> : IEnumerable<HiveTask<TRequest, TResult>>
{
    private readonly ComputationQueue _poolComputationQueue;
    private readonly ComputationTaskFactory<TRequest, TResult> _computationTaskFactory;
    
    private readonly ConcurrentSet<HiveTask<TRequest, TResult>> _queuedTasks = new();
    private readonly Lazy<HiveResultBag<TRequest, TResult>> _resultBag = new(() => new());

    internal HiveQueue(ComputationQueue poolComputationQueue, Compute<TRequest, TResult> compute)
    {
        _poolComputationQueue = poolComputationQueue;
        _computationTaskFactory = new ComputationTaskFactory<TRequest, TResult>(compute, OnTaskCompleted);
    }

    public HiveTask<TRequest, TResult> Compute(TRequest request)
    {
        var (computation, task) = _computationTaskFactory.Create(request);
        _poolComputationQueue.EnqueueComputation(computation);

        return task;
    }

    public IBlockingReadOnlyCollection<Result<TRequest, TResult>> GetResultBag() => _resultBag.Value;

    public IEnumerator<HiveTask<TRequest, TResult>> GetEnumerator() => _queuedTasks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private void OnTaskCompleted(HiveTask<TRequest, TResult> task, Result<TRequest, TResult> result)
    {
        _queuedTasks.Remove(task);

        if (_resultBag.IsValueCreated)
            _resultBag.Value.Add(result);
    }
}