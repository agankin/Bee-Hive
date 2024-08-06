using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TRequest, TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public HiveTask(TRequest request, TaskCompletionSource<TResult> taskCompletionSource, CancellationTokenSource cancellationTokenSource)
    {
        Request = request;
        _taskCompletionSource = taskCompletionSource;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public TRequest Request { get; }

    public Task<TResult> Task => _taskCompletionSource.Task;

    public TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();

    public void Cancel() => _cancellationTokenSource.Cancel();

    public static implicit operator Task<TResult>(HiveTask<TRequest, TResult> hiveTask) => hiveTask._taskCompletionSource.Task;

    internal void Complete(Result<TRequest, TResult> result)
    {
        result.Match(
            _taskCompletionSource.SetResult,
            _taskCompletionSource.SetException,
            _taskCompletionSource.SetCanceled
        );
    }
}