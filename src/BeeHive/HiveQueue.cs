using System.Collections;

namespace BeeHive;

/// <summary>
/// A Hive queue containing computations to be run in the Hive.
/// </summary>
/// <typeparam name="TRequest">The request type of the computation.</typeparam>
/// <typeparam name="TResult">The result type of the computation.</typeparam>
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

    /// <summary>
    /// Returns the current number of elements in the queue.
    /// </summary>
    public int Count => _queuedHiveTasks.Count;

    /// <summary>
    /// Enqueues computation to the Hive.
    /// </summary>
    /// <param name="request">A request that will be passed to the computation delegate.</param>
    /// <returns>A Hive task.</returns>
    public HiveTask<TRequest, TResult> EnqueueCompute(TRequest request)
    {
        var hiveTask = _hiveTaskFactory.Create(request);

        _queuedHiveTasks.Add(hiveTask);
        _poolComputationQueue.EnqueueComputation(hiveTask.Computation);

        return hiveTask;
    }

    /// <summary>
    /// Creates a queue result bag.
    /// </summary>
    /// <returns>An instance of Hive queue result bag.</returns>
    public IHiveResultBag<TRequest, TResult> CreateResultBag() => _resultBagCollection.AddNewBag();

    /// <inheritdoc/>
    public IEnumerator<HiveTask<TRequest, TResult>> GetEnumerator() => _queuedHiveTasks.GetEnumerator();

    /// <inheritdoc/>
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