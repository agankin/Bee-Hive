using System.Collections;

namespace BeeHive;

public class HiveQueue<TRequest, TResult> : IEnumerable<HiveTask<TRequest, TResult>>
{
    private readonly ComputationQueue _poolComputationQueue;
    private readonly HiveTaskFactory<TRequest, TResult> _hiveTaskFactory;
    
    private readonly ConcurrentSet<HiveTask<TRequest, TResult>> _queuedHiveTasks = new();
    private readonly Lazy<HiveResultBag<TRequest, TResult>> _resultBag = new(() => new());

    internal HiveQueue(ComputationQueue poolComputationQueue, Compute<TRequest, TResult> compute, CancellationToken poolCancellationToken)
    {
        _poolComputationQueue = poolComputationQueue;
        _hiveTaskFactory = new HiveTaskFactory<TRequest, TResult>(compute, OnTaskCompleted, OnTaskCancelled, poolCancellationToken);
    }

    public HiveTask<TRequest, TResult> EnqueueCompute(TRequest request)
    {
        var hiveTask = _hiveTaskFactory.Create(request);

        _queuedHiveTasks.Add(hiveTask);
        _poolComputationQueue.EnqueueComputation(hiveTask.Computation);

        return hiveTask;
    }

    public ILiteTakeableCollection<Result<TRequest, TResult>> GetResultBag() => _resultBag.Value;

    public IEnumerator<HiveTask<TRequest, TResult>> GetEnumerator() => _queuedHiveTasks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private void OnTaskCompleted(HiveTask<TRequest, TResult> hiveTask, Result<TRequest, TResult> result)
    {
        _queuedHiveTasks.Remove(hiveTask);

        if (_resultBag.IsValueCreated)
            _resultBag.Value.Add(result);
    }

    private void OnTaskCancelled(HiveTask<TRequest, TResult> hiveTask)
    {
        _queuedHiveTasks.Remove(hiveTask);
        _poolComputationQueue.RemoveComputation(hiveTask.Computation);
    }
}