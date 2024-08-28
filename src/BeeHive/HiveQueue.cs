using System.Collections;

namespace BeeHive;

public class HiveQueue<TRequest, TResult> : IReadOnlyCollection<HiveTask<TRequest, TResult>>
{
    private readonly ComputationQueue _poolComputationQueue;
    private readonly HiveTaskFactory<TRequest, TResult> _hiveTaskFactory;
    
    private readonly ConcurrentSet<HiveTask<TRequest, TResult>> _queuedHiveTasks = new();
    private readonly HiveResultBagCollection<TRequest, TResult> _resultBagCollection = new();

    internal HiveQueue(ComputationQueue poolComputationQueue, Compute<TRequest, TResult> compute, CancellationToken poolCancellationToken)
    {
        _poolComputationQueue = poolComputationQueue;
        _hiveTaskFactory = new HiveTaskFactory<TRequest, TResult>(compute, OnTaskCompleted, OnTaskCancelled, poolCancellationToken);
    }

    public int Count => _queuedHiveTasks.Count;

    public HiveTask<TRequest, TResult> EnqueueCompute(TRequest request)
    {
        var hiveTask = _hiveTaskFactory.Create(request);

        _queuedHiveTasks.Add(hiveTask);
        _poolComputationQueue.EnqueueComputation(hiveTask.Computation);

        return hiveTask;
    }

    public IHiveResultBag<TRequest, TResult> CreateResultBag() => _resultBagCollection.AddNewBag();

    public IEnumerator<HiveTask<TRequest, TResult>> GetEnumerator() => _queuedHiveTasks.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    
    private void OnTaskCompleted(HiveTask<TRequest, TResult> hiveTask, Result<TRequest, TResult> result)
    {
        _queuedHiveTasks.Remove(hiveTask);
        _resultBagCollection.AddResult(result);
    }

    private void OnTaskCancelled(HiveTask<TRequest, TResult> hiveTask)
    {
        _queuedHiveTasks.Remove(hiveTask);
        _poolComputationQueue.RemoveComputation(hiveTask.Computation);
    }
}