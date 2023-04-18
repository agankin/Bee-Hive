namespace BeeHive
{
    public static class ComputationConfigurationBuilder
    {
        public static ComputationConfiguration MaxParallelExecution(this ComputationConfiguration config, int maxParallelExecution) =>
            config with { MaxParallelExecution = maxParallelExecution };

        public static ComputationConfiguration WithMinLoadScheduling(this ComputationConfiguration config) =>
            config with { SchedulingStrategy = new MinLoadSchedulingStrategy() };

        public static ComputationConfiguration WithRandomScheduling(this ComputationConfiguration config) =>
            config with { SchedulingStrategy = new RandomSchedulingStrategy() };
    }
}