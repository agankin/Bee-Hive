namespace BeeHive;

internal record HiveComputationTask<TResult>(
    Action Compute,
    HiveTask<TResult> Task
);