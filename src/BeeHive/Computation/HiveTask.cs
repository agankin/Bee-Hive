using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TResult>
{
    private readonly CancellationTokenSource _cancellationTokenSource;

    public HiveTask(Task<Result<TResult>> task, CancellationTokenSource cancellationTokenSource)
    {
        Task = task;
        _cancellationTokenSource = cancellationTokenSource;
    }

    public Task<Result<TResult>> Task { get; }

    public TaskAwaiter<Result<TResult>> GetAwaiter() => Task.GetAwaiter();

    public void Cancel() => _cancellationTokenSource.Cancel();

    public static implicit operator Task<Result<TResult>>(HiveTask<TResult> hiveTask) => hiveTask.Task;
}