namespace BeeHive
{
    internal class RandomSchedulingStrategy : ISchedulingStrategy
    {
        public void Schedule(IReadOnlyList<HiveThread> threads, Action computation)
        {
            var idx = new Random().Next(threads.Count);

            threads[idx].Load(computation);
        }
    }
}