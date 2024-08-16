namespace BeeHive;

internal delegate void OnTaskCancelled<TRequest, TResult>(HiveTask<TRequest, TResult> task);