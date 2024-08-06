namespace BeeHive;

internal delegate void OnTaskCompleted<TRequest, TResult>(
    HiveTask<TRequest, TResult> task,
    Result<TRequest, TResult> result
);