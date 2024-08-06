namespace BeeHive;

internal record ComputationTask<TRequest, TResult>(
    Action Computation,
    HiveTask<TRequest, TResult> Task
);