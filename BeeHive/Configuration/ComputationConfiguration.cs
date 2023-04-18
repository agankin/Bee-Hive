namespace BeeHive
{
    public record ComputationConfiguration
    {
        private ComputationConfiguration() { }

        public static readonly ComputationConfiguration Default = new ComputationConfiguration
        {
            MaxParallelExecution = 1,
            SchedulingStrategy = new MinLoadSchedulingStrategy(),
        };

        internal int MaxParallelExecution { get; init; }

        internal ISchedulingStrategy SchedulingStrategy { get; init; } = null!;
    }
}