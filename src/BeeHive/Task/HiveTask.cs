using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TRequest, TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource;
    private readonly TaskCancellationTokenSource _taskCancellationTokenSource;
    private readonly Action<HiveTask<TRequest, TResult>> _onCancelled;

    private volatile int _status = (int)HiveTaskStatus.Pending;

    internal HiveTask(
        TRequest request,
        Action computation,
        TaskCompletionSource<TResult> taskCompletionSource,
        TaskCancellationTokenSource taskCancellationTokenSource,
        Action<HiveTask<TRequest, TResult>> onCancelled)
    {
        Request = request;
        Computation = computation;

        _taskCompletionSource = taskCompletionSource;
        _taskCancellationTokenSource = taskCancellationTokenSource;
        _onCancelled = onCancelled;
    }

    public TRequest Request { get; }

    public HiveTaskStatus Status => (HiveTaskStatus)_status;

    public Task<TResult> Task => _taskCompletionSource.Task;

    internal Action Computation { get; }

    public TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();

    public bool Cancel()
    {
        if (TrySetPendingCancelled())
        {
            _taskCancellationTokenSource.Cancel();
            _onCancelled(this);
        }
        else if (TrySetInProgressCancelled())
        {
            _taskCancellationTokenSource.Cancel();
        }

        return Status == HiveTaskStatus.Cancelled;
    }

    public static implicit operator Task<TResult>(HiveTask<TRequest, TResult> hiveTask) => hiveTask._taskCompletionSource.Task;

    internal void Complete(Result<TRequest, TResult> result)
    {
        _status = (int)result.Match(
            _ => HiveTaskStatus.SuccessfullyCompleted,
            _ => HiveTaskStatus.Error,
            () => HiveTaskStatus.Cancelled
        );

        result.Match(
            _taskCompletionSource.SetResult,
            _taskCompletionSource.SetException,
            _taskCompletionSource.SetCanceled
        );
    }
    
    internal bool TrySetInProgress()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.InProgress, (int)HiveTaskStatus.Pending);
        return status == (int)HiveTaskStatus.Pending;
    }

    private bool TrySetPendingCancelled()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.Cancelled, (int)HiveTaskStatus.Pending);
        return status == (int)HiveTaskStatus.Pending;
    }

    private bool TrySetInProgressCancelled()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.Cancelled, (int)HiveTaskStatus.InProgress);
        return status == (int)HiveTaskStatus.InProgress;
    }
}