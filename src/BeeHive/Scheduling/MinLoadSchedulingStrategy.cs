namespace BeeHive;

internal class MinLoadSchedulingStrategy : ISchedulingStrategy
{
    public void Schedule(IReadOnlyList<HiveThread> threads, Action computation)
    {
        threads.OrderBy(thread => thread.QueuedCount).First().Load(computation);
    }
}