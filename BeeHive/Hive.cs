using System.Collections.Concurrent;

namespace BeeHive
{
    public class Hive
    {
        private readonly ConcurrentDictionary<HiveComputationId, HiveThreadPool> _threadPoolById = new();

        public HiveComputation<TRequest, TResponse> AddComputation<TRequest, TResponse>(
            Func<TRequest, TResponse> compute,
            int maxParallelCount)
        {
            var id = HiveComputationId.Create();
            var schedulingStrategy = new MinLoadSchedulingStrategy();
            var pool = new HiveThreadPool(schedulingStrategy, maxParallelCount);

            _threadPoolById.TryAdd(id, pool);

            return new HiveComputation<TRequest, TResponse>(id, compute, QueueComputation);
        }

        private void QueueComputation(HiveComputationId id, Action compute)
        {
            if (!_threadPoolById.TryGetValue(id, out var pool))
                throw new InvalidOperationException("Hive Thread Pool not found.");

            pool.Load(compute);
        }
    }
}