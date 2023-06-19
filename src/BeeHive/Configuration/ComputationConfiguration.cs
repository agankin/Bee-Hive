namespace BeeHive
{
    public record ComputationConfiguration
    {
        private ComputationConfiguration() { }

        public static readonly ComputationConfiguration Default = new ComputationConfiguration
        {
            MinLiveThreads = 1,
            MaxParallelExecution = 1,
            SchedulingStrategy = new MinLoadSchedulingStrategy(),
        };

        internal int MinLiveThreads { get; init; }

        internal int MaxParallelExecution { get; init; }

        internal ISchedulingStrategy SchedulingStrategy { get; init; } = null!;
    }
}