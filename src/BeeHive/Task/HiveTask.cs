using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TRequest, TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource;
    private readonly Action _onCancelRequested;

    private volatile int _status = (int)HiveTaskStatus.Pending;

    public HiveTask(TRequest request, TaskCompletionSource<TResult> taskCompletionSource, Action onCancelRequested)
    {
        Request = request;
        _taskCompletionSource = taskCompletionSource;
        _onCancelRequested = onCancelRequested;
    }

    public TRequest Request { get; }

    public HiveTaskStatus Status => (HiveTaskStatus)_status;

    public Task<TResult> Task => _taskCompletionSource.Task;

    public TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();

    public void Cancel() => _onCancelRequested();

    public static implicit operator Task<TResult>(HiveTask<TRequest, TResult> hiveTask) => hiveTask._taskCompletionSource.Task;

    internal void Complete(Result<TRequest, TResult> result)
    {
        result.Match(
            _taskCompletionSource.SetResult,
            _taskCompletionSource.SetException,
            _taskCompletionSource.SetCanceled
        );
    }
    
    internal bool TryComplete(Result<TRequest, TResult> result)
    {
        if (!TrySetCompleted(result))
            return false;

        Complete(result);
        return true;
    }

    internal bool TrySetInProgress()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.InProgress, (int)HiveTaskStatus.Pending);
        return status == (int)HiveTaskStatus.Pending;
    }

    internal bool TrySetPendingCancelled()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.Cancelled, (int)HiveTaskStatus.Pending);
        return status == (int)HiveTaskStatus.Pending;
    }

    internal bool TrySetInProgressCancelled()
    {
        var status = Interlocked.CompareExchange(ref _status, (int)HiveTaskStatus.Cancelled, (int)HiveTaskStatus.InProgress);
        return status == (int)HiveTaskStatus.InProgress;
    }

    private bool TrySetCompleted(Result<TRequest, TResult> result)
    {
        var newStatus = (int)result.Match(
            _ => HiveTaskStatus.SuccessfullyCompleted,
            _ => HiveTaskStatus.Error,
            () => HiveTaskStatus.Cancelled
        );

        var status = Interlocked.CompareExchange(ref _status, newStatus, (int)HiveTaskStatus.InProgress);
        return status == (int)HiveTaskStatus.InProgress;
    }
}