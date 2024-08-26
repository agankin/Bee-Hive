using System.Runtime.CompilerServices;

namespace BeeHive;

public class HiveTask<TRequest, TResult>
{
    private readonly TaskCompletionSource<TResult> _taskCompletionSource;
    private readonly TaskCancellationTokenSource _taskCancellationTokenSource;
    private readonly Action<HiveTask<TRequest, TResult>> _onCancelled;

    private volatile int _state = (int)HiveTaskState.Pending;

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

    public HiveTaskState State => (HiveTaskState)_state;

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

        return State == HiveTaskState.Cancelled;
    }

    public static implicit operator Task<TResult>(HiveTask<TRequest, TResult> hiveTask) => hiveTask._taskCompletionSource.Task;

    internal void Complete(Result<TRequest, TResult> result)
    {
        _state = (int)result.Match(
            _ => HiveTaskState.SuccessfullyCompleted,
            _ => HiveTaskState.Error,
            () => HiveTaskState.Cancelled
        );

        result.Match(
            _taskCompletionSource.SetResult,
            _taskCompletionSource.SetException,
            _taskCompletionSource.SetCanceled
        );
    }
    
    internal bool TrySetInProgress()
    {
        var state = Interlocked.CompareExchange(ref _state, (int)HiveTaskState.InProgress, (int)HiveTaskState.Pending);
        return state == (int)HiveTaskState.Pending;
    }

    private bool TrySetPendingCancelled()
    {
        var state = Interlocked.CompareExchange(ref _state, (int)HiveTaskState.Cancelled, (int)HiveTaskState.Pending);
        return state == (int)HiveTaskState.Pending;
    }

    private bool TrySetInProgressCancelled()
    {
        var state = Interlocked.CompareExchange(ref _state, (int)HiveTaskState.Cancelled, (int)HiveTaskState.InProgress);
        return state == (int)HiveTaskState.InProgress;
    }
}