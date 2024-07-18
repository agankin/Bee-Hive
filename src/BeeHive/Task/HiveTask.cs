using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TRequest, TResult>
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    public HiveTask(TRequest request, Task<Result<TRequest, TResult>> task, CancellationTokenSource cancellationTokenSource)
    {
        Request = request;
        Task = task;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public TRequest Request { get; }

    public Task<Result<TRequest, TResult>> Task { get; }

    public TaskAwaiter<Result<TRequest, TResult>> GetAwaiter() => Task.GetAwaiter();

    public void Cancel() => _cancellationTokenSource.Cancel();

    public static implicit operator Task<Result<TRequest, TResult>>(HiveTask<TRequest, TResult> hiveTask) => hiveTask.Task;
}