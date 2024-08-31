using System.Runtime.CompilerServices;

namespace BeeHive;

/// <summary>
/// Represents a queued computation to be performed in the Hive.
/// </summary>
/// <typeparam name="TRequest">The request type of the computation.</typeparam>
/// <typeparam name="TResult">The result type of the computation.</typeparam>
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

    /// <summary>
    /// The request task was created for.
    /// </summary>
    public TRequest Request { get; }

    /// <summary>
    /// The current state.
    /// </summary>
    public HiveTaskState State => (HiveTaskState)_state;

    /// <summary>
    /// A canonical Task.
    /// </summary>
    public Task<TResult> Task => _taskCompletionSource.Task;

    internal Action Computation { get; }

    /// <summary>
    /// Returns an awaiter used to await this Hive Task.
    /// </summary>
    public TaskAwaiter<TResult> GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Attempts to cancel this Hive task.
    /// </summary>
    /// <remarks>
    /// Cancels immediately if the computation is in Pending state.
    /// Requests cancellation if the computation is in InProgress state and
    /// in this case the time of actual cancellation depends on whether and how the computation supports cooperative cancellation. 
    /// </remarks>
    /// <returns>
    /// True if cancellation possible - the Hive task is in Pending or InProgress state.
    /// If the task has already completed/failed returns false.
    /// </returns>
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