namespace BeeHive;

/// <summary>
/// Contains extension methods for Hive queues.
/// </summary>
public static class HiveQueueExtensions
{
    /// <summary>
    /// Returns a task representing completion of all Hive tasks in the queue.
    /// </summary>
    /// <param name="queue">A Hive queue.</param>
    /// <typeparam name="TRequest">The request type of the computation.</typeparam>
    /// <typeparam name="TResult">The result type of the computation.</typeparam>
    /// <returns>A task representing completion of all Hive tasks in the queue.</returns>
    public static async Task WhenAll<TRequest, TResult>(this HiveQueue<TRequest, TResult> queue)
    {
        var queueTasks = queue.Select(hiveTask => hiveTask.Task).ToArray();
        await Task.WhenAll(queueTasks);
    }
}