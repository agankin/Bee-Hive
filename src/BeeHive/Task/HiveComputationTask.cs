namespace BeeHive;

internal record HiveComputationTask<TRequest, TResult>(
    Computation<TRequest, TResult> Computation,
    HiveTask<TRequest, TResult> Task
);