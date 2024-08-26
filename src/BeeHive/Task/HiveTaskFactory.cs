using System.Runtime.CompilerServices;
using System.Threading;

namespace BeeHive;

internal class HiveTaskFactory<TRequest, TResult>
{
    private readonly Compute<TRequest, TResult> _compute;
    private readonly OnTaskCompleted<TRequest, TResult> _onCompleted;
    private readonly OnTaskCancelled<TRequest, TResult> _onCancelled;
    private readonly CancellationToken _poolCancellationToken;

    internal HiveTaskFactory(
        Compute<TRequest, TResult> compute,
        OnTaskCompleted<TRequest, TResult> onCompleted,
        OnTaskCancelled<TRequest, TResult> onCancelled,
        CancellationToken poolCancellationToken)
    {
        _compute = compute;
        _onCompleted = onCompleted;
        _onCancelled = onCancelled;
        _poolCancellationToken = poolCancellationToken;
    }

    internal HiveTask<TRequest, TResult> Create(TRequest request)
    {
        var taskCompletionSource = new TaskCompletionSource<TResult>(TaskCreationOptions.RunContinuationsAsynchronously);
        var taskCancellationTokenSource = new TaskCancellationTokenSource(_poolCancellationToken);

        HiveTask<TRequest, TResult> hiveTask = null!;
        var computation = () => Compute(hiveTask, taskCancellationTokenSource);
        hiveTask = new HiveTask<TRequest, TResult>(request, computation, taskCompletionSource, taskCancellationTokenSource, OnTaskCancelled);
        
        return hiveTask;
    }

    private void Compute(HiveTask<TRequest, TResult> task, TaskCancellationTokenSource taskCancellationTokenSource)
    {
        if (!task.TrySetInProgress())
            return;

        var taskCancellationToken = taskCancellationTokenSource.Token;
        if (taskCancellationToken.IsCancellationRequested)
        {
            OnCancelled(task);
            return;
        }

        var awaiter = ComputeCore(task.Request, taskCancellationToken).GetAwaiter();
        if (awaiter.IsCompleted)
        {
            OnComputeCompleted(task, taskCancellationTokenSource, awaiter);
        }
        else
        {
            awaiter.OnCompleted(() => OnComputeCompleted(task, taskCancellationTokenSource, awaiter));
        }
    }

    private ValueTask<TResult> ComputeCore(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return _compute(request, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            var task = Task.FromCanceled<TResult>(cancellationToken);
            return new ValueTask<TResult>(task);
        }
        catch (Exception ex)
        {
            var task = Task.FromException<TResult>(ex);
            return new ValueTask<TResult>(task);
        }
    }

    private void OnComputeCompleted(
        HiveTask<TRequest, TResult> task,
        TaskCancellationTokenSource taskCancellationTokenSource,
        ValueTaskAwaiter<TResult> awaiter)
    {
        using var _ = taskCancellationTokenSource;

        if (taskCancellationTokenSource.Token.IsCancellationRequested)
        {
            OnCancelled(task);
            return;
        }

        var request = task.Request;
        
        try
        {
            var resultValue = awaiter.GetResult();
            var result = Result<TRequest, TResult>.FromValue(request, resultValue);
            
            OnCompleted(task, result);
        }
        catch (Exception ex) when (ex is OperationCanceledException or TaskCanceledException)
        {
            OnCancelled(task);
        }
        catch (Exception ex)
        {
            var errorResult = Result<TRequest, TResult>.FromError(request, ex);
            OnCompleted(task, errorResult);
        }
    }

    private void OnCompleted(HiveTask<TRequest, TResult> task, Result<TRequest, TResult> result)
    {        
        task.Complete(result);
        _onCompleted(task, result);
    }

    private void OnCancelled(HiveTask<TRequest, TResult> task)
    {
        var cancelledResult = Result<TRequest, TResult>.Cancelled(task.Request);
        OnCompleted(task, cancelledResult);
    }

    private void OnTaskCancelled(HiveTask<TRequest, TResult> task)
    {        
        var cancelledResult = Result<TRequest, TResult>.Cancelled(task.Request);
        task.Complete(cancelledResult);
        _onCancelled(task);
    }
}