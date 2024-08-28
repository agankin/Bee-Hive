namespace BeeHive;

public static class HiveQueueExtensions
{
    public static async Task WhenAll<TRequest, TResult>(this HiveQueue<TRequest, TResult> queue)
    {
        var queueTasks = queue.Select(hiveTask => hiveTask.Task).ToArray();
        await Task.WhenAll(queueTasks);
    }
}