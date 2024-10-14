using System.Collections;

namespace BeeHive;

/// <summary>
/// A Hive Queue containing computations to be run in the Hive.
/// </summary>
/// <typeparam name="TRequest">The type of computation request.</typeparam>
/// <typeparam name="TResult">The type of computation result.</typeparam>
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
    /// Returns the current number of elements in the Queue.
    /// </summary>
    public int Count => _queuedHiveTasks.Count;

    /// <summary>
    /// Adds computation request to the Hive.
    /// </summary>
    /// <param name="request">A request that will be passed to the computation delegate.</param>
    /// <returns>A new instance of <see cref="HiveTask{TRequest, TResult}"/>.</returns>
    public HiveTask<TRequest, TResult> AddRequest(TRequest request)
    {
        var hiveTask = _hiveTaskFactory.Create(request);

        _queuedHiveTasks.Add(hiveTask);
        _poolComputationQueue.EnqueueComputation(hiveTask.Computation);

        return hiveTask;
    }

    /// <summary>
    /// Creates a Hive Result Bag automaticaly populated with completed computations results.
    /// </summary>
    /// <returns>An instance of Hive Result Bag.</returns>
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