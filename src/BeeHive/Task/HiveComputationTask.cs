namespace BeeHive;

internal record HiveComputationTask<TResult>(
    HiveComputation Computation,
    HiveTask<TResult> Task
);