namespace BeeHive;

internal record ComputationTask<TRequest, TResult>(
    Computation<TRequest, TResult> Computation,
    HiveTask<TRequest, TResult> Task
);