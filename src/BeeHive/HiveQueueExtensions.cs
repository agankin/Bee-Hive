namespace BeeHive;

/// <summary>
/// Contains extension methods for <see cref="HiveQueue{TRequest, TResult}"/>.
/// </summary>
public static class HiveQueueExtensions
{
    /// <summary>
    /// Returns a Task representing completion of all Hive Tasks in the Hive Queue.
    /// </summary>
    /// <param name="queue">The Hive Queue.</param>
    /// <typeparam name="TRequest">The type of computation request.</typeparam>
    /// <typeparam name="TResult">The type of computation result.</typeparam>
    /// <returns>A Task representing completion of all Hive Tasks in the Queue.</returns>
    public static async Task WhenAll<TRequest, TResult>(this HiveQueue<TRequest, TResult> queue)
    {
        var queueTasks = queue.Select(hiveTask => hiveTask.Task).ToArray();
        if (queueTasks.Length == 0)
            return;

        try
        {
            await Task.WhenAll(queueTasks);
        }
        catch {}
    }
}