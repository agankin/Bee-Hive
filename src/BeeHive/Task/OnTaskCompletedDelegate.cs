namespace BeeHive;

internal delegate void OnTaskCompleted<TRequest, TResult>(HiveTask<TRequest, TResult> task);